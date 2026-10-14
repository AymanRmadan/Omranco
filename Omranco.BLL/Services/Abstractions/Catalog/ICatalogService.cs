using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Catalog.Requests;
using Omranco.BLL.DTOS.Catalog.Responses;

namespace Omranco.BLL.Services.Abstractions.CatalogServices
{
    public interface ICatalogService
    {
        // Public

        Task<Result<List<CategoryTreeResponse>>>
            GetCategoriesAsync();

        Task<Result<List<UnitResponse>>>
            GetUnitsAsync();

        Task<Result<List<ProductSummaryResponse>>>
            GetProductsAsync(
                int? categoryId);

        Task<Result<ProductDetailsResponse>>
            GetProductAsync(
                int productId);


        // Categories

        Task<Result<int>> CreateCategoryAsync(
            CreateCategoryRequest request);

        Task<Result> UpdateCategoryAsync(
            int categoryId,
            UpdateCategoryRequest request);


        // Units

        Task<Result<int>> CreateUnitAsync(
            CreateUnitRequest request);

        Task<Result> UpdateUnitAsync(
            int unitId,
            UpdateUnitRequest request);


        // Products

        Task<Result<int>> CreateProductAsync(
            CreateProductRequest request);

        Task<Result> UpdateProductAsync(
            int productId,
            UpdateProductRequest request);

        Task<Result> SetProductUnitsAsync(
            int productId,
            SetProductUnitsRequest request);


        // Attributes

        Task<Result<int>> CreateAttributeAsync(
            int productId,
            CreateProductAttributeRequest request);

        Task<Result> UpdateAttributeAsync(
            int attributeId,
            UpdateProductAttributeRequest request);


        // Options

        Task<Result<int>> CreateAttributeOptionAsync(
            int attributeId,
            CreateProductAttributeOptionRequest request);

        Task<Result> UpdateAttributeOptionAsync(
            int optionId,
            UpdateProductAttributeOptionRequest request);


        // Variants

        Task<Result<int>> CreateVariantAsync(
            int productId,
            CreateProductVariantRequest request);

        Task<Result> UpdateVariantAsync(
            int variantId,
            UpdateProductVariantRequest request);
    }
}