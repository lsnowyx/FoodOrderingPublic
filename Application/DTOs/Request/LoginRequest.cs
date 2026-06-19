using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Request;

public sealed record LoginRequest(
    [param: Required]
    [param: StringLength(256)]
    string Username,

    [param: Required]
    [param: StringLength(256)]
    string Password);
