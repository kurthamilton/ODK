using ODK.Core.DataTypes;

namespace ODK.Services.Chapters.Models;

public class ChapterPropertyCreateModel : ChapterPropertyUpdateModel
{
    public DataType DataType { get; set; }
}
