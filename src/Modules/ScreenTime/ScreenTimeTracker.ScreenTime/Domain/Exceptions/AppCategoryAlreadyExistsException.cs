using ScreenTimeTracker.BuildingBlocks.Exceptions;

namespace ScreenTimeTracker.ScreenTime.Domain.Exceptions;

public class AppCategoryAlreadyExistsException : ConflictException
{
    public AppCategoryAlreadyExistsException(string name)
        : base($"App category with name '{name}' already exists.") { }
}
