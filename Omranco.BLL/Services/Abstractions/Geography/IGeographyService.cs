using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Geography.Requests;
using Omranco.BLL.DTOS.Geography.Responses;

namespace Omranco.BLL.Services.Abstractions.GeographyServices
{
    public interface IGeographyService
    {
        // Public / App

        Task<Result<List<GovernorateListItemResponse>>>
            GetGovernoratesAsync();

        Task<Result<GovernorateResponse>>
            GetGovernorateAsync(
                int governorateId);

        Task<Result<List<ZoneResponse>>>
            GetZonesAsync(
                int governorateId);


        // Admin

        Task<Result<int>>
            CreateGovernorateAsync(
                CreateGovernorateRequest request);

        Task<Result>
            UpdateGovernorateAsync(
                int governorateId,
                UpdateGovernorateRequest request);

        Task<Result<int>>
            CreateZoneAsync(
                CreateZoneRequest request);

        Task<Result>
            UpdateZoneAsync(
                int zoneId,
                UpdateZoneRequest request);
    }
}