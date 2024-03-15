namespace Juulsgaard.Tools.Extensions;

public static class CancellationTokenExtensions
{
	/// <summary>
	/// Create a task that only resolves when the cancellation token is cancelled
	/// </summary>
	/// <param name="token">The cancellation token</param>
	public static Task AwaitAsync(this CancellationToken token)
	{
		if (token.IsCancellationRequested) {
			return Task.FromResult(true);
		}
		
		var completionSource = new TaskCompletionSource<bool>();
		token.Register(() => completionSource.SetResult(true));

		if (token.IsCancellationRequested) {
			completionSource.TrySetResult(true);
		}


		return completionSource.Task;
	}

	/// <summary>
	/// Create a task that only resolves when one of two cancellation tokens is cancelled
	/// </summary>
	/// <param name="token">The cancellation token</param>
	/// <param name="cancellationToken">An optional secondary cancellation token</param>
	public static Task AwaitAsync(this CancellationToken token, CancellationToken? cancellationToken)
	{
		if (cancellationToken is null) return token.AwaitAsync();
		
		if (token.IsCancellationRequested || cancellationToken.Value.IsCancellationRequested) {
			return Task.FromResult(true);
		}
		
		var completionSource = new TaskCompletionSource<bool>();
		token.Register(() => completionSource.TrySetResult(true));
		cancellationToken.Value.Register(() => completionSource.TrySetResult(true));

		if (token.IsCancellationRequested || cancellationToken.Value.IsCancellationRequested) {
			completionSource.TrySetResult(true);
		}

		return completionSource.Task;
	}
}