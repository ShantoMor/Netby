namespace TransactionsService.Clients
{
    public interface IProductsClient
    {
        Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ProductDto?> AdjustStockAsync(Guid id, int delta, string reason, CancellationToken ct = default);
    }

    public class ProductsClient(HttpClient http) : IProductsClient
    {
        public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var resp = await http.GetAsync($"/api/productos/{id}", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: ct);
        }

        public async Task<ProductDto?> AdjustStockAsync(Guid id, int delta, string reason, CancellationToken ct = default)
        {
            var payload = new { delta, reason };
            var resp = await http.PutAsJsonAsync($"/api/productos/{id}/adjust-stock", payload, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: ct);
        }
    }

    public record ProductDto(Guid Id, string Name, string? Description, string Category, string? ImageUrl, decimal Price, int Stock);
}
