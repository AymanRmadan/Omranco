using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Projects;

namespace Omranco.BLL.Services.Abstractions.ProjectServices;

public interface IProjectService
{
    Task<Result<ProjectResponse>> CreateAsync(
        int accountId,
        int currentUserId,
        CreateProjectRequest request);

    Task<Result<ProjectResponse>> UpdateAsync(
        int projectId,
        int currentUserId,
        UpdateProjectRequest request);

    Task<Result<List<ProjectResponse>>> GetAccessibleProjectsAsync(
        int accountId,
        int currentUserId);

    Task<Result> AssignMemberAsync(
        int projectId,
        int currentUserId,
        AssignProjectMemberRequest request);

    Task<Result> RemoveMemberAsync(
        int projectId,
        int customerAccountUserId,
        int currentUserId);

    Task<Result> DeactivateAsync(
        int projectId,
        int currentUserId);
}