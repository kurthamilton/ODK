using System;
using System.Collections.Generic;
using System.Linq;
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
    public MockOdkContext()
        : base(new OdkContextSettings(""))
    {
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
            Platform = PlatformType.Default,
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

    internal ChapterSubscription CreateChapterSubscription(
        Chapter? chapter = null,
        SitePaymentSettings? sitePaymentSettings = null,
        Currency? currency = null)
    {
        currency ??= CreateCurrency();
        sitePaymentSettings ??= CreateSitePaymentSettings();
        chapter ??= CreateChapter();

        return Create(new ChapterSubscription
        {
            ChapterId = chapter.Id,
            Currency = currency,
            CurrencyId = currency.Id,
            Id = Guid.NewGuid(),
            SitePaymentSettingId = sitePaymentSettings.Id            
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

    internal Currency CreateCurrency() => Create(new Currency
    {
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
            Date = date ?? DateTime.UtcNow.AddDays(5),
            Id = Guid.NewGuid(),
            PublishedUtc = DateTime.UtcNow,
            VenueId = venue.Id
        });
    }

    internal Member CreateMember(
        bool activated = true,
        bool siteAdmin = false,
        bool createSiteSubscription = false,
        Action<Member>? afterCreate = null)
    {
        var id = Guid.NewGuid();

        var member = Create(new Member
        {
            Activated = activated,
            Id = id,
            Chapters = [],
            SiteAdmin = siteAdmin
        });

        if (createSiteSubscription)
        {
            CreateMemberSiteSubscription(member);
        }

        afterCreate?.Invoke(member);

        return member;
    }

    internal MemberSiteSubscription CreateMemberSiteSubscription(
        Member member,
        SiteSubscription? siteSubscription = null,
        DateTime? expiresUtc = null)
    {
        siteSubscription ??= CreateSiteSubscription();

        return Create(new MemberSiteSubscription
        {
            ExpiresUtc = expiresUtc,
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            SiteSubscriptionId = siteSubscription.Id
        });
    }

    internal Payment CreatePayment(
        Currency? currency = null,
        Member? member = null,
        SitePaymentSettings? sitePaymentSettings = null,
        Chapter? chapter = null,
        DateTime? paidUtc = null)
    {
        currency ??= CreateCurrency();
        member ??= CreateMember();
        sitePaymentSettings ??= CreateSitePaymentSettings();

        return Create(new Payment
        {
            Id = Guid.NewGuid(),
            Amount = 100m,
            ChapterId = chapter?.Id,
            CreatedUtc = DateTime.UtcNow,
            CurrencyId = currency.Id,
            MemberId = member.Id,
            PaidUtc = paidUtc,
            Reference = "REF123",
            SitePaymentSettingId = sitePaymentSettings.Id
        });
    }

    internal PaymentCheckoutSession CreatePaymentCheckoutSession(
        Payment? payment = null,
        DateTime? completedUtc = null,
        DateTime? expiredUtc = null)
    {
        payment ??= CreatePayment();

        return Create(new PaymentCheckoutSession
        {
            Id = Guid.NewGuid(),
            MemberId = payment.MemberId,
            PaymentId = payment.Id,
            CompletedUtc = completedUtc,
            ExpiredUtc = expiredUtc
        });
    }

    internal SitePaymentSettings CreateSitePaymentSettings()
    {
        return Create(new SitePaymentSettings
        {
            Active = true,
            Enabled = true,
            Id = Guid.NewGuid()
        });
    }

    internal SiteSubscription CreateSiteSubscription(
        IEnumerable<SiteFeatureType>? features = null,
        int? groupLimit = null)
    {
        return Create(new SiteSubscription
        {
            Id = Guid.NewGuid(),
            Name = "Test Subscription",
            Description = "Test subscription for testing",
            GroupLimit = groupLimit ?? 10,
            Enabled = true,
            Default = false,
            Platform = PlatformType.Default,
            SitePaymentSettingId = Guid.NewGuid(),
            Features = features?.Select(x => new SiteSubscriptionFeature
            {
                Id = Guid.NewGuid(),
                Feature = x
            }).ToList() ?? []
        });
    }

    internal SiteSubscriptionPrice CreateSiteSubscriptionPrice(
        SiteSubscription? siteSubscription = null,
        Currency? currency = null)
    {
        currency ??= CreateCurrency();
        siteSubscription ??= CreateSiteSubscription();

        return Create(new SiteSubscriptionPrice
        {
            Amount = 100,
            Currency = currency,
            CurrencyId = currency.Id,
            ExternalId = "external_id",
            Frequency = SiteSubscriptionFrequency.Yearly,
            Id = Guid.NewGuid(),
            SiteSubscriptionId = siteSubscription.Id
        });
    }

    internal Venue CreateVenue(Chapter chapter) => Create(new Venue
    {
        ChapterId = chapter.Id,
        Id = Guid.NewGuid()
    });

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // generate unique DB name per-test
        options.UseInMemoryDatabase($"odk-{Guid.NewGuid()}");
    }
}