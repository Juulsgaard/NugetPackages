using System.Linq.Expressions;

namespace Crud.Extensions;

public static class ExpressionExtensions
{
	public static Func<T, Expression<Func<T, bool>>> ToIdentifier<T, TProp>(this Expression<Func<T, TProp>> selector)
	{
		return model => {
			var value = selector.Compile()(model);
			var constant = Expression.Constant(value, typeof(TProp));
			var comparison = Expression.Equal(selector.Body, constant);
			return Expression.Lambda<Func<T, bool>>(comparison, selector.Parameters[0]);
		};
	}

	public static Func<TProp, Expression<Func<T, bool>>> ToValueIdentifier<T, TProp>(this Expression<Func<T, TProp>> selector)
	{
		return value => {
			var constant = Expression.Constant(value, typeof(TProp));
			var comparison = Expression.Equal(selector.Body, constant);
			return Expression.Lambda<Func<T, bool>>(comparison, selector.Parameters[0]);
		};
	}
}