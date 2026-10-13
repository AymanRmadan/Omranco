using Microsoft.AspNetCore.Authorization;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Geography.Requests;
using Omranco.BLL.Services.Abstractions.GeographyServices;

namespace Omranco.PL.Controllers.Admin;

[ApiController]
[Route("api/admin/geography")]
[Authorize]
public sealed class AdminGeographyController
    : ControllerBase
{
    private readonly IGeographyService
        _geographyService;


    public AdminGeographyController(
        IGeographyService geographyService)
    {
        _geographyService =
            geographyService;
    }


    // ==========================================
    // Governorates
    // ==========================================

    [HttpPost("governorates")]
    public async Task<IActionResult>
        CreateGovernorate(
            [FromBody]
            CreateGovernorateRequest request)
    {
        var result =
            await _geographyService
                .CreateGovernorateAsync(
                    request
                );


        return result.IsSuccess
            ? Ok(
                new
                {
                    GovernorateId =
                        result.Value
                }
            )
            : result.ToProblem();
    }


    [HttpPut(
        "governorates/{governorateId:int}")]
    public async Task<IActionResult>
        UpdateGovernorate(
            int governorateId,
            [FromBody]
            UpdateGovernorateRequest request)
    {
        var result =
            await _geographyService
                .UpdateGovernorateAsync(
                    governorateId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }


    // ==========================================
    // Zones
    // ==========================================

    [HttpPost("zones")]
    public async Task<IActionResult>
        CreateZone(
            [FromBody]
            CreateZoneRequest request)
    {
        var result =
            await _geographyService
                .CreateZoneAsync(
                    request
                );


        return result.IsSuccess
            ? Ok(
                new
                {
                    ZoneId =
                        result.Value
                }
            )
            : result.ToProblem();
    }


    [HttpPut("zones/{zoneId:int}")]
    public async Task<IActionResult>
        UpdateZone(
            int zoneId,
            [FromBody]
            UpdateZoneRequest request)
    {
        var result =
            await _geographyService
                .UpdateZoneAsync(
                    zoneId,
                    request
                );


        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }
}