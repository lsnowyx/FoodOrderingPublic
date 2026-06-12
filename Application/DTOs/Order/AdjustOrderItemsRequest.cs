namespace Application.DTOs.Order;

public sealed record AdjustOrderItemsRequest(List<AdjustItemDto> Items);
