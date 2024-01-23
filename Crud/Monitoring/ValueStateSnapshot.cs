namespace Crud.Monitoring;

public class ValueStateSnapshot<TModel> : IStateSnapshot<TModel>
{
	private readonly TModel _value;

	public ValueStateSnapshot(TModel value)
	{
		_value = value switch
		{
			ValueType => value,
			string => value,
			_ => value
		};
	}

	public TModel GetValue() => _value;

	public bool Compare<TOther>(IStateSnapshot<TOther> snapshot)
	{
		var value = GetValue();
		var otherValue = snapshot.GetValue();
		if (value == null) return otherValue == null;
		return value.Equals(otherValue);
	}
}