namespace ODK.Deploy.Services.Remote
{
    public class RemoteServiceSettings
    {
        public string LocalTemp { get; set; }

        public string RemoteBackup { get; set; }

        public string RemoteDeploy { get; set; }

        public RemoteType Type { get; set; }
    }
}
