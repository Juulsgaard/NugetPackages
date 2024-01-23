using System.Linq.Expressions;
using Lib.Exceptions;

namespace Crud.Helpers;

public static class CrudErrorHelper {
		
	public static NotFoundException GenerateError<TInput>(Expression<Func<TInput, bool>>? filter = null, string? entityName = null) {
			
		entityName ??= EntityNameHelper.GetEntityName(typeof(TInput).Name);
			
		try {
			if (filter == null || filter.Body.NodeType != ExpressionType.Equal)
				return new NotFoundException($"No {entityName} found");

			var op = (BinaryExpression) filter.Body;

			string? param = null;
			string? side = null;
			if (op.Left.NodeType == ExpressionType.MemberAccess) {
				var left = (MemberExpression) op.Left;
				if (left.Member.DeclaringType == typeof(TInput)) {
					param = left.Member.Name;
					side = "left";
				}
			}

			if (param == null && op.Right.NodeType == ExpressionType.MemberAccess) {
				var right = (MemberExpression) op.Right;
				if (right.Member.DeclaringType == typeof(TInput)) {
					param = right.Member.Name;
					side = "right";
				}
			}

			if (param == null)
				return new NotFoundException($"No {entityName} found");

			var value = Expression
			   .Lambda(side == "left" ? (MemberExpression) op.Right : (MemberExpression) op.Left)
			   .Compile()
			   .DynamicInvoke();

			return new NotFoundException($"{entityName} with {param} '{value ?? "null"}' does not exist.");
		} catch (Exception) {
			return new NotFoundException($"No {entityName} found");
		}
	}

		
		
}