using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Rfqs;
using Omranco.BLL.Services.Abstractions.RfqServices;
using Omranco.PL.Models.Rfqs;

namespace Omranco.PL.Controllers.Rfqs;

[ApiController]
[Route("api/rfqs")]
[Authorize]
public sealed class RfqController
    : ControllerBase
{
    private readonly IRfqService _service;


    public RfqController(
        IRfqService service)
    {
        _service = service;
    }


    [HttpPost("project/{projectId:int}")]
    public async Task<IActionResult> Create(
        int projectId,
        [FromBody] CreateRfqRequest request)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .CreateDraftAsync(
                    projectId,
                    userId,
                    request
                );



        return result.IsSuccess ? Ok(new
        {
            RfqId = result.Value
        }) : result.ToProblem();
    }


    [HttpGet("{rfqId:int}")]
    public async Task<IActionResult> GetById(
        int rfqId)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .GetByIdAsync(
                    rfqId,
                    userId
                );

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpGet("project/{projectId:int}")]
    public async Task<IActionResult> GetProjectRfqs(
        int projectId)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .GetProjectRfqsAsync(
                    projectId,
                    userId
                );

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPut("{rfqId:int}")]
    public async Task<IActionResult> Update(
        int rfqId,
        [FromBody] UpdateRfqRequest request)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .UpdateDraftAsync(
                    rfqId,
                    userId,
                    request
                );

        return result.IsSuccess ? Ok() : result.ToProblem();
    }


    [HttpPost("{rfqId:int}/attachments")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAttachment(
        int rfqId,
        [FromForm] UploadRfqAttachmentForm request)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        await using var stream =
            request.File.OpenReadStream();


        var serviceRequest =
            new UploadRfqAttachmentRequest(
                request.Type,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                stream
            );


        var result =
            await _service
                .AddAttachmentAsync(
                    rfqId,
                    userId,
                    serviceRequest
                );

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpDelete(
        "{rfqId:int}/attachments/{attachmentId:int}")]
    public async Task<IActionResult> RemoveAttachment(
        int rfqId,
        int attachmentId)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .RemoveAttachmentAsync(
                    rfqId,
                    attachmentId,
                    userId
                );

        return result.IsSuccess ? Ok() : result.ToProblem();

    }


    [HttpPost("{rfqId:int}/submit")]
    public async Task<IActionResult> Submit(
        int rfqId)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .SubmitAsync(
                    rfqId,
                    userId
                );

        return result.IsSuccess ? Ok() : result.ToProblem();
    }


    [HttpPost("{rfqId:int}/close")]
    public async Task<IActionResult> Close(
        int rfqId)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .CloseAsync(
                    rfqId,
                    userId
                );

        return result.IsSuccess ? Ok() : result.ToProblem();
    }


    [HttpPost("{rfqId:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int rfqId)
    {
        var userId =
            User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .CancelAsync(
                    rfqId,
                    userId
                );


        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}