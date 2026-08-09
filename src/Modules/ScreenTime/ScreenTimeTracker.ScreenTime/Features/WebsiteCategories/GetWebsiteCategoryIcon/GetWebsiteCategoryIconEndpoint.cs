using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategory;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryIcon;

public class GetWebsiteCategoryIconEndpoint(IMediator mediator)
    : Endpoint<GetWebsiteCategoryIconRequest, EmptyResponse>
{
    public override void Configure()
    {
        Get("website-categories/{websiteCategoryId}/icon");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetWebsiteCategoryIconRequest req, CancellationToken ct)
    {
        ErrorOr<GetWebsiteCategoryResponse> result = await mediator.Send(
            new GetWebsiteCategoryQuery(req.WebsiteCategoryId),
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
                    detail: "This website category has no icon.",
                    statusCode: StatusCodes.Status404NotFound,
                    extensions: new Dictionary<string, object?>
                    {
                        ["code"] = "WebsiteCategory.HasNoIcon",
                    }
                )
            );
            return;
        }

        if (!File.Exists(iconPath))
        {
            await Send.ResultAsync(
                Results.Problem(
                    detail: "The icon file does not exist.",
                    statusCode: StatusCodes.Status404NotFound,
                    extensions: new Dictionary<string, object?>
                    {
                        ["code"] = "WebsiteCategory.IconFileDoesNotExist",
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
