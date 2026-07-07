using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Services;

public static class ControllerValidationExtensions
{
    public static bool ValidateRequestDto(this Controller controller, object request)
    {
        return controller.TryValidateModel(request);
    }

    public static bool ValidateRequestDtoForRedirect(
        this Controller controller,
        object request,
        string fallbackMessage)
    {
        if (controller.ValidateRequestDto(request))
        {
            return true;
        }

        MvcErrorHelper.SetFirstModelStateErrorMessage(
            controller.TempData,
            controller.ModelState,
            fallbackMessage);
        return false;
    }
}
