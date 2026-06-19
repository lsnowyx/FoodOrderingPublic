using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Tracking;

public sealed record GetOrderTrackingRequest(
    [param: Required]
    string Token);
