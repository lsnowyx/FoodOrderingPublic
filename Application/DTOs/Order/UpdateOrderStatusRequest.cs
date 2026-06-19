using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Order;

public sealed record UpdateOrderStatusRequest(
    [param: Required]
    string Status);
