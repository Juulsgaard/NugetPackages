using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Crud.Monitoring;

/**
 * A snapshot of an entity that only stores non-nav properties
 */
public class EntityStateSnapshot<TModel> : IStateSnapshot<TModel>
{
	// ReSharper disable once StaticMemberInGenericType
	private static readonly MethodInfo SHALLOW_CLONE = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic|BindingFlags.Instance)!;
	
	private readonly IEntityType _entityType;
	
	// A shallow copy of the value passed
	private TModel _value;

	public EntityStateSnapshot(TModel model, IEntityType entityType)
	{
		_entityType = entityType;
		_value = (TModel)SHALLOW_CLONE.Invoke(model, Array.Empty<object>())!;
	}

	public TModel GetValue() => _value;

	public bool Compare<TOther>(IStateSnapshot<TOther> snapshot)
	{
		var value = GetValue();
		var otherValue = snapshot.GetValue();
		if (value == null) return otherValue == null;
		if (otherValue == null) return false;

		if (snapshot is not EntityStateSnapshot<TModel>) return value.Equals(otherValue);
		
		foreach (var prop in _entityType.GetProperties()) {
			var getter = prop.GetGetter();
			var val = getter.GetClrValue(value);
			var otherVal = getter.GetClrValue(otherValue);

			if (val == null) {
				if (otherVal == null) continue;
				return false;
			}
			if (otherVal == null) return false;

			if (!val.Equals(otherVal)) return false;
		}

		return true;
	}
}