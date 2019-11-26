using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Authentication;
using ODK.Services.Authorization;
using ODK.Services.Exceptions;
using ODK.Services.Mails;
using ODK.Services.Members;

namespace ODK.Services.Tests.Members
{
    public static class MemberServiceTests
    {
        [TestCase("", "last")]
        [TestCase("first", "")]
        public static void UpdateProfile_EmptyRequiredProperties_ThrowsException(string firstName, string lastName)
        {
            MemberService service = CreateService();

            UpdateMemberProfile profile = new UpdateMemberProfile
            {
                FirstName = firstName,
                LastName = lastName
            };

            Assert.ThrowsAsync<OdkServiceException>(() => service.UpdateMemberProfile(Guid.NewGuid(), profile));
        }

        [Test]
        public static void UpdateProfile_EmptyRequiredMemberProperty_ThrowsException()
        {
            ChapterProperty[] chapterProperties =
            {
                CreateMockChapterProperty("required", required: true),
                CreateMockChapterProperty("optional", required: false)
            };

            MemberProperty[] memberProperties =
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
                FirstName = "first",
                LastName = "last",
                Properties = memberProperties.Select(x => new UpdateMemberProperty
                {
                    ChapterPropertyId = x.ChapterPropertyId,
                    Value = x.Value
                })
            };

            Assert.ThrowsAsync<OdkServiceException>(() => service.UpdateMemberProfile(Guid.NewGuid(), profile));
        }

        private static MemberService CreateService(IMemberRepository memberRepository = null,
            IChapterRepository chapterRepository = null)
        {
            return new MemberService(memberRepository ?? CreateMockMemberRepository(CreateMockMember()),
                chapterRepository ?? CreateMockChapterRepository(),
                CreateMockAuthorizationService(),
                CreateMockMailService(),
                new AuthenticationSettings());
        }

        private static IAuthorizationService CreateMockAuthorizationService()
        {
            return Mock.Of<IAuthorizationService>();
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

        private static IMailService CreateMockMailService()
        {
            return Mock.Of<IMailService>();
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
                true,
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
