
using System.Net;
using System.Net.Http.Json;

namespace MonolithToMicro.Order.Api.Integrations;

public sealed class ProductApiClient(HttpClient httpClient)
{
    public async Task<InventoryItemResponse?> GetByIdAsync(
        Guid productId,
        CancellationToken ct)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "ProductId cannot be empty.",
                nameof(productId));
        }

        using var response = await httpClient.GetAsync(
            $"api/product/{productId:D}",
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var product =
            await response.Content.ReadFromJsonAsync<InventoryItemResponse>(
                cancellationToken: ct);

        if (product is null)
        {
            throw new InvalidDataException(
                "Product API returned an empty product response.");
        }

        if (product.Id != productId)
        {
            throw new InvalidDataException(
                "Product API returned a mismatched product ID.");
        }

        if (product.AvailableQuantity < 0 || product.UnitPrice <= 0)
        {
            throw new InvalidDataException(
                "Product API returned invalid inventory data.");
        }

        return product;
    }
}
