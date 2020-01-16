namespace ODK.Deploy.Core.Servers
{
    public class Server
    {
        public FileSystemSettings FileSystem { get; set; }

        public FtpSettings Ftp { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public ServerPaths Paths { get; set; }

        public RestSettings Rest { get; set; }

        public ServerType Type { get; set; }
    }
}
