using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.DeleteAppCategory;

public class DeleteAppCategoryEndpoint(IMediator mediator)
    : Endpoint<DeleteAppCategoryRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("app-categories/{appCategoryId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteAppCategoryRequest req, CancellationToken ct)
    {
        await mediator.Send(new DeleteAppCategoryCommand(AppCategoryId: req.AppCategoryId), ct);
        await Send.NoContentAsync(ct);
    }
}
