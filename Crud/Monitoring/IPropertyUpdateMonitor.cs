using System.Linq.Expressions;

namespace Juulsgaard.Crud.Monitoring;

public interface IPropertyUpdateMonitor<TModel> : IUpdateMonitor<TModel>
{
	/// <summary>
	/// Creates an expression that matches elements with the new value
	/// </summary>
	/// <returns>An expression</returns>
	Expression<Func<TModel, bool>> HasNewValueExpression();

	/// <summary>
	/// Creates an expression that matches elements with the old value
	/// </summary>
	/// <returns>An expression</returns>
	Expression<Func<TModel, bool>> HasOldValueExpression();
}