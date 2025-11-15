namespace ScreenTimeTracker.Application.DTOs
{
    public record ExecutableMetadata(
        string? Description,
        byte[]? IconBytes,
        string? FileExtension
    );
}