using ODK.Core.Topics;
using ODK.Data.Core;
using ODK.Services.Topics.ViewModels;

namespace ODK.Services.Topics;

public class TopicAdminService : OdkAdminServiceBase, ITopicAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public TopicAdminService(IUnitOfWork unitOfWork) 
        : base(unitOfWork)
    {
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

    public async Task<TopicsAdminPageViewModel> GetTopicsAdminPageViewModel(Guid currentMemberId)
    {
        var (topicGroups, topics) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.TopicGroupRepository.GetAll(),
            x => x.TopicRepository.GetAll());

        return new TopicsAdminPageViewModel
        {
            TopicGroups = topicGroups,
            Topics = topics
        };
    }
}
