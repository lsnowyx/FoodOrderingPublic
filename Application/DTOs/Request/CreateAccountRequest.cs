using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Request;

public sealed record CreateAccountRequest(
    [param: Required]
    [param: StringLength(256, MinimumLength = 1)]
    string UserName,

    [param: Required]
    [param: StringLength(256, MinimumLength = 1)]
    string Password);
