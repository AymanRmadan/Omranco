using Microsoft.AspNetCore.Authorization;
using Omranco.BLL;
using Omranco.BLL.Commons.ResponseResults;
using Omranco.BLL.DTOS.Customers;
using Omranco.BLL.Services.Abstractions.CustomerServices;

namespace Omranco.PL.Controllers.Customers;

[ApiController]
[Route("api/customer-accounts")]
[Authorize]
public sealed class CustomerAccountController
    : ControllerBase
{
    private readonly ICustomerAccountService _service;

    public CustomerAccountController(
        ICustomerAccountService service)
    {
        _service = service;
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerAccountRequest request)
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service.CreateAsync(
                userId,
                request
            );


        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpGet]
    public async Task<IActionResult> GetMyAccounts()
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service
                .GetMyAccountsAsync(userId);


        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpGet("{accountId:int}/members")]
    public async Task<IActionResult> GetMembers(
        int accountId)
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service.GetMembersAsync(
                accountId,
                userId
            );


        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPost("{accountId:int}/members")]
    public async Task<IActionResult> AddMember(
        int accountId,
        [FromBody] AddCustomerAccountMemberRequest request)
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service.AddMemberAsync(
                accountId,
                userId,
                request
            );


        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpDelete(
        "{accountId:int}/members/{accountUserId:int}")]
    public async Task<IActionResult> RemoveMember(
        int accountId,
        int accountUserId)
    {
        var userId = User.GetUserId();

        if (userId == 0)
            return Unauthorized();


        var result =
            await _service.RemoveMemberAsync(
                accountId,
                accountUserId,
                userId
            );


        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}