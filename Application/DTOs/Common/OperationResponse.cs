namespace Application.DTOs.Common;

public sealed record OperationResponse(
    bool Success,
    string Message
);
