using Microsoft.AspNetCore.Authorization;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.CatalogServices;

namespace Omranco.PL.Controllers.Catalog;

[ApiController]
[Route("api/catalog")]
[Authorize]
public sealed class CatalogController
    : ControllerBase
{
    private readonly ICatalogService
        _catalogService;


    public CatalogController(
        ICatalogService catalogService)
    {
        _catalogService =
            catalogService;
    }


    [HttpGet("categories")]
    public async Task<IActionResult>
        GetCategories()
    {
        var result =
            await _catalogService
                .GetCategoriesAsync();


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpGet("units")]
    public async Task<IActionResult>
        GetUnits()
    {
        var result =
            await _catalogService
                .GetUnitsAsync();


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpGet("products")]
    public async Task<IActionResult>
        GetProducts(
            [FromQuery]
            int? categoryId)
    {
        var result =
            await _catalogService
                .GetProductsAsync(
                    categoryId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpGet("products/{productId:int}")]
    public async Task<IActionResult>
        GetProduct(
            int productId)
    {
        var result =
            await _catalogService
                .GetProductAsync(
                    productId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
}