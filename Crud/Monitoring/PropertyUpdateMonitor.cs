using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Juulsgaard.Crud.Monitoring;

/// <inheritdoc/>
public class PropertyUpdateMonitor<TModel, TProp> : IPropertyUpdateMonitor<TModel> where TModel : class
{
	private IStateSnapshot<TProp>? _oldSnapshot;
	private IStateSnapshot<TProp>? _newSnapshot;
	
	private readonly Expression<Func<TModel, TProp>> _expression;
	private readonly DbContext _dbContext;

	public PropertyUpdateMonitor(Expression<Func<TModel, TProp>> selector, DbContext dbContext)
	{
		_expression = selector;
		_dbContext = dbContext;
		Selector = selector.Compile();
	}

	/// <inheritdoc/>
	public bool Changed { get; set; }

	/// <summary>
	/// A selector to evaluate the prop
	/// </summary>
	public Func<TModel, TProp> Selector { get; }

	public TProp OldValue => _oldSnapshot != null ? _oldSnapshot.GetValue() : throw new ArgumentException("Tried to access unset old value");
	public TProp NewValue => _newSnapshot != null ? _newSnapshot.GetValue() : throw new ArgumentException("Tried to access unset new value");

	/// <inheritdoc/>
	public void UpdateNew(TModel model)
	{
		if (_oldSnapshot == null) {
			throw new Exception("You cannot set the UpdateMonitor 'New State' before the 'Old State'");
		}
		
		var prop = Selector(model);
		var entityType = _dbContext.Model.FindEntityType(typeof(TProp));
		_newSnapshot = entityType == null 
			? new ValueStateSnapshot<TProp>(prop) 
			: new EntityStateSnapshot<TProp>(prop, entityType);

		Changed = !_newSnapshot.Compare(_oldSnapshot);
	}

	/// <inheritdoc/>
	public void UpdateOld(TModel model)
	{
		var prop = Selector(model);
		var entityType = _dbContext.Model.FindEntityType(typeof(TProp));
		_oldSnapshot = entityType == null 
			? new ValueStateSnapshot<TProp>(prop) 
			: new EntityStateSnapshot<TProp>(prop, entityType);
	}

	/// <inheritdoc/>
	public Expression<Func<TModel, bool>> HasNewValueExpression()
	{
		return Expression.Lambda<Func<TModel, bool>>(
			Expression.Equal(
				_expression.Body,
				Expression.Constant(NewValue, typeof(TProp))
			),
			_expression.Parameters[0]
		);
	}

	/// <inheritdoc/>
	public Expression<Func<TModel, bool>> HasOldValueExpression()
	{
		return Expression.Lambda<Func<TModel, bool>>(
			Expression.Equal(
				_expression.Body,
				Expression.Constant(OldValue, typeof(TProp))
			),
			_expression.Parameters[0]
		);
	}
}