using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Crud.Monitoring;

public class ListUpdateMonitor<TModel, TProp> : IUpdateMonitor<TModel> where TModel : class
{
	private IStateSnapshot<List<TProp>>? _oldSnapshot;
	private IStateSnapshot<List<TProp>>? _newSnapshot;
	
	private readonly DbContext _dbContext;

	public ListUpdateMonitor(Expression<Func<TModel, IEnumerable<TProp>>> selector, DbContext dbContext)
	{
		_dbContext = dbContext;
		Selector = selector.Compile();
	}

	/// <inheritdoc/>
	public bool Changed { get; set; }

	/// <summary>
	/// A selector to evaluate the prop
	/// </summary>
	public Func<TModel, IEnumerable<TProp>> Selector { get; }

	public List<TProp> OldValue => _oldSnapshot != null ? _oldSnapshot.GetValue() : throw new ArgumentException("Tried to access unset old value");
	public List<TProp> NewValue => _newSnapshot != null ? _newSnapshot.GetValue() : throw new ArgumentException("Tried to access unset new value");

	/// <inheritdoc/>
	public void UpdateNew(TModel model)
	{
		if (_oldSnapshot == null) {
			throw new Exception("You cannot set the UpdateMonitor 'New State' before the 'Old State'");
		}
		
		var prop = Selector(model);
		var entityType = _dbContext.Model.FindEntityType(typeof(TProp));
		_newSnapshot = new ListStateSnapshot<TProp>(prop, entityType);

		Changed = !_newSnapshot.Compare(_oldSnapshot);
	}

	/// <inheritdoc/>
	public void UpdateOld(TModel model)
	{
		var prop = Selector(model);
		var entityType = _dbContext.Model.FindEntityType(typeof(TProp));
		_oldSnapshot =  new ListStateSnapshot<TProp>(prop, entityType);
	}
}