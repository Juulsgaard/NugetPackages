using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Crud.Transactions;

public static class TransactionExtensions
{
    public static Task<InnerTransaction> BeginInnerTransactionAsync(this DbContext context)
    {
        return InnerTransaction.FromFacade(context.Database);
    }
		
    public static Task<InnerTransaction> BeginInnerTransactionAsync(this DatabaseFacade facade)
    {
        return InnerTransaction.FromFacade(facade);
    }
}