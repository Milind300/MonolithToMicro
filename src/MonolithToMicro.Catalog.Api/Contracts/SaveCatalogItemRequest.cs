using System.ComponentModel.DataAnnotations;

namespace MonolithToMicro.Catalog.Api.Contracts;

public sealed class SaveCatalogItemRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = true)]
    [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = true)]
    [StringLength(200, ErrorMessage = "Category cannot exceed 200 characters.")]
    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public bool IsActive { get; set; }
}