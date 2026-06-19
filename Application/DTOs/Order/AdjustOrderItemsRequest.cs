using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Order;

public sealed record AdjustOrderItemsRequest(
    [param: Required]
    [param: MinLength(1)]
    List<AdjustItemDto> Items);
