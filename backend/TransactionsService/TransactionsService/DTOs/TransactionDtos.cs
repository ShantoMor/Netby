using System.ComponentModel.DataAnnotations;
using TransactionsService.Models;

namespace TransactionsService.DTOs
{
    public record TransactionDto(Guid Id, DateTime Date, TransactionType Type, Guid ProductId, int Quantity, decimal UnitPrice, decimal Total, string? Detail);

    public record CreateTransactionDto(
    [Required] TransactionType Type,
    [Required] Guid ProductId,
    [Range(1, int.MaxValue)] int Quantity,
    [Range(0, double.MaxValue)] decimal UnitPrice,
    string? Detail
);

    public record UpdateTransactionDto(
    [Required] TransactionType Type,
    [Range(1, int.MaxValue)] int Quantity,
    [Range(0, double.MaxValue)] decimal UnitPrice,
    string? Detail
);

    public record ProductSnapshot(Guid Id, string Name, int Stock);
}
