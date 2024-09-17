﻿using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class MemberChaptersViewModel
{
    public required IReadOnlyCollection<ChapterWithDistanceViewModel> Admin { get; init; }    

    public required IReadOnlyCollection<ChapterWithDistanceViewModel> Member { get; init; }

    public required IReadOnlyCollection<ChapterWithDistanceViewModel> Owned { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<ChapterTexts> Texts { get; init; }

    public int Total => Admin.Count + Member.Count + Owned.Count;
}
