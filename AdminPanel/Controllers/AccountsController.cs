using AdminPanel.Models.Auth;
using AdminPanel.Services;
using Application.DTOs.Request;
using Application.Features.Account.CreateStaff;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize(AuthorizationPolicyConstants.ADMIN_POLICY)]
public class AccountsController : Controller
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IActionResult Create()
    {
        return View(new CreateAccountViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = model.Adapt<CreateAdminAccountRequest>();
        if (!this.ValidateRequestDto(request))
        {
            return View(model);
        }

        try
        {
            var command = request.Adapt<CreateStaffAccountCommand>();
            var response = await _mediator.Send(command);

            MvcErrorHelper.SetSuccessMessage(
                TempData,
                $"Account created successfully for {response.UserName}.");
            return RedirectToAction(nameof(Create));
        }
        catch (Exception exception) when (MvcErrorHelper.IsFormBusinessException(exception))
        {
            MvcErrorHelper.AddToModelState(ModelState, exception);
            return View(model);
        }
    }
}
