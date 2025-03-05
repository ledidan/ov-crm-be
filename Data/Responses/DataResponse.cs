namespace Data.Responses
{
    public record DataStringResponse
        (bool Flag, string? Message = null, string? Data = null);
    public record DataObjectResponse
        (bool Flag, string? Message = null, object? Data = null);
}
