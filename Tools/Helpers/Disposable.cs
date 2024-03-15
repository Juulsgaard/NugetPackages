namespace Juulsgaard.Tools.Helpers;

public sealed class Disposable(Action onDisposed) : IDisposable
{
	public void Dispose() => onDisposed.Invoke();
}