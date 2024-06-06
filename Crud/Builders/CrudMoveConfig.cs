using System.Linq.Expressions;
using AutoMapper;
using Juulsgaard.Crud.Domain.Interfaces;
using Juulsgaard.Crud.Extensions;
using Juulsgaard.Crud.Models;
using Juulsgaard.Crud.Transactions;
using Juulsgaard.Tools.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Juulsgaard.Crud.Builders;

/// <summary>
/// This is the config for moving an Entity
/// </summary>
/// <typeparam name="TModel">The tracked Model</typeparam>
/// <remarks>Moving is defined by updating indices to create a new order of elements</remarks>
public class CrudMoveConfig<TModel> where TModel : class
{
	protected internal CrudTargetConfig<TModel> Target { get; }
	protected internal int Index { get; protected set; }
	protected internal Func<TModel, Expression<Func<TModel, bool>>>? SubSetIdentifier { get; set; }
		
	protected internal DbContext Context => Target.Context;
	protected internal IMapper? Mapper => Target.Mapper;
	protected CrudExceptionLookup? ExceptionLookup;
	
	private bool _safeMove = false;


	/// <summary>
	/// Manually instantiate a <see cref="CrudMoveConfig{TModel}"/>
	/// </summary>
	/// <param name="target">The target to move</param>
	/// <param name="index">The new index of the moved model</param>
	public CrudMoveConfig(CrudTargetConfig<TModel> target, int index)
	{
		Target = target;
		Index = index;
	}


	/// <summary>
	/// Specify an identifier for a subset of indexed items
	/// </summary>
	/// <param name="identifier">A delegate that return <c>true</c> if the two items are in the same subset</param>
	/// <returns>The current Config</returns>
	/// <remarks>
	/// <para>It is recommended to use the shorthand for this method <see cref="WithIdentifier{TProp}"/> whenever possible!</para>
	/// <para>The <paramref name="identifier"/> compares two properties that categorizes a subset of items with their own order</para>
	/// </remarks>
	/// <example>
	///     Update the order of dogs that belong to the same owner
	///     <code>
	///         Context.Dogs
	///             .Filter(x => x.Name == "Fido")
	///             .Move(2)
	///             .WithIdentifier(x => y => x.Owner == y.Owner)
	///             .ExecuteAsync()
	///     </code>
	/// </example>
	public CrudMoveConfig<TModel> WithIdentifier(Func<TModel, Expression<Func<TModel, bool>>> identifier)
	{
		SubSetIdentifier = identifier;
		return this;
	}

	/// <summary>
	/// Specify an identifier for a subset of indexed items
	/// </summary>
	/// <param name="selector">A delegate that returns the property by which to identify a subset of indices</param>
	/// <typeparam name="TProp">The type of the grouping prop</typeparam>
	/// <returns>The current Config</returns>
	/// <remarks>The <paramref name="selector"/>'s target is a property that categorizes a subset of items with their own order</remarks>
	/// <example>
	///     Update the order of dogs that belong to the same owner
	///     <code>
	///         Context.Dogs
	///             .Filter(x => x.Name == "Fido")
	///             .Move(2)
	///             .WithIdentifier(x => x.Owner)
	///             .ExecuteAsync()
	///     </code>
	/// </example>
	public CrudMoveConfig<TModel> WithIdentifier<TProp>(Expression<Func<TModel, TProp>> selector)
	{
		SubSetIdentifier = model => {
			var value = selector.Compile()(model);
			var constant = Expression.Constant(value, typeof(TProp));
			var comparison = Expression.Equal(selector.Body, constant);
			return Expression.Lambda<Func<TModel, bool>>(comparison, selector.Parameters[0]);
		};
		return this;
	}

	/// <summary>
	/// This is a noop, since a move action is always saved
	/// </summary>
	/// <returns>The current Config</returns>
	public CrudMoveConfig<TModel> Save()
	{
		return this;
	}
	
	/// <summary>
	/// Change the implementation so a Unique Index on the Index column isn't violated
	/// </summary>
	/// <returns>The current Config</returns>
	public CrudMoveConfig<TModel> SafeMove()
	{
		_safeMove = true;
		return this;
	}
	
	/// <summary>
	/// Define Exception text overrides for different Exception types
	/// </summary>
	/// <param name="default">Default exception text - Used for all undefined exceptions</param>
	/// <param name="concurrency">Exception text for concurrency errors</param>
	/// <param name="notFound">Exception text for not found errors</param>
	/// <returns>The current Config</returns>
	public CrudMoveConfig<TModel> WithExceptions(
		string? @default = null,
		string? concurrency = null,
		string? notFound = null
	)
	{
		ExceptionLookup = new() {
			Concurrency = concurrency ?? @default,
			Default = @default
		};
		
		notFound ??= @default;
		if (notFound != null) Target.WithException(notFound);
		return this;
	}

