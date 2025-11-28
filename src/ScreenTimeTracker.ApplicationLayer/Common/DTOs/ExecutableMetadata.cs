namespace ScreenTimeTracker.ApplicationLayer.Common.DTOs;

public record ExecutableMetadata(
    string? Description,
    byte[]? IconBytes,
    string? FileExtension
);
