namespace ScreenTimeTracker.ApplicationLayer.Common.DTOs;

public record ProcessInfoDto(
    Guid Id,
    string Name,
    string? Alias,
    bool AutoUpdate,
    string LastAutoUpdated,
    string? ExecutablePath,
    string? IconPath,
    string? Description
);