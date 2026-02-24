using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Subscriptions;
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

    internal MockOdkContext SetupChapter(
        Chapter chapter,
        SiteSubscription? siteSubscription = null,
        IEnumerable<Member>? adminMembers = null)
        => SetupChapter(
            chapter,
            adminMembers?.Select(x => new ChapterAdminMember
            {
                ChapterId = chapter.Id,
                MemberId = x.Id
            }),
            siteSubscription);

    internal MockOdkContext SetupChapter(
        Chapter chapter,
        IEnumerable<ChapterAdminMember>? adminMembers,
        SiteSubscription? siteSubscription)
    {
        Add(chapter);

        if (siteSubscription != null)
        {
            Add(siteSubscription);
            Add(new MemberSiteSubscription
            {
                MemberId = chapter.OwnerId,
                SiteSubscriptionId = siteSubscription.Id
            });
        }

        if (adminMembers != null)
        {
            AddRange(adminMembers);
        }

        return this;
    }

    internal MockOdkContext SetupMember(
        Member member,
        SiteSubscription? siteSubscription = null)
    {
        Add(member);

        if (siteSubscription != null)
        {
            Add(siteSubscription);
            Add(new MemberSiteSubscription
            {
                MemberId = member.Id,
                SiteSubscriptionId = siteSubscription.Id
            });
        }

        return this;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseInMemoryDatabase("odk");
    }
}