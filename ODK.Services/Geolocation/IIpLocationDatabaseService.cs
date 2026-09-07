using ODK.Services.Geolocation.ViewModels;

namespace ODK.Services.Geolocation;

public interface IIpLocationDatabaseService
{
    Task<ServiceResult> Delete(IMemberServiceRequest request, string fileName);

    /// <summary>
    /// Lists a directory on the server so a site admin can see what the application can actually read.
    /// Read-only, and never the contents of a file. A null or empty path means the configured database
    /// directory.
    /// </summary>
    Task<DirectoryViewModel> GetDirectory(IMemberServiceRequest request, string? path);

    Task<IpLocationDatabaseViewModel> GetViewModel(IMemberServiceRequest request);

    /// <summary>
    /// Writes a file into a directory and deletes it again, to establish whether the application can.
    /// </summary>
    Task<ServiceResult> TestWrite(IMemberServiceRequest request, string path);

    Task<ServiceResult> Update();
}
