﻿using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.SocialMedia;
using ODK.Core.Topics;

namespace ODK.Services.Chapters.ViewModels;

public class GroupHomePageViewModel : GroupPageViewModelBase
{
    public required ChapterLocation? ChapterLocation { get; init; }

    public required bool HasImage { get; init; }

    public required IReadOnlyCollection<InstagramPost> InstagramPosts { get; init; }

    public required ChapterLinks? Links { get; init; }

    public required int MemberCount { get; init; }    

    public required IReadOnlyCollection<Member> Owners { get; init; }

    public required IReadOnlyCollection<GroupPageListEventViewModel> RecentEvents { get; init; }

    public required ChapterTexts? Texts { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }

    public required IReadOnlyCollection<GroupPageListEventViewModel> UpcomingEvents { get; init; }
}
