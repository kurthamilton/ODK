using System;
using ODK.Core.Chapters;

namespace ODK.Core.Members
{
    public interface IMember
    {
        int? AdminUserId { get; }

        ChapterModel Chapter { get; }

        bool Disabled { get; }

        string Email { get; }

        bool EmailOptIn { get; }
        
        string FacebookProfile { get; }

        string FavouriteBeverage { get; }

        string FirstName { get; }

        string FullName { get; }

        string Hometown { get; }

        int Id { get; }
        
        DateTime? Joined { get; }

        string KnittingExperience { get; }
        
        string KnittingExperienceOther { get; }
        
        string LastName { get; }

        double? LastPaymentAmount { get; }

        DateTime? LastPaymentDate { get; }
        
        string Neighbourhood { get; }

        string PictureThumbnailUrl { get; }

        string PictureUrl { get; }
        
        string Reason { get; }

        DateTime? SubscriptionEndDate { get; }

        SubscriptionStatus SubscriptionStatus { get; }

        MemberTypes Type { get; }
    }
}
