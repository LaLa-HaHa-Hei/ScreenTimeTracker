using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.ImportData;

public record ImportDataCommand(string RawJson) : IRequest<ImportDataResponse>;
