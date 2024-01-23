using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Crud.Transactions;

public class InnerTransaction : IDbContextTransaction
{
	public static async Task<InnerTransaction> FromFacade(DatabaseFacade facade)
	{
		return new InnerTransaction(
			facade.CurrentTransaction != null,
			facade.CurrentTransaction ?? await facade.BeginTransactionAsync()
		);
	}

	protected readonly bool IsInner;
	public bool Committed { get; protected set; }
	protected IDbContextTransaction Transaction { get; }
		
	public Guid TransactionId => Transaction.TransactionId;
		
	public InnerTransaction(bool isInner, IDbContextTransaction transaction)
	{
		IsInner = isInner;
		Transaction = transaction;
	}

	public void Dispose()
	{
		if (!IsInner || !Committed) {
			Transaction.Dispose();
		}
	}

	public void Commit()
	{
		Committed = true;
		if (!IsInner) {
			Transaction.Commit();
		}
	}
		
	public void ForceCommit()
	{
		Committed = true;
		Transaction.Commit();
	}

	public void Rollback()
	{
		Transaction.Rollback();
	}

	public Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
	{
		Committed = true;
		if (!IsInner) {
			return Transaction.CommitAsync(cancellationToken);
		}

		return Task.CompletedTask;
	}
		
	public Task ForceCommitAsync()
	{
		Committed = true;
		return Transaction.CommitAsync();
	}

	public ValueTask DisposeAsync()
	{
		if (!IsInner || !Committed) {
			return Transaction.DisposeAsync();
		}
			
		return ValueTask.CompletedTask;
	}
		
	public Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
	{
		return Transaction.RollbackAsync(cancellationToken);
	}
}