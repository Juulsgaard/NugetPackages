using Juulsgaard.Tools.Helpers;

namespace Juulsgaard.Tools.Extensions;

public static class LockingExtensions
{
	public static async Task<IDisposable> EnterAsync(this SemaphoreSlim semaphore)
	{
		await semaphore.WaitAsync();
		return new Disposable(() => semaphore.Release());
	}
	
	public static async Task<IDisposable> EnterAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken)
	{
		await semaphore.WaitAsync(cancellationToken);
		return new Disposable(() => semaphore.Release());
	}
}