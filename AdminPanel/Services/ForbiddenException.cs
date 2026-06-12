namespace AdminPanel.Services;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("Access forbidden") { }
}
