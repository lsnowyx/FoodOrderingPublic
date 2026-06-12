using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Order;

public sealed class AdjustItemDto
{
    public Guid ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
