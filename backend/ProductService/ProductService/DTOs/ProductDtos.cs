using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs
{
    public record ProductDto(Guid Id, string Name, string? Description, string Category, string? ImageUrl, decimal Price, int Stock);
    public record ProductCreateDto(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Required, MaxLength(100)] string Category,
    [MaxLength(500)] string? ImageUrl,
    [Range(0, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int Stock
);

    public record ProductUpdateDto(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Required, MaxLength(100)] string Category,
    [MaxLength(500)] string? ImageUrl,
    [Range(0, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int Stock
);

    public record AdjustStockRequest(int Delta, string Reason);
}
