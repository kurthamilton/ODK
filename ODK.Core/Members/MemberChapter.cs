﻿namespace ODK.Core.Members;

public class MemberChapter
{
    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid MemberId { get; set; }
}
