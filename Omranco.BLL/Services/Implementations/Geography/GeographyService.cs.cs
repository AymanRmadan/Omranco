using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Geography.Requests;
using Omranco.BLL.DTOS.Geography.Responses;
using Omranco.BLL.Services.Abstractions.GeographyServices;
using Omranco.Domain.Entities.Geography;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.GeographyServices
{
    public sealed class GeographyService
        : IGeographyService
    {
        private readonly IUnitOfWork _unitOfWork;


        public GeographyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork =
                unitOfWork;
        }


        // =====================================================
        // Public - Governorates
        // =====================================================

        public async Task<Result<List<GovernorateListItemResponse>>> GetGovernoratesAsync()
        {
            var governorates =
                await _unitOfWork
                    .Governorates
                    .GetAllAsync(
                        filter:
                            x => x.IsActive,

                        orderBy:
                            query =>
                                query.OrderBy(
                                    x => x.NameAr
                                )
                    );


            var response =
                governorates
                    .Select(
                        x =>
                            new GovernorateListItemResponse(
                                x.Id,
                                x.NameAr,
                                x.NameEn,
                                x.IsActive
                            )
                    )
                    .ToList();


            return Result.Success(
                response
            );
        }


        // =====================================================
        // Public - Governorate Details
        // =====================================================

        public async Task<Result<GovernorateResponse>> GetGovernorateAsync(int governorateId)
        {
            var governorate =
                await _unitOfWork
                    .Governorates
                    .GetByIdAsync(
                        governorateId
                    );


            if (
                governorate is null
                ||
                !governorate.IsActive
            )
            {
                return Result
                    .Failure<GovernorateResponse>(
                        GeographyErrors
                            .GovernorateNotFound
                    );
            }


            var zones =
                await _unitOfWork
                    .Zones
                    .GetAllAsync(
                        filter:
                            x =>
                                x.GovernorateId ==
                                    governorate.Id
                                &&
                                x.IsActive,

                        orderBy:
                            query =>
                                query.OrderBy(
                                    x => x.NameAr
                                )
                    );


            return Result.Success(
                new GovernorateResponse(
                    governorate.Id,
                    governorate.NameAr,
                    governorate.NameEn,
                    governorate.IsActive,

                    zones
                        .Select(
                            MapZone
                        )
                        .ToList()
                )
            );
        }


        // =====================================================
        // Public - Zones
        // =====================================================

        public async Task<Result<List<ZoneResponse>>> GetZonesAsync(int governorateId)
        {
            var governorateExists =
                await _unitOfWork
                    .Governorates
                    .AnyAsync(
                        x =>
                            x.Id ==
                                governorateId
                            &&
                            x.IsActive
                    );


            if (!governorateExists)
            {
                return Result
                    .Failure<List<ZoneResponse>>(
                        GeographyErrors
                            .GovernorateNotFound
                    );
            }


            var zones =
                await _unitOfWork
                    .Zones
                    .GetAllAsync(
                        filter:
                            x =>
                                x.GovernorateId ==
                                    governorateId
                                &&
                                x.IsActive,

                        orderBy:
                            query =>
                                query.OrderBy(
                                    x => x.NameAr
                                )
                    );


            return Result.Success(
                zones
                    .Select(
                        MapZone
                    )
                    .ToList()
            );
        }


        // =====================================================
        // Admin - Create Governorate
        // =====================================================

        public async Task<Result<int>> CreateGovernorateAsync(CreateGovernorateRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.NameAr))
            {
                return Result.Failure<int>(
                    GeographyErrors.InvalidName
                );
            }


            var nameAr =
                request.NameAr.Trim();


            var alreadyExists =
                await _unitOfWork
                    .Governorates
                    .AnyAsync(
                        x =>
                            x.NameAr ==
                            nameAr
                    );


            if (alreadyExists)
            {
                return Result.Failure<int>(
                    GeographyErrors
                        .GovernorateAlreadyExists
                );
            }


            var governorate =
                new Governorate
                {
                    NameAr =
                        nameAr,

                    NameEn =
                        Normalize(
                            request.NameEn
                        ),

                    IsActive =
                        true
                };


            await _unitOfWork
                .Governorates
                .InsertAsync(
                    governorate
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                governorate.Id
            );
        }


        // =====================================================
        // Admin - Update Governorate
        // =====================================================

        public async Task<Result> UpdateGovernorateAsync(int governorateId, UpdateGovernorateRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.NameAr))
            {
                return Result.Failure(
                    GeographyErrors.InvalidName
                );
            }


            var governorate =
                await _unitOfWork
                    .Governorates
                    .GetByIdAsync(
                        governorateId
                    );


            if (governorate is null)
            {
                return Result.Failure(
                    GeographyErrors
                        .GovernorateNotFound
                );
            }


            var nameAr =
                request.NameAr.Trim();


            var duplicate =
                await _unitOfWork
                    .Governorates
                    .AnyAsync(
                        x =>
                            x.Id !=
                                governorateId
                            &&
                            x.NameAr ==
                                nameAr
                    );


            if (duplicate)
            {
                return Result.Failure(
                    GeographyErrors
                        .GovernorateAlreadyExists
                );
            }


            var trackedGovernorate =
                await _unitOfWork
                    .Governorates
                    .FindAsync(
                        governorateId
                    );


            if (trackedGovernorate is null)
            {
                return Result.Failure(
                    GeographyErrors
                        .GovernorateNotFound
                );
            }


            trackedGovernorate.NameAr =
                nameAr;

            trackedGovernorate.NameEn =
                Normalize(
                    request.NameEn
                );

            trackedGovernorate.IsActive =
                request.IsActive;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Admin - Create Zone
        // =====================================================

        public async Task<Result<int>> CreateZoneAsync(CreateZoneRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.NameAr))
            {
                return Result.Failure<int>(
                    GeographyErrors.InvalidName
                );
            }


            var governorateExists =
                await _unitOfWork
                    .Governorates
                    .AnyAsync(
                        x =>
                            x.Id ==
                                request.GovernorateId
                    );


            if (!governorateExists)
            {
                return Result.Failure<int>(
                    GeographyErrors
                        .GovernorateNotFound
                );
            }


            var nameAr =
                request.NameAr.Trim();


            var duplicate =
                await _unitOfWork
                    .Zones
                    .AnyAsync(
                        x =>
                            x.GovernorateId ==
                                request.GovernorateId
                            &&
                            x.NameAr ==
                                nameAr
                    );


            if (duplicate)
            {
                return Result.Failure<int>(
                    GeographyErrors
                        .ZoneAlreadyExists
                );
            }


            var zone =
                new Zone
                {
                    GovernorateId =
                        request.GovernorateId,

                    NameAr =
                        nameAr,

                    NameEn =
                        Normalize(
                            request.NameEn
                        ),

                    IsActive =
                        true
                };


            await _unitOfWork
                .Zones
                .InsertAsync(
                    zone
                );


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success(
                zone.Id
            );
        }


        // =====================================================
        // Admin - Update Zone
        // =====================================================

        public async Task<Result> UpdateZoneAsync(int zoneId, UpdateZoneRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.NameAr))
            {
                return Result.Failure(
                    GeographyErrors.InvalidName
                );
            }


            var zone =
                await _unitOfWork
                    .Zones
                    .GetByIdAsync(
                        zoneId
                    );


            if (zone is null)
            {
                return Result.Failure(
                    GeographyErrors.ZoneNotFound
                );
            }


            var governorateExists =
                await _unitOfWork
                    .Governorates
                    .AnyAsync(
                        x =>
                            x.Id ==
                            request.GovernorateId
                    );


            if (!governorateExists)
            {
                return Result.Failure(
                    GeographyErrors
                        .GovernorateNotFound
                );
            }


            var nameAr =
                request.NameAr.Trim();


            var duplicate =
                await _unitOfWork
                    .Zones
                    .AnyAsync(
                        x =>
                            x.Id !=
                                zoneId
                            &&
                            x.GovernorateId ==
                                request.GovernorateId
                            &&
                            x.NameAr ==
                                nameAr
                    );


            if (duplicate)
            {
                return Result.Failure(
                    GeographyErrors
                        .ZoneAlreadyExists
                );
            }


            var trackedZone =
                await _unitOfWork
                    .Zones
                    .FindAsync(
                        zoneId
                    );


            if (trackedZone is null)
            {
                return Result.Failure(
                    GeographyErrors.ZoneNotFound
                );
            }


            trackedZone.GovernorateId =
                request.GovernorateId;

            trackedZone.NameAr =
                nameAr;

            trackedZone.NameEn =
                Normalize(
                    request.NameEn
                );

            trackedZone.IsActive =
                request.IsActive;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        // =====================================================
        // Helpers
        // =====================================================

        private static ZoneResponse MapZone(Zone zone)
        {
            return new ZoneResponse(
                zone.Id,
                zone.GovernorateId,
                zone.NameAr,
                zone.NameEn,
                zone.IsActive
            );
        }


        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }
    }
}