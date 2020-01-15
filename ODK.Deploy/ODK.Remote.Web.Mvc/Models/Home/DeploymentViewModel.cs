namespace ODK.Remote.Web.Mvc.Models.Home
{
    public class DeploymentViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string LastBackup { get; set; }

        public string LastUpload { get; set; }

        public bool Offline { get; set; }

        public string OfflineFile { get; set; }

        public string RemotePath { get; set; }
    }
}
