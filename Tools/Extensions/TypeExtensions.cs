namespace Juulsgaard.Tools.Extensions;

public static class TypeExtensions
{
	
	/// <summary>
	/// Returns true if the type is nullable
	/// </summary>
	/// <param name="type">The type to check</param>
	/// <returns>True if the type is nullable</returns>
	public static bool CanBeNull(this Type type)
	{
		return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
	}
	
	/// <summary>
	/// Check if <paramref name="type"/> extends the generic type <paramref name="baseGeneric"/>
	/// </summary>
	/// <param name="type">The type to check</param>
	/// <param name="baseGeneric">The base type to check for</param>
	/// <returns>Returns true if <paramref name="type"/> extends <paramref name="baseGeneric"/></returns>
	public static bool ExtendsRawGeneric(this Type type, Type baseGeneric)
	{
		baseGeneric = baseGeneric.IsGenericType ? baseGeneric.GetGenericTypeDefinition() : baseGeneric;
		var cursor = type;
		
		while (cursor != null && cursor != typeof(object)) {
			var generic = cursor.IsGenericType ? cursor.GetGenericTypeDefinition() : cursor;
			if (generic == baseGeneric) return true;
			cursor = cursor.BaseType;
		}

		return false;
	}
}