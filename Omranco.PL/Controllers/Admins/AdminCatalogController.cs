using Microsoft.AspNetCore.Authorization;
using Omranco.BLL.Authorization;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Catalog.Requests;
using Omranco.BLL.Services.Abstractions.CatalogServices;

namespace Omranco.PL.Controllers.Admin;

[ApiController]
[Route("api/admin/catalog")]
[Authorize(Policy = AuthorizationPolicies.Admin)]
public sealed class AdminCatalogController
    : ControllerBase
{
    private readonly ICatalogService
        _catalogService;


    public AdminCatalogController(
        ICatalogService catalogService)
    {
        _catalogService =
            catalogService;
    }


    // =====================================================
    // Categories
    // =====================================================

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(
        [FromBody]
        CreateCategoryRequest request)
    {
        var result =
            await _catalogService
                .CreateCategoryAsync(
                    request
                );


        return result.IsSuccess
            ? Ok(
                new
                {
                    CategoryId =
                        result.Value
                }
            )
            : result.ToProblem();
    }


    [HttpPut("categories/{categoryId:int}")]
    public async Task<IActionResult> UpdateCategory(
        int categoryId,
        [FromBody]
        UpdateCategoryRequest request)
    {
        var result =
            await _catalogService
                .UpdateCategoryAsync(
                    categoryId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    // =====================================================
    // Units
    // =====================================================

    [HttpPost("units")]
    public async Task<IActionResult> CreateUnit(
        [FromBody]
        CreateUnitRequest request)
    {
        var result =
            await _catalogService
                .CreateUnitAsync(
                    request
                );


        return result.IsSuccess
            ? Ok(
                new
                {
                    UnitId =
                        result.Value
                }
            )
            : result.ToProblem();
    }


    [HttpPut("units/{unitId:int}")]
    public async Task<IActionResult> UpdateUnit(
        int unitId,
        [FromBody]
        UpdateUnitRequest request)
    {
        var result =
            await _catalogService
                .UpdateUnitAsync(
                    unitId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    // =====================================================
    // Products
    // =====================================================

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct(
        [FromBody]
        CreateProductRequest request)
    {
        var result =
            await _catalogService
                .CreateProductAsync(
                    request
                );


        return result.IsSuccess
            ? Ok(
                new
                {
                    ProductId =
                        result.Value
                }
            )
            : result.ToProblem();
    }


    [HttpPut("products/{productId:int}")]
    public async Task<IActionResult> UpdateProduct(
        int productId,
        [FromBody]
        UpdateProductRequest request)
    {
        var result =
            await _catalogService
                .UpdateProductAsync(
                    productId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    [HttpPut("products/{productId:int}/units")]
    public async Task<IActionResult> SetProductUnits(
        int productId,
        [FromBody]
        SetProductUnitsRequest request)
    {
        var result =
            await _catalogService
                .SetProductUnitsAsync(
                    productId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    // =====================================================
    // Attributes
    // =====================================================

    [HttpPost(
        "products/{productId:int}/attributes")]
    public async Task<IActionResult> CreateAttribute(
        int productId,
        [FromBody]
        CreateProductAttributeRequest request)
    {
        var result =
            await _catalogService
                .CreateAttributeAsync(
                    productId,
                    request
                );


        return result.IsSuccess
            ? Ok(
                new
                {
                    AttributeId =
                        result.Value
                }
            )
            : result.ToProblem();
    }


    [HttpPut("attributes/{attributeId:int}")]
    public async Task<IActionResult> UpdateAttribute(
        int attributeId,
        [FromBody]
        UpdateProductAttributeRequest request)
    {
        var result =
            await _catalogService
                .UpdateAttributeAsync(
                    attributeId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    // =====================================================
    // Attribute Options
    // =====================================================

    [HttpPost(
        "attributes/{attributeId:int}/options")]
    public async Task<IActionResult> CreateOption(
        int attributeId,
        [FromBody]
        CreateProductAttributeOptionRequest request)
    {
        var result =
            await _catalogService
                .CreateAttributeOptionAsync(
                    attributeId,
                    request
                );


        return result.IsSuccess
            ? Ok(
                new
                {
                    OptionId =
                        result.Value
                }
            )
            : result.ToProblem();
    }


    [HttpPut("options/{optionId:int}")]
    public async Task<IActionResult> UpdateOption(
        int optionId,
        [FromBody]
        UpdateProductAttributeOptionRequest request)
    {
        var result =
            await _catalogService
                .UpdateAttributeOptionAsync(
                    optionId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    // =====================================================
    // Variants
    // =====================================================

    [HttpPost(
        "products/{productId:int}/variants")]
    public async Task<IActionResult> CreateVariant(
        int productId,
        [FromBody]
        CreateProductVariantRequest request)
    {
        var result =
            await _catalogService
                .CreateVariantAsync(
                    productId,
                    request
                );


        return result.IsSuccess
            ? Ok(
                new
                {
                    VariantId =
                        result.Value
                }
            )
            : result.ToProblem();
    }


    [HttpPut("variants/{variantId:int}")]
    public async Task<IActionResult> UpdateVariant(
        int variantId,
        [FromBody]
        UpdateProductVariantRequest request)
    {
        var result =
            await _catalogService
                .UpdateVariantAsync(
                    variantId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }
}