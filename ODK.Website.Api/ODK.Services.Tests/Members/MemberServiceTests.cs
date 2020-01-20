using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.DataTypes;
using ODK.Core.Members;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Services.Exceptions;
using ODK.Services.Imaging;
using ODK.Services.Members;
using ODK.Services.Payments;

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
                LastName = lastName,
                Properties = new UpdateMemberProperty[0]
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
            IChapterRepository chapterRepository = null, ICacheService cacheService = null)
        {
            return new MemberService(memberRepository ?? CreateMockMemberRepository(CreateMockMember()),
                chapterRepository ?? CreateMockChapterRepository(),
                CreateMockAuthorizationService(),
                CreateMockMailService(),
                new MemberServiceSettings(),
                CreateMockImageService(),
                Mock.Of<IPaymentService>(),
                cacheService ?? CreateMockCacheService(new Member(Guid.NewGuid(), Guid.NewGuid(), "email", true, "first", "last", DateTime.Today, true, false, 0)));
        }

        private static IAuthorizationService CreateMockAuthorizationService()
        {
            return Mock.Of<IAuthorizationService>();
        }

        private static ICacheService CreateMockCacheService(Member member = null)
        {
            Mock<ICacheService> mock = new Mock<ICacheService>();

            mock.Setup(x => x.GetOrSetVersionedItem(It.IsAny<Func<Task<Member>>>(), It.IsAny<object>(), It.IsAny<long?>()))
                .ReturnsAsync(new VersionedServiceResult<Member>(0, member));

            return mock.Object;
        }

        private static ChapterProperty CreateMockChapterProperty(string label = null, bool required = false)
        {
            return new ChapterProperty(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DataType.Text,
                "name",
                label ?? "property",
                1,
                required,
                null,
                null,
                false);
        }

        private static IChapterRepository CreateMockChapterRepository(IEnumerable<ChapterProperty> chapterProperties = null)
        {
            Mock<IChapterRepository> mock = new Mock<IChapterRepository>();

            mock.Setup(x => x.GetChapterProperties(It.IsAny<Guid>()))
                .ReturnsAsync(chapterProperties?.ToArray() ?? new ChapterProperty[0]);

            return mock.Object;
        }

        private static IImageService CreateMockImageService()
        {
            return Mock.Of<IImageService>();
        }

        private static IEmailService CreateMockMailService()
        {
            return Mock.Of<IEmailService>();
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
                false,
                0);
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

            mock.Setup(x => x.GetMember(It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(member);

            mock.Setup(x => x.GetMemberProperties(It.IsAny<Guid>()))
                .ReturnsAsync(memberProperties?.ToArray() ?? new MemberProperty[0]);

            return mock.Object;
        }
    }
}
