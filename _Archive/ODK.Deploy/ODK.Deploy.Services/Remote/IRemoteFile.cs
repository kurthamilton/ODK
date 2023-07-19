using System;

namespace ODK.Deploy.Services.Remote
{
    public interface IRemoteFile : IRemoteItem
    {
        DateTime LastModified { get; }
    }
}
