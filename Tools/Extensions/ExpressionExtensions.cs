using System.Linq.Expressions;

namespace Juulsgaard.Tools.Extensions;

public static class ExpressionExtensions
{
	public static Expression<T> ReplaceParameter<T>(this Expression<T> exp, ParameterExpression target, Expression replacement)
	{
		var replacer = new ParameterReplacerVisitor(target, replacement);
		return replacer.Replace(exp);
	} 
	
	public static Expression ReplaceParameter(this Expression exp, ParameterExpression target, Expression replacement)
	{
		var replacer = new ParameterReplacerVisitor(target, replacement);
		return replacer.Replace(exp);
	} 

	#region Expression Builders

	public static Expression<Func<T, bool>> OrExpression<T>(this Expression<Func<T, bool>>? baseExp, Expression<Func<T, bool>> exp)
	{
		if (baseExp is null) return exp;
		var or = Expression.OrElse(baseExp.Body, exp.Body.ReplaceParameter(exp.Parameters[0], baseExp.Parameters[0]));
		return Expression.Lambda<Func<T, bool>>(or, baseExp.Parameters[0]);
	}
	
	public static Expression<Func<T, bool>> AndExpression<T>(this Expression<Func<T, bool>>? baseExp, Expression<Func<T, bool>> exp)
	{
		if (baseExp is null) return exp;
		var or = Expression.AndAlso(baseExp.Body, exp.Body.ReplaceParameter(exp.Parameters[0], baseExp.Parameters[0]));
		return Expression.Lambda<Func<T, bool>>(or, baseExp.Parameters[0]);
	}

	#endregion
}

file class ParameterReplacerVisitor : ExpressionVisitor
{
	private readonly ParameterExpression _target;
	private readonly Expression _value;

	public ParameterReplacerVisitor(ParameterExpression target, Expression value)
	{
		_target = target;
		_value = value;
	}
	
	public Expression<T> Replace<T>(Expression<T> root)
	{
		return (Expression<T>)VisitLambda(root);
	}
	
	public Expression Replace(Expression root)
	{
		return Visit(root);
	}

	protected override Expression VisitLambda<T>(Expression<T> node)
	{
		// Leave all parameters alone except the one we want to replace.
		var parameters = node.Parameters
		   .Where(p => p != _target);

		return Expression.Lambda<T>(Visit(node.Body), parameters);
	}

	protected override Expression VisitParameter(ParameterExpression node)
	{
		// Replace the target with the new value, visit other params as usual.
		return node == _target ? _value : base.VisitParameter(node);
	}
	
	
}