namespace AdminPanel.Models.Dashboard;

public class OrderSummaryViewModel
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
}
