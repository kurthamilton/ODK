using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Topics;
using ODK.Data.Core;
using ODK.Services.Members;
using ODK.Services.Topics.Models;
using ODK.Services.Topics.ViewModels;

namespace ODK.Services.Topics;

public class TopicAdminService : OdkAdminServiceBase, ITopicAdminService
{
    private readonly IMemberEmailService _memberEmailService;
    private readonly IUnitOfWork _unitOfWork;

    public TopicAdminService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService) 
        : base(unitOfWork)
    {
        _memberEmailService = memberEmailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddTopic(Guid currentMemberId, Guid topicGroupId, string name)
    {
        var (topicGroup, existing) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.TopicGroupRepository.GetById(topicGroupId),
            x => x.TopicRepository.GetByName(name));

        if (existing != null)
        {
            return ServiceResult.Failure($"Topic already exists under topic group {existing.TopicGroup.Name}");
        }

        _unitOfWork.TopicRepository.Add(new Topic
        {
            Name = name,
            Public = true,
            TopicGroupId = topicGroupId
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task ApproveTopics(
        MemberServiceRequest request, ApproveTopicsModel approved, ApproveTopicsModel rejected)
    {
        var rejectedNewChapterTopicIds = rejected.Chapters
            .Select(x => x.NewTopicId)
            .ToArray();

        var approvedNewChapterTopicIds = approved.Chapters
            .Select(x => x.NewTopicId)
            .Where(x => rejectedNewChapterTopicIds.All(y => y != x))
            .ToArray();

        var newChapterTopicIds = approvedNewChapterTopicIds
            .Concat(rejectedNewChapterTopicIds)
            .ToArray();

        var rejectedNewMemberTopicIds = rejected.Members
            .Select(x => x.NewTopicId)
            .ToArray();

        var approvedNewMemberTopicIds = approved.Members            
            .Select(x => x.NewTopicId)
            .Where(x => rejectedNewMemberTopicIds.All(y => y != x))
            .ToArray();        

        var newMemberTopicIds = approvedNewMemberTopicIds
            .Concat(rejectedNewMemberTopicIds)
            .ToArray();

        var currentMemberId = request.CurrentMemberId;

        var (topicGroups, topics, newMemberTopics, newChapterTopics) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.TopicGroupRepository.GetAll(),
            x => x.TopicRepository.GetAll(),
            x => x.NewMemberTopicRepository.GetByIds(newMemberTopicIds),
            x => x.NewChapterTopicRepository.GetByIds(newChapterTopicIds));

        if (newMemberTopics.Count == 0 && newChapterTopics.Count == 0)
        {
            return;
        }

        var approvedChapterModelDictionary = approved.Chapters
            .ToDictionary(x => x.NewTopicId);

        var rejectedChapterModelDictionary = rejected.Chapters
            .ToDictionary(x => x.NewTopicId);

        var newChapterTopicDictionary = newChapterTopics
            .ToDictionary(x => x.Id);

        var approvedMemberModelDictionary = approved.Members
            .ToDictionary(x => x.NewTopicId);

        var rejectedMemberModelDictionary = rejected.Members
            .ToDictionary(x => x.NewTopicId);

        var newMemberTopicDictionary = newMemberTopics
            .ToDictionary(x => x.Id);

        var topicDictionary = topics
            .GroupBy(x => x.TopicGroup.Name)
            .ToDictionary(x => x.Key, x => x.ToDictionary(y => y.Name, StringComparer.InvariantCultureIgnoreCase), StringComparer.InvariantCultureIgnoreCase);

        var topicGroupDictionary = topicGroups
            .ToDictionary(x => x.Name, StringComparer.InvariantCultureIgnoreCase);

        foreach (var approvedChapterModelItem in approved.Chapters)
        {
            var newChapterTopic = newChapterTopicDictionary[approvedChapterModelItem.NewTopicId];
            newChapterTopic.Topic = approvedChapterModelItem.Topic;
            newChapterTopic.TopicGroup = approvedChapterModelItem.TopicGroup;

            topicGroupDictionary.TryGetValue(newChapterTopic.TopicGroup, out var topicGroup);

            if (topicGroup == null)
            {
                topicGroup = new TopicGroup
                {
                    Id = Guid.NewGuid(),
                    Name = newChapterTopic.TopicGroup
                };

                _unitOfWork.TopicGroupRepository.Add(topicGroup);
                topicGroupDictionary.Add(topicGroup.Name, topicGroup);

                topicDictionary.Add(topicGroup.Name, new Dictionary<string, Topic>(StringComparer.InvariantCultureIgnoreCase));
            }

            topicDictionary[topicGroup.Name].TryGetValue(newChapterTopic.Topic, out var topic);

            if (topic == null)
            {
                topic = new Topic
                {
                    Id = Guid.NewGuid(),
                    Name = newChapterTopic.Topic,
                    Public = true,
                    TopicGroupId = topicGroup.Id
                };
                _unitOfWork.TopicRepository.Add(topic);

                topicDictionary[topicGroup.Name].Add(topic.Name, topic);
            }            

            _unitOfWork.ChapterTopicRepository.Add(new ChapterTopic
            {
                ChapterId = newChapterTopic.ChapterId,
                TopicId = topic.Id
            });

            _unitOfWork.NewChapterTopicRepository.Delete(newChapterTopic);
        }

        foreach (var approvedMemberModelItem in approved.Members)
        {
            var newMemberTopic = newMemberTopicDictionary[approvedMemberModelItem.NewTopicId];
            newMemberTopic.Topic = approvedMemberModelItem.Topic;
            newMemberTopic.TopicGroup = approvedMemberModelItem.TopicGroup;

            topicGroupDictionary.TryGetValue(newMemberTopic.TopicGroup, out var topicGroup);

            if (topicGroup == null)
            {
                topicGroup = new TopicGroup
                {
                    Id = Guid.NewGuid(),
                    Name = newMemberTopic.TopicGroup
                };

                _unitOfWork.TopicGroupRepository.Add(topicGroup);
                topicGroupDictionary.Add(topicGroup.Name, topicGroup);
                topicDictionary.Add(topicGroup.Name, new Dictionary<string, Topic>(StringComparer.InvariantCultureIgnoreCase));
            }

            topicDictionary[topicGroup.Name].TryGetValue(newMemberTopic.Topic, out var topic);

            if (topic == null)
            {
                topic = new Topic
                {
                    Id = Guid.NewGuid(),
                    Name = newMemberTopic.Topic,
                    Public = true,
                    TopicGroupId = topicGroup.Id
                };
                _unitOfWork.TopicRepository.Add(topic);

                topicDictionary[topicGroup.Name].Add(topic.Name, topic);
            }

            _unitOfWork.MemberTopicRepository.Add(new MemberTopic
            {
                MemberId = newMemberTopic.MemberId,
                TopicId = topic.Id
            });

            _unitOfWork.NewMemberTopicRepository.Delete(newMemberTopic);
        }

        foreach (var rejectedChapterModelItem in rejected.Chapters)
        {
            var newTopic = newChapterTopicDictionary[rejectedChapterModelItem.NewTopicId];
            _unitOfWork.NewChapterTopicRepository.Delete(newTopic);
        }

        foreach (var rejectedMemberModelItem in rejected.Members)
        {
            var newTopic = newMemberTopicDictionary[rejectedMemberModelItem.NewTopicId];
            _unitOfWork.NewMemberTopicRepository.Delete(newTopic);
        }

        await _unitOfWork.SaveChangesAsync();

        var approvedChapterTopics = newChapterTopics
            .Where(x => approvedChapterModelDictionary.ContainsKey(x.Id))
            .ToArray();

        var rejectedChapterTopics = newChapterTopics
            .Where(x => rejectedChapterModelDictionary.ContainsKey(x.Id))
            .ToArray();

        var approvedMemberTopics = newMemberTopics
            .Where(x => approvedMemberModelDictionary.ContainsKey(x.Id))
            .ToArray();

        var rejectedMemberTopics = newMemberTopics
            .Where(x => rejectedMemberModelDictionary.ContainsKey(x.Id))
            .ToArray();

        var memberIds = newMemberTopics
            .Select(x => x.MemberId)
            .Concat(newChapterTopics.Select(x => x.MemberId))
            .Distinct()
            .ToArray();

        var members = await _unitOfWork.MemberRepository.GetByIds(memberIds).Run();

        await _memberEmailService.SendTopicApprovedEmails(
            request.HttpRequestContext,
            approvedMemberTopics.Cast<INewTopic>().Concat(approvedChapterTopics).ToArray(), 
            members);
        await _memberEmailService.SendTopicRejectedEmails(
            request.HttpRequestContext,
            rejectedMemberTopics.Cast<INewTopic>().Concat(rejectedChapterTopics).ToArray(), 
            members);
    }

    public async Task<TopicsAdminPageViewModel> GetTopicsAdminPageViewModel(Guid currentMemberId)
    {
        var (topicGroups, topics, newMemberTopics, newChapterTopics) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.TopicGroupRepository.GetAll(),
            x => x.TopicRepository.GetAll(),
            x => x.NewMemberTopicRepository.GetAll(),
            x => x.NewChapterTopicRepository.GetAll());

        return new TopicsAdminPageViewModel
        {
            NewChapterTopics = newChapterTopics,
            NewMemberTopics = newMemberTopics,
            TopicGroups = topicGroups,
            Topics = topics
        };
    }
}
