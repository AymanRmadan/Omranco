using Microsoft.AspNetCore.Authorization;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.Services.Abstractions.GeographyServices;

namespace Omranco.PL.Controllers.Geography;

[ApiController]
[Route("api/geography")]
[Authorize]
public sealed class GeographyController
    : ControllerBase
{
    private readonly IGeographyService
        _geographyService;


    public GeographyController(
        IGeographyService geographyService)
    {
        _geographyService =
            geographyService;
    }


    [HttpGet("governorates")]
    public async Task<IActionResult>
        GetGovernorates()
    {
        var result =
            await _geographyService
                .GetGovernoratesAsync();


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpGet(
        "governorates/{governorateId:int}")]
    public async Task<IActionResult>
        GetGovernorate(
            int governorateId)
    {
        var result =
            await _geographyService
                .GetGovernorateAsync(
                    governorateId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpGet(
        "governorates/{governorateId:int}/zones")]
    public async Task<IActionResult>
        GetZones(
            int governorateId)
    {
        var result =
            await _geographyService
                .GetZonesAsync(
                    governorateId
                );


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
}