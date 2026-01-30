using ODK.Core.Topics;
using ODK.Data.Core;
using ODK.Services.Members;
using ODK.Services.Topics.Models;

namespace ODK.Services.Topics;

public class TopicService : ITopicService
{
    private readonly IMemberEmailService _memberEmailService;
    private readonly IUnitOfWork _unitOfWork;

    public TopicService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService)
    {
        _memberEmailService = memberEmailService;
        _unitOfWork = unitOfWork;
    }

    public async Task AddNewChapterTopics(MemberChapterServiceRequest request, IReadOnlyCollection<NewTopicModel> models)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (topics, chapterNewTopics, siteEmailSettings) = await _unitOfWork.RunAsync(
            x => x.TopicRepository.GetAll(),
            x => x.NewChapterTopicRepository.GetByChapterId(chapter.Id),
            x => x.SiteEmailSettingsRepository.Get(platform));

        var topicGroupDictionary = topics
            .GroupBy(x => x.TopicGroup.Name)
            .ToDictionary(
                x => x.Key,
                x => x.Select(x => x.Name).ToHashSet(StringComparer.InvariantCultureIgnoreCase),
                StringComparer.InvariantCultureIgnoreCase);

        foreach (var chapterNewTopic in chapterNewTopics)
        {
            if (!topicGroupDictionary.ContainsKey(chapterNewTopic.TopicGroup))
            {
                topicGroupDictionary[chapterNewTopic.TopicGroup] = new HashSet<string>();
            }

            topicGroupDictionary[chapterNewTopic.TopicGroup].Add(chapterNewTopic.Topic);
        }

        var newTopics = new List<NewChapterTopic>();
        foreach (var model in models)
        {
            var topic = model.Topic.Trim();
            var topicGroup = model.TopicGroup.Trim();

            if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(topicGroup))
            {
                continue;
            }

            if (!topicGroupDictionary.ContainsKey(topicGroup))
            {
                topicGroupDictionary[model.TopicGroup] = new HashSet<string>();
            }

            if (topicGroupDictionary[topicGroup].Contains(topic))
            {
                continue;
            }

            newTopics.Add(new NewChapterTopic
            {
                ChapterId = chapter.Id,
                MemberId = request.CurrentMember.Id,
                Topic = topic,
                TopicGroup = topicGroup
            });

            topicGroupDictionary[model.TopicGroup].Add(model.Topic);
        }

        if (newTopics.Count > 0)
        {
            _unitOfWork.NewChapterTopicRepository.AddMany(newTopics);
            await _unitOfWork.SaveChangesAsync();

            await _memberEmailService.SendNewTopicEmail(request, newTopics, siteEmailSettings);
        }
    }

    public async Task AddNewMemberTopics(MemberServiceRequest request, IReadOnlyCollection<NewTopicModel> models)
    {
        var (currentMember, platform) = (request.CurrentMember, request.Platform);

        var (topics, memberNewTopics, siteEmailSettings) = await _unitOfWork.RunAsync(
            x => x.TopicRepository.GetAll(),
            x => x.NewMemberTopicRepository.GetByMemberId(currentMember.Id),
            x => x.SiteEmailSettingsRepository.Get(platform));

        var topicGroupDictionary = topics
            .GroupBy(x => x.TopicGroup.Name)
            .ToDictionary(
                x => x.Key,
                x => x.Select(x => x.Name).ToHashSet(StringComparer.InvariantCultureIgnoreCase),
                StringComparer.InvariantCultureIgnoreCase);

        foreach (var memberNewTopic in memberNewTopics)
        {
            if (!topicGroupDictionary.ContainsKey(memberNewTopic.TopicGroup))
            {
                topicGroupDictionary[memberNewTopic.TopicGroup] = new HashSet<string>();
            }

            topicGroupDictionary[memberNewTopic.TopicGroup].Add(memberNewTopic.Topic);
        }

        var newTopics = new List<NewMemberTopic>();
        foreach (var model in models)
        {
            var topic = model.Topic.Trim();
            var topicGroup = model.TopicGroup.Trim();

            if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(topicGroup))
            {
                continue;
            }

            if (!topicGroupDictionary.ContainsKey(topicGroup))
            {
                topicGroupDictionary[model.TopicGroup] = new HashSet<string>();
            }

            if (topicGroupDictionary[topicGroup].Contains(topic))
            {
                continue;
            }

            newTopics.Add(new NewMemberTopic
            {
                MemberId = currentMember.Id,
                Topic = topic,
                TopicGroup = topicGroup
            });

            topicGroupDictionary[model.TopicGroup].Add(model.Topic);
        }

        if (newTopics.Count > 0)
        {
            _unitOfWork.NewMemberTopicRepository.AddMany(newTopics);
            await _unitOfWork.SaveChangesAsync();

            await _memberEmailService.SendNewTopicEmail(request, newTopics, siteEmailSettings);
        }
    }
}
