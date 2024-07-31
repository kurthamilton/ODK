using ODK.Core.Members;

namespace ODK.Services.ViewModels;

public abstract class ViewModelBase
{
    public required Member? CurrentMember { get; init; }
}
