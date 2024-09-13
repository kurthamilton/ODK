using ODK.Core.DataTypes;

namespace ODK.Services.Chapters.Models;

public class CreateChapterProperty : UpdateChapterProperty
{
    public DataType DataType { get; set; }
}
