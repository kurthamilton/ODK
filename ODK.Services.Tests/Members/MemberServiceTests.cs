using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Exceptions;
using ODK.Services.Members;

namespace ODK.Services.Tests.Members
{
    public static class MemberServiceTests
    {
        [TestCase("", "first", "last")]
        [TestCase("email", "", "last")]
        [TestCase("email", "first", "")]
        public static void UpdateProfile_EmptyRequiredProperties_ThrowsException(string emailAddress, string firstName, string lastName)
        {
            MemberService service = CreateService();

            Member member = CreateMockMember(emailAddress: emailAddress, firstName: firstName, lastName: lastName);
            
            UpdateMemberProfile profile = new UpdateMemberProfile
            {
                EmailAddress = emailAddress,
                FirstName = firstName,
                LastName = lastName
            };

            Assert.ThrowsAsync<OdkServiceException>(() => service.UpdateMemberProfile(profile));
        }

        [Test]
        public static void UpdateProfile_EmptyRequiredMemberProperty_ThrowsException()
        {
            ChapterProperty[] chapterProperties = new[]
            {
                CreateMockChapterProperty("required", required: true),
                CreateMockChapterProperty("optional", required: false)
            };

            MemberProperty[] memberProperties = new[]
            {
                CreateMockMemberProperty(chapterProperties[0].Id, ""),
                CreateMockMemberProperty(chapterProperties[1].Id, "optional")
            };

            Member member = CreateMockMember();            
            IChapterRepository chapterRepository = CreateMockChapterRepository(chapterProperties: chapterProperties);
            IMemberRepository memberRepository = CreateMockMemberRepository(member, memberProperties);
            MemberService service = CreateService(memberRepository, chapterRepository);

            UpdateMemberProfile profile = new UpdateMemberProfile
            {
                EmailAddress = "email",
                FirstName = "first",
                LastName = "last",
                Properties = memberProperties.Select(x => new UpdateMemberProperty
                {
                    ChapterPropertyId = x.ChapterPropertyId,
                    Value = x.Value
                })
            };

            Assert.ThrowsAsync<OdkServiceException>(() => service.UpdateMemberProfile(profile));
        }

        private static MemberService CreateService(IMemberRepository memberRepository = null,
            IChapterRepository chapterRepository = null)
        {
            return new MemberService(memberRepository ?? CreateMockMemberRepository(CreateMockMember()),
                chapterRepository ?? CreateMockChapterRepository());
        }

        private static ChapterProperty CreateMockChapterProperty(string name = null, bool required = false)
        {
            return new ChapterProperty(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                name ?? "property",
                1,
                required,
                null,
                null);
        }

        private static IChapterRepository CreateMockChapterRepository(IEnumerable<ChapterProperty> chapterProperties = null)
        {
            Mock<IChapterRepository> mock = new Mock<IChapterRepository>();

            mock.Setup(x => x.GetChapterProperties(It.IsAny<Guid>()))
                .ReturnsAsync(chapterProperties?.ToArray() ?? new ChapterProperty[0]);

            return mock.Object;
        }

        private static Member CreateMockMember(Guid? memberId = null, Guid? chapterId = null,
            string emailAddress = null, string firstName = null, string lastName = null)
        {
            return new Member(
                memberId ?? Guid.NewGuid(),
                chapterId ?? Guid.NewGuid(),
                emailAddress ?? "email",
                true,
                firstName ?? "first",
                lastName ?? "last",
                DateTime.Now,
                false);
        }

        private static MemberProperty CreateMockMemberProperty(Guid? chapterPropertyId = null, 
            string value = null)
        {
            return new MemberProperty(
                Guid.NewGuid(),
                Guid.NewGuid(),
                chapterPropertyId ?? Guid.NewGuid(),
                value);
        }

        private static IMemberRepository CreateMockMemberRepository(Member member = null, 
            IEnumerable<MemberProperty> memberProperties = null)
        {
            Mock<IMemberRepository> mock = new Mock<IMemberRepository>();

            mock.Setup(x => x.GetMember(It.IsAny<Guid>()))
                .ReturnsAsync(member);

            mock.Setup(x => x.GetMemberProperties(It.IsAny<Guid>()))
                .ReturnsAsync(memberProperties?.ToArray() ?? new MemberProperty[0]);

            return mock.Object;
        }
    }
}
