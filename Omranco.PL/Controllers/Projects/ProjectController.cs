using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Projects;
using Omranco.BLL.Services.Abstractions.ProjectServices;

namespace Omranco.PL.Controllers.Projects;

[ApiController]
[Route("api/projects")]
[Authorize]
public sealed class ProjectController
    : ControllerBase
{
    private readonly IProjectService _service;

    public ProjectController(
        IProjectService service)
    {
        _service = service;
    }


    [HttpGet("account/{accountId:int}")]
    public async Task<IActionResult> GetAccessibleProjects(
        int accountId)
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .GetAccessibleProjectsAsync(
                    accountId,
                    userId
                );

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPost("account/{accountId:int}")]
    public async Task<IActionResult> Create(
        int accountId,
        [FromBody] CreateProjectRequest request)
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service.CreateAsync(
                accountId,
                userId,
                request
            );

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPut("{projectId:int}")]
    public async Task<IActionResult> Update(
        int projectId,
        [FromBody] UpdateProjectRequest request)
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service.UpdateAsync(
                projectId,
                userId,
                request
            );

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPost("{projectId:int}/members")]
    public async Task<IActionResult> AssignMember(
        int projectId,
        [FromBody] AssignProjectMemberRequest request)
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service.AssignMemberAsync(
                projectId,
                userId,
                request
            );


        return result.IsSuccess ? Ok() : result.ToProblem();
    }


    [HttpDelete(
        "{projectId:int}/members/{accountUserId:int}")]
    public async Task<IActionResult> RemoveMember(
        int projectId,
        int accountUserId)
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service.RemoveMemberAsync(
                projectId,
                accountUserId,
                userId
            );


        return result.IsSuccess ? Ok() : result.ToProblem();
    }


    [HttpPost("{projectId:int}/deactivate")]
    public async Task<IActionResult> Deactivate(
        int projectId)
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service.DeactivateAsync(
                projectId,
                userId
            );


        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}