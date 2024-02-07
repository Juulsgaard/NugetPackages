namespace Juulsgaard.Tools.Maybe;

public interface IMaybe<out T> : IMaybe
{
	public T Value { get; }
	public T ValueOrThrow { get; }
	public T? ValueOrDefault { get; }
}

public interface IMaybe
{
	public bool HasValue { get; }
	public bool IsEmpty { get; }
}