namespace AdminPanel.Models.Lookup;

public class Select2LookupResponseViewModel
{
    public List<Select2LookupItemViewModel> Results { get; set; } = new();
    public Select2PaginationViewModel Pagination { get; set; } = new();
}
