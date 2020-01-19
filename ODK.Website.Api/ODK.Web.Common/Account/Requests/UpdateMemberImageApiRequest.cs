namespace ODK.Web.Common.Account.Requests
{
    public class UpdateMemberImageApiRequest
    {
        public string ContentType { get; set; }

        public byte[] ImageData { get; set; }
    }
}
