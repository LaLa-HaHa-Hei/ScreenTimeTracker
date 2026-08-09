using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategory;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryIcon;

public class GetAppCategoryIconEndpoint(IMediator mediator)
    : Endpoint<GetAppCategoryIconRequest, EmptyResponse>
{
    public override void Configure()
    {
        Get("app-categories/{appCategoryId}/icon");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAppCategoryIconRequest req, CancellationToken ct)
    {
        ErrorOr<GetAppCategoryResponse> result = await mediator.Send(
            new GetAppCategoryQuery(req.AppCategoryId),
            ct
        );

        if (result.IsError)
        {
            var firstError = result.FirstError;
            await Send.ResultAsync(
                Results.Problem(
                    detail: firstError.Description,
                    statusCode: firstError.Type switch
                    {
                        ErrorType.NotFound => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status500InternalServerError,
                    },
                    extensions: new Dictionary<string, object?> { ["code"] = firstError.Code }
                )
            );
            return;
        }

        var iconPath = result.Value.IconPath;

        if (string.IsNullOrEmpty(iconPath))
        {
            await Send.ResultAsync(
                Results.Problem(
                    detail: "This app category has no icon.",
                    statusCode: StatusCodes.Status404NotFound,
                    extensions: new Dictionary<string, object?>
                    {
                        ["code"] = "AppCategory.HasNoIcon",
                    }
                )
            );
            return;
        }

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(iconPath, out var contentType))
        {
            contentType = "image/png"; // 默认当作 png 图片
        }

        await Send.FileAsync(new FileInfo(iconPath), contentType, cancellation: ct);
    }
}
