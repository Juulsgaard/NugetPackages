using System.Diagnostics.CodeAnalysis;

namespace Juulsgaard.Tools.Maybe;

public readonly struct Maybe<T> : IMaybe<T>, IEquatable<IMaybe<T>>, IEquatable<T>, IEquatable<IMaybe>, IEquatable<object>
{
	public static Maybe<T> Empty() => new();
	public static Maybe<T> From(T value) => new(value);

	private readonly T? _value;
	public bool HasValue { get; }
	public bool IsEmpty => !HasValue;

	private Maybe(T value)
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
	public T ValueOrThrow => Value;
	public T? ValueOrDefault => _value;

	#region Conversion

	public static implicit operator Maybe<T>(T value)
	{
		if (value is Maybe<T> m) return m;
		return Maybe.From(value);
	}

	public static implicit operator Maybe<T>(Maybe value) => Maybe<T>.Empty();

	#endregion

	#region Equality

	public bool Equals(IMaybe<T>? other)
	{
		if (other is null) return false;
		if (other.HasValue != HasValue) return false;
		if (other.IsEmpty && IsEmpty) return true;

		if (other.Value is null) {
			return Value is null;
		}

		if (Value is null) return false;

		return Value.Equals(other.Value);
	}

	public bool Equals(T? value)
	{
		if (value is null) return false;
		if (IsEmpty) return false;
		if (Value is null) return false;

		return Value.Equals(value);
	}

	public bool Equals(IMaybe? other)
	{
		if (other is IMaybe<T> maybe) return Equals(maybe);
		if (other is null) return false;
		return IsEmpty;
	}

	public override bool Equals(object? other)
	{
		if (other is null) return false;

		return other switch {
			IMaybe empty    => Equals(empty),
			T val           => Equals(val),
			_               => false
		};
	}

	public override int GetHashCode()
	{
		if (IsEmpty) return -1;
		if (_value == null) return 0;
		return _value.GetHashCode();
	}

	#endregion

	#region Operators

	public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);
	public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);
	
	public static bool operator ==(Maybe<T> left, IMaybe right) => left.Equals(right);
	public static bool operator !=(Maybe<T> left, IMaybe right) => !(left == right);

	#endregion
	
	public override string ToString()
	{
		if (IsEmpty) return "Empty";
		return _value?.ToString() ?? "null";
	}
}

public class Maybe : IMaybe
{
	public static Maybe Empty() => new();
	public static Maybe<T> From<T>(T value) => Maybe<T>.From(value);

	private Maybe()
	{ }

	public bool HasValue => false;
	public bool IsEmpty => true;
}