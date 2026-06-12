using System.ComponentModel.DataAnnotations;
using Common.Constants;

namespace Application.DTOs.Request;

public sealed record CreateAdminAccountRequest(
    [param: Required(ErrorMessage = "UserName is required.")]
    [param: StringLength(256, MinimumLength = 1)]
    string UserName,

    [param: Required(ErrorMessage = "Password is required.")]
    [param: StringLength(256, MinimumLength = 1)]
    string Password,

    [param: Required(ErrorMessage = "Role is required.")]
    string Role
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var allowedRoles = new[] { UserRoleConstants.MENU_MANAGER_ROLE, UserRoleConstants.ORDER_MANAGER_ROLE };
        
        if (!allowedRoles.Contains(Role, StringComparer.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                $"Only '{UserRoleConstants.MENU_MANAGER_ROLE}' and '{UserRoleConstants.ORDER_MANAGER_ROLE}' roles can be created. Received: {Role}",
                new[] { nameof(Role) }
            );
        }
    }
}
