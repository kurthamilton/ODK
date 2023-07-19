using ODK.Core.DataTypes;

namespace ODK.Web.Api.Admin.Chapters.Requests
{
    public class CreateChapterPropertyApiRequest : UpdateChapterPropertyApiRequest
    {
        public DataType DataType { get; set; }
    }
}
