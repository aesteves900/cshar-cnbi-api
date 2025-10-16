using Cnab.Domain.Enums;

namespace Cnab.Domain.Services;

public static class TransactionClassifier
{
    public static int Sign(TransactionKind kind) => kind switch
    {
        TransactionKind.Debit => +1,
        TransactionKind.Credit => +1,
        TransactionKind.LoanReceipt => +1,
        TransactionKind.Sales => +1,
        TransactionKind.TedReceipt => +1,
        TransactionKind.DocReceipt => +1,
        TransactionKind.Boleto => -1,
        TransactionKind.Financing => -1,
        TransactionKind.Rent => -1,
        _ => 0
    };
}