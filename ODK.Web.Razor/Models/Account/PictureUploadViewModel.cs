﻿using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Account;

public class PictureUploadViewModel
{
    public required string? ChapterName { get; set; }

    public int? CropHeight { get; set; }

    public int? CropWidth { get; set; }

    public int? CropX { get; set; }

    public int? CropY { get; set; }

    /// <summary>
    /// Dummy form field
    /// </summary>
    public string? Image { get; set; }

    public MemberImage? MemberImage { get; set; } 
}
