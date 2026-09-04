using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.EntityFramework;

namespace ODK.Services.Tests.Helpers;

internal class MockOdkContext : OdkContext
{
    private readonly bool _noTracking;

    public MockOdkContext()
        : this(noTracking: false)
    {
    }

    /* The real context reads without tracking, so an included entity arrives as a separate instance per
       row - and a write that attaches two payments sharing one currency is rejected. Tracking resolves
       those to one instance and hides it, so a test covering a multi-row write has to ask for the
       behaviour the app actually runs with. */
    public MockOdkContext(bool noTracking)
        : base(new OdkContextSettings(""))
    {
        _noTracking = noTracking;
    }

    public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
    {
        if (entity is IDatabaseEntity databaseEntity)
        {
            if (databaseEntity.Id == default)
            {
                databaseEntity.Id = Guid.NewGuid();
            }
        }

        return base.Add(entity);
    }

    public void AddRange<TEntity>(params TEntity[] entities)
        where TEntity : class
    {
        foreach (var entity in entities)
        {
            Add(entity);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    internal T Create<T>(T entity)
        where T : class
    {
        Add(entity);
        return entity;
    }

    internal Chapter CreateChapter(
        Member? owner = null,
        Country? country = null,
        SiteSubscription? siteSubscription = null,
        DateTime? approvedUtc = null,
        string name = "",
        PlatformType platform = PlatformType.Default,
        TimeZoneInfo? timeZone = null,
        IEnumerable<Member>? adminMembers = null,
        IEnumerable<Member>? members = null,
        IEnumerable<Member>? unapprovedMembers = null,
        Action<Chapter>? afterCreate = null)
    {
        country ??= CreateCountry();
        owner ??= CreateMember();

        if (siteSubscription != null)
        {
            CreateMemberSiteSubscription(owner, siteSubscription);
        }

        var chapter = Create(new Chapter
        {
            ApprovedUtc = approvedUtc,
            Id = Guid.NewGuid(),
            Name = name,
            Slug = UrlUtils.Slugify(name),
            OwnerId = owner.Id,
            CreatedUtc = DateTime.UtcNow,
            CountryId = country.Id,
            Platform = platform,
            TimeZone = timeZone ?? Chapter.DefaultTimeZone
        });

        CreateChapterAdminMember(chapter, owner, role: ChapterAdminRole.Owner);

        if (adminMembers != null)
        {
            foreach (var adminMember in adminMembers)
            {
                CreateChapterAdminMember(chapter, adminMember);
            }
        }

        if (members != null)
        {
            foreach (var member in members)
            {
                member.Chapters.Add(new MemberChapter
                {
                    Approved = true,
                    Id = Guid.NewGuid(),
                    ChapterId = chapter.Id,
                    CreatedUtc = DateTime.UtcNow,
                    MemberId = member.Id
                });
            }
        }

        if (unapprovedMembers != null)
        {
            foreach (var member in unapprovedMembers)
            {
                member.Chapters.Add(new MemberChapter
                {
                    Approved = false,
                    Id = Guid.NewGuid(),
                    ChapterId = chapter.Id,
                    CreatedUtc = DateTime.UtcNow,
                    MemberId = member.Id
                });
            }
        }

        afterCreate?.Invoke(chapter);

        return chapter;
    }

    internal ChapterAdminMember CreateChapterAdminMember(
        Chapter chapter,
        Member member,
        ChapterAdminRole? role = null)
        => Create(new ChapterAdminMember
        {
            ChapterId = chapter.Id,
            Id = Guid.NewGuid(),
            Member = member,
            MemberId = member.Id,
            Role = role ?? ChapterAdminRole.Admin
        });

    internal ChapterHeaderImage CreateChapterHeaderImage(Chapter chapter) => Create(new ChapterHeaderImage
    {
        ChapterId = chapter.Id,
        ImageData = [1, 2, 3],
        MimeType = ChapterHeaderImage.DefaultMimeType
    });

    internal ChapterImage CreateChapterImage(Chapter chapter) => Create(new ChapterImage
    {
        ChapterId = chapter.Id,
        ImageData = [1, 2, 3],
        MimeType = ChapterImage.DefaultMimeType
    });

    internal ChapterPaymentAccount CreateChapterPaymentAccount(
        Chapter? chapter = null,
        string? externalId = null,
        bool setupComplete = true)
    {
        chapter ??= CreateChapter();

        var utcNow = DateTime.UtcNow;

        return Create(new ChapterPaymentAccount
        {
            ChapterId = chapter.Id,
            CreatedUtc = utcNow,
            ExternalId = externalId ?? "acct_test",
            Id = Guid.NewGuid(),
            IdentityDocumentsProvidedUtc = setupComplete ? utcNow : null,
            OnboardingCompletedUtc = setupComplete ? utcNow : null,
            Environment = EnvironmentType.Dev,
            OnboardingUrl = null,
            OwnerId = chapter.OwnerId,
            PaymentProvider = PaymentProviderType.Stripe
        });
    }

    internal ChapterSubscription CreateChapterSubscription(
        Chapter? chapter = null,
        Currency? currency = null)
    {
        currency ??= CreateCurrency();
        chapter ??= CreateChapter();

        return Create(new ChapterSubscription
        {
            ChapterId = chapter.Id,
            Currency = currency,
            CurrencyId = currency.Id,
            Environment = EnvironmentType.Dev,
            Id = Guid.NewGuid(),
            PaymentProvider = PaymentProviderType.Stripe
        });
    }

    internal Country CreateCountry(
        Currency? currency = null,
        string? isoCode2 = null)
    {
        currency ??= CreateCurrency();
        return Create(new Country
        {
            Continent = "",
            CurrencyId = currency.Id,
            Id = Guid.NewGuid(),
            IsoCode2 = isoCode2 ?? "GB",
            IsoCode3 = "",
            Name = ""
        });
    }

    internal Currency CreateCurrency(string? code = null) => Create(new Currency
    {
        Code = code ?? "GBP",
        Id = Guid.NewGuid()
    });

    internal Event CreateEvent(
        Chapter? chapter = null,
        Venue? venue = null,
        DateTime? date = null)
    {
        chapter ??= CreateChapter();
        venue ??= CreateVenue(chapter);

        return Create(new Event
        {
            ChapterId = chapter.Id,
            DateUtc = date ?? DateTime.UtcNow.AddDays(5),
            Id = Guid.NewGuid(),
            PublishedUtc = DateTime.UtcNow,
            VenueId = venue.Id
        });
    }

    internal Member CreateMember(
        bool activated = true,
        bool siteAdmin = false,
        bool createSiteSubscription = false,
        Action<Member>? afterCreate = null,
        TimeZoneInfo? timeZone = null)
    {
        var id = Guid.NewGuid();

        var member = Create(new Member
        {
            Activated = activated,
            Id = id,
            Chapters = [],
            SiteAdmin = siteAdmin,
            TimeZone = timeZone ?? TimeZoneInfo.Utc
        });

        if (createSiteSubscription)
        {
            CreateMemberSiteSubscription(member);
        }

        afterCreate?.Invoke(member);

        return member;
    }

    internal void CreateMemberSiteSubscription(
        Member member,
        SiteSubscription? siteSubscription = null,
        DateTime? expiresUtc = null,
        SiteSubscriptionPrice? siteSubscriptionPrice = null)
    {
        siteSubscription ??= CreateSiteSubscription();

        // The current MemberSiteSubscriptionLog record is the sole store read for feature gating.
        Create(new MemberSiteSubscriptionRecord
        {
            CreatedUtc = DateTime.UtcNow,
            ExpiresUtc = expiresUtc,
            Id = Guid.NewGuid(),
            IsCurrent = true,
            MemberId = member.Id,
            SiteSubscriptionId = siteSubscription.Id,
            SiteSubscriptionPriceId = siteSubscriptionPrice?.Id
        });
    }

    internal Payment CreatePayment(
        Currency? currency = null,
        Member? member = null,
        Chapter? chapter = null,
        DateTime? paidUtc = null,
        PlatformType platform = PlatformType.Default)
    {
        currency ??= CreateCurrency();
        member ??= CreateMember();

        return Create(new Payment
        {
            Id = Guid.NewGuid(),
            Amount = 100m,
            ChapterId = chapter?.Id,
            CreatedUtc = DateTime.UtcNow,
            CurrencyId = currency.Id,
            Environment = EnvironmentType.Dev,
            MemberId = member.Id,
            PaidUtc = paidUtc,
            PaymentProvider = PaymentProviderType.Stripe,
            Platform = chapter?.Platform ?? platform,
            Reference = "REF123"
        });
    }

    internal PaymentCheckoutSession CreatePaymentCheckoutSession(
        Payment? payment = null,
        DateTime? completedUtc = null,
        DateTime? expiredUtc = null,
        string sessionId = "cs_test")
    {
        payment ??= CreatePayment();

        return Create(new PaymentCheckoutSession
        {
            Id = Guid.NewGuid(),
            MemberId = payment.MemberId,
            PaymentId = payment.Id,
            CompletedUtc = completedUtc,
            ExpiredUtc = expiredUtc,
            SessionId = sessionId
        });
    }

    internal SitePaymentProduct CreateSitePaymentProduct(
        PlatformType platform = PlatformType.Default,
        string externalId = "product-external-id")
    {
        return Create(new SitePaymentProduct
        {
            Environment = EnvironmentType.Dev,
            ExternalId = externalId,
            Id = Guid.NewGuid(),
            PaymentProvider = PaymentProviderType.Stripe,
            Platform = platform
        });
    }

    internal SiteSubscription CreateSiteSubscription(
        int? groupLimit = null,
        IEnumerable<SiteFeatureType>? features = null,
        bool free = false,
        int? memberLimit = null,
        PlatformType platform = PlatformType.Default,
        SitePaymentProduct? sitePaymentProduct = null)
    {
        sitePaymentProduct ??= CreateSitePaymentProduct(platform);

        var siteSubscription = Create(new SiteSubscription
        {
            Id = Guid.NewGuid(),
            Name = "Test Subscription",
            DescriptionHtml = "Test subscription for testing",
            Free = free,
            GroupLimit = groupLimit ?? 10,
            MemberLimit = memberLimit,
            Enabled = true,
            Default = false,
            Environment = EnvironmentType.Dev,
            PaymentProvider = PaymentProviderType.Stripe,
            Platform = platform,
            SitePaymentProductId = sitePaymentProduct.Id
        });

        if (features != null)
        {
            foreach (var feature in features)
            {
                Create(new SiteSubscriptionFeature
                {
                    Feature = feature,
                    Id = Guid.NewGuid(),
                    SiteSubscriptionId = siteSubscription.Id
                });
            }
        }

        return siteSubscription;
    }

    internal SiteSubscriptionPrice CreateSiteSubscriptionPrice(
        SiteSubscription? siteSubscription = null,
        Currency? currency = null,
        decimal amount = 100)
    {
        currency ??= CreateCurrency();
        siteSubscription ??= CreateSiteSubscription();

        return Create(new SiteSubscriptionPrice
        {
            Amount = amount,
            Currency = currency,
            CurrencyId = currency.Id,
            ExternalId = "external_id",
            Frequency = SiteSubscriptionFrequency.Yearly,
            Id = Guid.NewGuid(),
            SiteSubscriptionId = siteSubscription.Id
        });
    }

    internal Venue CreateVenue(Chapter chapter, string name = "", string slug = "") => Create(new Venue
    {
        ChapterId = chapter.Id,
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug
    });

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // generate unique DB name per-test
        options.UseInMemoryDatabase($"odk-{Guid.NewGuid()}");

        if (_noTracking)
        {
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }
}