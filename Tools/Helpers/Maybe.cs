namespace Lib.Helpers;

public class Maybe<T> : Maybe
{
	private readonly T? _value;

	public override bool HasValue { get; }
	
	public Maybe(T value)
	{
		HasValue = true;
		_value = value;
	}
	
	public Maybe()
	{
		HasValue = false;
		_value = default;
	}

	public T Value => HasValue ? _value! : throw new NullReferenceException("Maybe doesn't contain value");
	public T? ValueOrDefault => _value;
}

public abstract class Maybe
{
	public static Maybe<T> Empty<T>()
	{
		return new Maybe<T>();
	}
	
	public static Maybe<T> FromValue<T>(T value)
	{
		return new Maybe<T>(value);
	}
	
	public abstract bool HasValue { get; }
}