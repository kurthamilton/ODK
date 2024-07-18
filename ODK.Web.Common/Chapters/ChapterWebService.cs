using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Events;

namespace ODK.Web.Common.Chapters;
public class ChapterWebService : IChapterWebService
{
    private readonly IEventService _eventService;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterWebService(IUnitOfWork unitOfWork,
        IEventService eventService)
    {
        _eventService = eventService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AboutPageViewModel> GetAboutPageViewModelAsync(Guid? currentMemberId, string chapterName)
    {
        var (chapter, member) = await GetChapterViewModelBasePropertiesAsync(currentMemberId, chapterName);

        var questions = await _unitOfWork.ChapterQuestionRepository.GetByChapterId(chapter.Id).RunAsync();

        return new AboutPageViewModel
        {
            Chapter = chapter,
            Member = member,
            Questions = questions
        };
    }

    public async Task<ChapterPageViewModel> GetChapterPageViewModelAsync(Guid? currentMemberId, string chapterName)
    {
        var (chapter, member) = await GetChapterViewModelBasePropertiesAsync(currentMemberId, chapterName);

        var events = await _eventService.GetEventResponseViewModels(member, chapter.Id, DateTime.Today);

        var isChapterMember = member?.ChapterId == chapter.Id;

        var (links, texts, latestMembers, instagramPosts) = await _unitOfWork.RunAsync(
            x => x.ChapterLinksRepository.GetByChapterId(chapter.Id),
            x => isChapterMember ? x.ChapterTextsRepository.GetByChapterId(chapter.Id) : DeferreredDefaults.Single(new ChapterTexts()),
            x => isChapterMember ? x.MemberRepository.GetLatestByChapterId(chapter.Id, 8) : DeferreredDefaults.Multiple<Member>(),
            x => x.InstagramPostRepository.GetByChapterId(chapter.Id, 8));

        return new ChapterPageViewModel
        {
            Chapter = chapter,
            Events = events,
            InstagramPosts = instagramPosts,
            LatestMembers = latestMembers,
            Links = links,
            Member = member,
            Texts = texts
        };
    }

    public async Task<ContactPageViewModel> GetContactPageViewModelAsync(Guid? currentMemberId, string chapterName)
    {
        var (chapter, member, settings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByName(chapterName),
            x => currentMemberId != null ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value) : DeferreredDefaults.SingleOrDefault<Member>(),
            x => x.SiteSettingsRepository.Get());

        if (chapter == null)
        {
            throw new OdkNotFoundException();
        }

        return new ContactPageViewModel
        {
            Chapter = chapter,
            Member = member,
            Settings = settings
        };
    }

    public async Task<EventPageViewModel> GetEventPageViewModelAsync(Guid? currentMemberId, string chapterName, Guid eventId)
    {
        var (chapter, member, @event, venue, settings, responses) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByName(chapterName),
            x => currentMemberId != null ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value) : DeferreredDefaults.SingleOrDefault<Member>(),
            x => x.EventRepository.GetById(eventId),
            x => x.VenueRepository.GetByEventId(eventId),
            x => x.SiteSettingsRepository.Get(),
            x => x.EventResponseRepository.GetByEventId(eventId));

        if (member?.ChapterId != chapter.Id)
        {
            responses = [];
        }

        var memberIds = responses
            .Select(x => x.MemberId)
            .ToArray();

        var members = memberIds.Length > 0
            ? await _unitOfWork.MemberRepository.GetByChapterId(chapter.Id, memberIds).RunAsync()
            : [];

        return new EventPageViewModel
        {
            Chapter = chapter,
            Event = @event,
            Member = member,
            Members = members,
            Responses = responses,
            Settings = settings,
            Venue = venue,
        };
    }

    public async Task<EventsPageViewModel> GetEventsPageViewModelAsync(Guid? currentMemberId, string chapterName)
    {
        var (chapter, member) = await GetChapterViewModelBasePropertiesAsync(currentMemberId, chapterName);

        var events = await _eventService.GetEventResponseViewModels(member, chapter.Id);

        return new EventsPageViewModel
        {
            Chapter = chapter,
            Events = events,
            Member = member
        };
    }

    private async Task<(Chapter, Member?)> GetChapterViewModelBasePropertiesAsync(Guid? currentMemberId, string chapterName)
    {
        var (chapter, member) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByName(chapterName),
            x => currentMemberId != null ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value) : DeferreredDefaults.SingleOrDefault<Member>());

        if (chapter == null)
        {
            throw new OdkNotFoundException();
        }

        return (chapter, member);
    }
}
