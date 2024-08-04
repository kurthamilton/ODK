using ODK.Core;
using ODK.Core.Chapters;
using ODK.Data.Core;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public class MemberViewModelService : IMemberViewModelService
{
    private readonly IUnitOfWork _unitOfWork;

    public MemberViewModelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MemberPageViewModel> GetMemberPage(Guid currentMemberId, string chapterName, Guid memberId)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        OdkAssertions.Exists(chapter);

        var (members, chapterProperties, memberProperties) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByChapterId(chapter.Id, [currentMemberId, memberId]),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.MemberPropertyRepository.GetByMemberId(memberId, chapter.Id));

        var currentMember = members.FirstOrDefault(x => x.Id == currentMemberId);
        var member = members.FirstOrDefault(x => x.Id == memberId);

        OdkAssertions.Exists(currentMember);
        OdkAssertions.Exists(member);

        return new MemberPageViewModel
        {
            Chapter = chapter,
            ChapterProperties = chapterProperties,
            CurrentMember = currentMember,
            Member = member,
            MemberProperties = memberProperties
        };
    }

    public async Task<MembersPageViewModel> GetMembersPage(Guid currentMemberId, string chapterName)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        OdkAssertions.Exists(chapter);

        var members = await _unitOfWork.MemberRepository.GetByChapterId(chapter.Id).RunAsync();
        var member = members.FirstOrDefault(x => x.Id == currentMemberId);
        OdkAssertions.MemberOf(member, chapter.Id);

        return new MembersPageViewModel
        {
            ChapterName = chapter.Name,
            Members = members
        };
    }
}