	/// <summary>
	/// Executes the Move operation
	/// </summary>
	/// <returns>The moved model</returns>
	/// <remarks>This will always save as it's a multi part operation, that requires intermediary save operations</remarks>
	public async Task<TModel> ExecuteAsync()
	{
		var model = await Target.FirstAsync();

		if (model is not ISorted indexModel) return model;

		var oldIndex = indexModel.Index;
		
		if (Index < 0) {
			Index = -1;
		}

		if (Index == oldIndex || Index == oldIndex + 1) return model;

		var query = Target.Query;

		if (SubSetIdentifier != null)
		{
			query = query.Where(SubSetIdentifier(model));
		}

		if (query is not IQueryable<ISorted> tempQuery) return model;
		
		if (Target.Testing) {
			await ExecuteNoSql(tempQuery, indexModel, oldIndex);
			return model;
		}
		
		if (Index > 0) {
			var max = await tempQuery
				.OrderByDescending(x => x.Index)
				.Select(x => x.Index + 1)
				.FirstOrDefaultAsync();

			if (Index > max + 1) {
				Index = max + 1;
			}
		}
		
		await using var trx = await Context.BeginInnerTransactionAsync();
		
		// Remove from sorted list
		if (Index == -1) {

			indexModel.Index = -1;
			await SaveAndHandleErrors();

			await tempQuery.Where(x => x.Index > oldIndex)
				.ExecuteUpdateAsync(c => c.SetProperty(x => x.Index, x => x.Index - 1));

			await trx.CommitAsync();
			return model;
		} 
		
		// Add to sorted list
		if (oldIndex == -1) {
			
			await tempQuery.Where(x => x.Index >= Index)
				.ExecuteUpdateAsync(c => c.SetProperty(x => x.Index, x => x.Index + 1));
				
			indexModel.Index = Index;
			await SaveAndHandleErrors();

			await trx.CommitAsync();
			return model;
		} 

		if (_safeMove) {
			await ExecuteSafe(tempQuery, indexModel, oldIndex);
			await trx.CommitAsync();
			return model;
		}

		await ExecuteDefault(tempQuery, indexModel, oldIndex);
		await trx.CommitAsync();
		return model;
	}

	private async Task ExecuteDefault(IQueryable<ISorted> tempQuery, ISorted indexModel, int oldIndex)
	{
		
		if (Index > oldIndex) { // Move to higher index
			
			// Remove elements down into the hole left by the promoted element
			await tempQuery.Where(x => x.Index > oldIndex)
				.Where(x => x.Index < Index)
				.ExecuteUpdateAsync(c => c.SetProperty(x => x.Index, x => x.Index - 1));
				
			indexModel.Index = Index - 1;
			await SaveAndHandleErrors();
			
			return;
			
		}
		
		// Move to lower index
		await tempQuery.Where(x => x.Index >= Index)
			.Where(x => x.Index < oldIndex)
			.ExecuteUpdateAsync(c => c.SetProperty(x => x.Index, x => x.Index + 1));

		indexModel.Index = Index;
		await SaveAndHandleErrors();
			
	}

	private async Task ExecuteSafe(IQueryable<ISorted> tempQuery, ISorted indexModel, int oldIndex)
	{
		await tempQuery.Where(x => x.Index >= Index)
		   .ExecuteUpdateAsync(c => c.SetProperty(x => x.Index, x => x.Index + 1));

		indexModel.Index = Index;

		await SaveAndHandleErrors();

		await tempQuery.Where(x => x.Index > oldIndex)
		   .ExecuteUpdateAsync(c => c.SetProperty(x => x.Index, x => x.Index - 1));

		if (Index > oldIndex) {
			indexModel.Index--;
			await SaveAndHandleErrors();
		}
	}
	
	private async Task ExecuteNoSql(IQueryable<ISorted> tempQuery, ISorted indexModel, int oldIndex)
	{
		await using var trx = await Context.BeginInnerTransactionAsync();
		
		var items = await tempQuery
		   .Where(x => x.Index >= Index || x.Index > oldIndex)
		   .ToListAsync();

		var max = items.Count == 0 ? 0 : items.Max(x => x.Index);

		if (Index > max + 1) {
			Index = max + 1;
		}
		
		// Remove from sorted list
		if (Index == -1) {

			indexModel.Index = -1;

			var above = items.Where(x => x.Index > oldIndex).ToList();
			above.ForEach(x => x.Index--);

			await SaveAndHandleErrors();
			await trx.CommitAsync();
			return;
		} 
		
		// Add to sorted list
		if (oldIndex == -1) {
			
			var above = items.Where(x => x.Index >= Index).ToList();
			above.ForEach(x => x.Index++);
				
			indexModel.Index = Index;

			await SaveAndHandleErrors();
			await trx.CommitAsync();
			return;
		} 

		var itemsAbove = items.Where(x => x.Index >= Index).ToList();
		itemsAbove.ForEach(x => x.Index++);

		await SaveAndHandleErrors();

		indexModel.Index = Index;

		await SaveAndHandleErrors();

		var itemsAboveOld = items.Where(x => x.Index > oldIndex).ToList();
			
		foreach (var item in itemsAboveOld) {
			item.Index--;
		}

		if (Index > oldIndex) {
			indexModel.Index--;
		}

		await SaveAndHandleErrors();

		await trx.CommitAsync();
	}
	
	private async Task SaveAndHandleErrors()
	{
		try {
			await Context.SaveChangesAsync();
		}
		catch (DbUpdateException e) {
			throw e.ProcessAsMove(Target.EntityName, ExceptionLookup);
		}
	}

	/// <summary>
	/// Executes the Move operation, ans maps the moved model to <typeparamref name="TMap"/>
	/// </summary>
	/// <typeparam name="TMap">The model to map the moved item to</typeparam>
	/// <returns>The moved and mapped model</returns>
	/// <remarks>This will always save as it's a multi part operation, that required intermediary save operations</remarks>
	public async Task<TMap> ExecuteAndMapAsync<TMap>()
	{
		if (Mapper == null) {
			throw new InternalException("DbContext does not expose IMapper");
		}
			
		var model = await ExecuteAsync();
		return Mapper.Map<TMap>(model);
	}
}