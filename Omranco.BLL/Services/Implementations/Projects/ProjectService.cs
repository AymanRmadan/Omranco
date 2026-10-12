using Omranco.BLL.Commons.Errors;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Projects;
using Omranco.BLL.Services.Abstractions.ProjectServices;
using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Enums;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.BLL.Services.Implementations.ProjectServices;

public sealed class ProjectService
    : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public async Task<Result<ProjectResponse>> CreateAsync(
        int accountId,
        int currentUserId,
        CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<ProjectResponse>(
                ProjectErrors.InvalidName
            );
        }


        var owner =
            await GetOwnerAsync(
                accountId,
                currentUserId
            );


        if (owner is null)
        {
            return Result.Failure<ProjectResponse>(
                ProjectErrors.AccessDenied
            );
        }


        var zone =
            await _unitOfWork
                .Zones
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == request.ZoneId
                        &&
                        x.IsActive
                );


        if (zone is null)
        {
            return Result.Failure<ProjectResponse>(
                ProjectErrors.ZoneNotFound
            );
        }


        var project =
            new Project
            {
                CustomerAccountId =
                    accountId,

                ZoneId =
                    request.ZoneId,

                Name =
                    request.Name.Trim(),

                Description =
                    Normalize(request.Description),

                Address =
                    Normalize(request.Address),

                Latitude =
                    request.Latitude,

                Longitude =
                    request.Longitude,

                IsActive =
                    true,

                CreatedAt =
                    DateTime.UtcNow
            };


        await _unitOfWork
            .Projects
            .InsertAsync(project);


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success(
            Map(project)
        );
    }


    public async Task<Result<ProjectResponse>> UpdateAsync(
        int projectId,
        int currentUserId,
        UpdateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<ProjectResponse>(
                ProjectErrors.InvalidName
            );
        }


        var project =
            await _unitOfWork
                .Projects
                .GetByIdAsync(projectId);


        if (project is null)
        {
            return Result.Failure<ProjectResponse>(
                ProjectErrors.NotFound
            );
        }


        var owner =
            await GetOwnerAsync(
                project.CustomerAccountId,
                currentUserId
            );


        if (owner is null)
        {
            return Result.Failure<ProjectResponse>(
                ProjectErrors.AccessDenied
            );
        }


        var zone =
            await _unitOfWork
                .Zones
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == request.ZoneId
                        &&
                        x.IsActive
                );


        if (zone is null)
        {
            return Result.Failure<ProjectResponse>(
                ProjectErrors.ZoneNotFound
            );
        }


        var trackedProject =
            await _unitOfWork
                .Projects
                .FindAsync(project.Id);


        if (trackedProject is null)
        {
            return Result.Failure<ProjectResponse>(
                ProjectErrors.NotFound
            );
        }


        trackedProject.Name =
            request.Name.Trim();

        trackedProject.Description =
            Normalize(request.Description);

        trackedProject.ZoneId =
            request.ZoneId;

        trackedProject.Address =
            Normalize(request.Address);

        trackedProject.Latitude =
            request.Latitude;

        trackedProject.Longitude =
            request.Longitude;

        trackedProject.UpdatedAt =
            DateTime.UtcNow;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success(
            Map(trackedProject)
        );
    }


    public async Task<Result<List<ProjectResponse>>>
        GetAccessibleProjectsAsync(
            int accountId,
            int currentUserId)
    {
        var accountUser =
            await _unitOfWork
                .CustomerAccountUsers
                .FirstOrDefaultAsync(
                    x =>
                        x.CustomerAccountId ==
                            accountId
                        &&
                        x.UserId ==
                            currentUserId
                        &&
                        x.IsActive
                );


        if (accountUser is null)
        {
            return Result
                .Failure<List<ProjectResponse>>(
                    ProjectErrors.AccessDenied
                );
        }


        if (accountUser.Role ==
            CustomerAccountUserRole.Owner)
        {
            var ownerProjects =
                await _unitOfWork
                    .Projects
                    .GetAllAsync(
                        filter:
                            x =>
                                x.CustomerAccountId ==
                                    accountId
                                &&
                                x.IsActive,

                        orderBy:
                            query =>
                                query.OrderByDescending(
                                    x => x.CreatedAt
                                )
                    );


            return Result.Success(
                ownerProjects
                    .Select(Map)
                    .ToList()
            );
        }


        var memberships =
            await _unitOfWork
                .ProjectMembers
                .GetAllAsync(
                    filter:
                        x =>
                            x.CustomerAccountUserId ==
                                accountUser.Id
                            &&
                            x.IsActive,

                    includes:
                    [
                        x => x.Project
                    ]
                );


        var projects =
            memberships
                .Where(
                    x =>
                        x.Project.IsActive
                        &&
                        x.Project.CustomerAccountId ==
                            accountId
                )
                .Select(
                    x => Map(x.Project)
                )
                .ToList();


        return Result.Success(projects);
    }


    public async Task<Result> AssignMemberAsync(
        int projectId,
        int currentUserId,
        AssignProjectMemberRequest request)
    {
        var project =
            await _unitOfWork
                .Projects
                .GetByIdAsync(projectId);


        if (project is null)
        {
            return Result.Failure(
                ProjectErrors.NotFound
            );
        }


        var owner =
            await GetOwnerAsync(
                project.CustomerAccountId,
                currentUserId
            );


        if (owner is null)
        {
            return Result.Failure(
                ProjectErrors.AccessDenied
            );
        }


        var member =
            await _unitOfWork
                .CustomerAccountUsers
                .GetByIdAsync(
                    request.CustomerAccountUserId
                );


        if (member is null
            ||
            member.CustomerAccountId !=
                project.CustomerAccountId
            ||
            !member.IsActive
            ||
            member.Role !=
                CustomerAccountUserRole.Member)
        {
            return Result.Failure(
                ProjectErrors.MemberNotInAccount
            );
        }


        var existingAssignment =
            await _unitOfWork
                .ProjectMembers
                .FirstOrDefaultAsync(
                    x =>
                        x.ProjectId ==
                            project.Id
                        &&
                        x.CustomerAccountUserId ==
                            member.Id
                );


        if (existingAssignment is not null)
        {
            if (existingAssignment.IsActive)
            {
                return Result.Failure(
                    ProjectErrors.MemberAlreadyAssigned
                );
            }


            var trackedAssignment =
                await _unitOfWork
                    .ProjectMembers
                    .FindAsync(
                        existingAssignment.Id
                    );


            if (trackedAssignment is null)
            {
                return Result.Failure(
                    ProjectErrors.ProjectMemberNotFound
                );
            }


            trackedAssignment.IsActive =
                true;

            trackedAssignment.AssignedByAccountUserId =
                owner.Id;

            trackedAssignment.AssignedAt =
                DateTime.UtcNow;


            await _unitOfWork
                .SaveChangeAsync();


            return Result.Success();
        }


        var assignment =
            new ProjectMember
            {
                ProjectId =
                    project.Id,

                CustomerAccountUserId =
                    member.Id,

                AssignedByAccountUserId =
                    owner.Id,

                IsActive =
                    true,

                AssignedAt =
                    DateTime.UtcNow
            };


        await _unitOfWork
            .ProjectMembers
            .InsertAsync(assignment);


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    public async Task<Result> RemoveMemberAsync(
        int projectId,
        int customerAccountUserId,
        int currentUserId)
    {
        var project =
            await _unitOfWork
                .Projects
                .GetByIdAsync(projectId);


        if (project is null)
        {
            return Result.Failure(
                ProjectErrors.NotFound
            );
        }


        var owner =
            await GetOwnerAsync(
                project.CustomerAccountId,
                currentUserId
            );


        if (owner is null)
        {
            return Result.Failure(
                ProjectErrors.AccessDenied
            );
        }


        var assignment =
            await _unitOfWork
                .ProjectMembers
                .FirstOrDefaultAsync(
                    x =>
                        x.ProjectId ==
                            projectId
                        &&
                        x.CustomerAccountUserId ==
                            customerAccountUserId
                        &&
                        x.IsActive
                );


        if (assignment is null)
        {
            return Result.Failure(
                ProjectErrors.ProjectMemberNotFound
            );
        }


        var trackedAssignment =
            await _unitOfWork
                .ProjectMembers
                .FindAsync(
                    assignment.Id
                );


        if (trackedAssignment is null)
        {
            return Result.Failure(
                ProjectErrors.ProjectMemberNotFound
            );
        }


        trackedAssignment.IsActive =
            false;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    public async Task<Result> DeactivateAsync(
        int projectId,
        int currentUserId)
    {
        var project =
            await _unitOfWork
                .Projects
                .GetByIdAsync(projectId);


        if (project is null)
        {
            return Result.Failure(
                ProjectErrors.NotFound
            );
        }


        var owner =
            await GetOwnerAsync(
                project.CustomerAccountId,
                currentUserId
            );


        if (owner is null)
        {
            return Result.Failure(
                ProjectErrors.AccessDenied
            );
        }


        var trackedProject =
            await _unitOfWork
                .Projects
                .FindAsync(project.Id);


        if (trackedProject is null)
        {
            return Result.Failure(
                ProjectErrors.NotFound
            );
        }


        trackedProject.IsActive =
            false;

        trackedProject.UpdatedAt =
            DateTime.UtcNow;


        await _unitOfWork
            .SaveChangeAsync();


        return Result.Success();
    }


    private async Task<CustomerAccountUser?> GetOwnerAsync(
        int accountId,
        int currentUserId)
    {
        return await _unitOfWork
            .CustomerAccountUsers
            .FirstOrDefaultAsync(
                x =>
                    x.CustomerAccountId ==
                        accountId
                    &&
                    x.UserId ==
                        currentUserId
                    &&
                    x.Role ==
                        CustomerAccountUserRole.Owner
                    &&
                    x.IsActive
            );
    }


    private static string? Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }


    private static ProjectResponse Map(
        Project project)
    {
        return new ProjectResponse(
            project.Id,
            project.CustomerAccountId,
            project.ZoneId,
            project.Name,
            project.Description,
            project.Address,
            project.Latitude,
            project.Longitude,
            project.IsActive,
            project.CreatedAt
        );
    }
}