using ErrorOr;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.RefreshWebsiteMetadata;

public record RefreshWebsiteMetadataCommand(Guid Id, Icon? Icon) : IRequest<ErrorOr<Updated>>;

public record Icon(string Extension, byte[] Data);
