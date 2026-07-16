using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ChapterContactMessages",
            columns: table => new
            {
                ChapterContactMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                FromAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RecaptchaScore = table.Column<double>(type: "float", nullable: true),
                RepliedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterContactMessages", x => x.ChapterContactMessageId);
            });

        migrationBuilder.CreateTable(
            name: "ChapterEmails",
            columns: table => new
            {
                ChapterEmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                EmailTypeId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterEmails", x => x.ChapterEmailId);
            });

        migrationBuilder.CreateTable(
            name: "ChapterEventSettings",
            columns: table => new
            {
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DefaultDayOfWeek = table.Column<int>(type: "int", nullable: true),
                DefaultDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DefaultEndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                DefaultScheduledEmailDayOfWeek = table.Column<int>(type: "int", nullable: true),
                DefaultScheduledEmailTimeOfDay = table.Column<TimeSpan>(type: "time", nullable: true),
                DefaultStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                DisableComments = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterEventSettings", x => x.ChapterId);
            });

        migrationBuilder.CreateTable(
            name: "ChapterLinks",
            columns: table => new
            {
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FacebookName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                InstagramName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TwitterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                WhatsApp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterLinks", x => x.ChapterId);
            });

        migrationBuilder.CreateTable(
            name: "ChapterMembershipSettings",
            columns: table => new
            {
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ApproveNewMembers = table.Column<bool>(type: "bit", nullable: false),
                Enabled = table.Column<bool>(type: "bit", nullable: false),
                MembershipDisabledAfterDaysExpired = table.Column<int>(type: "int", nullable: false),
                MembershipExpiringWarningDays = table.Column<int>(type: "int", nullable: false),
                TrialPeriodMonths = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterMembershipSettings", x => x.ChapterId);
            });

        migrationBuilder.CreateTable(
            name: "ChapterProperties",
            columns: table => new
            {
                ChapterPropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Hidden = table.Column<bool>(type: "bit", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DataTypeId = table.Column<int>(type: "int", nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DisplayOrder = table.Column<int>(type: "int", nullable: false),
                HelpText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Required = table.Column<bool>(type: "bit", nullable: false),
                Subtitle = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterProperties", x => x.ChapterPropertyId);
            });

        migrationBuilder.CreateTable(
            name: "ChapterPropertyOptions",
            columns: table => new
            {
                ChapterPropertyOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChapterPropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DisplayOrder = table.Column<int>(type: "int", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterPropertyOptions", x => x.ChapterPropertyOptionId);
            });

        migrationBuilder.CreateTable(
            name: "ChapterQuestions",
            columns: table => new
            {
                ChapterQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DisplayOrder = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterQuestions", x => x.ChapterQuestionId);
            });

        migrationBuilder.CreateTable(
            name: "Chapters",
            columns: table => new
            {
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ApprovedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                BannerImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                DisplayOrder = table.Column<int>(type: "int", nullable: true),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PlatformTypeId = table.Column<int>(type: "int", nullable: false),
                PublishedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RedirectUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ThemeBackground = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ThemeColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Chapters", x => x.ChapterId);
            });

        migrationBuilder.CreateTable(
            name: "Currencies",
            columns: table => new
            {
                CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Currencies", x => x.CurrencyId);
            });

        migrationBuilder.CreateTable(
            name: "Emails",
            columns: table => new
            {
                EmailTypeId = table.Column<int>(type: "int", nullable: false),
                Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Overridable = table.Column<bool>(type: "bit", nullable: false),
                Subject = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Emails", x => x.EmailTypeId);
            });

        migrationBuilder.CreateTable(
            name: "Errors",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ExceptionMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ExceptionType = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Errors", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "EventInvites",
            columns: table => new
            {
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventInvites", x => new { x.EventId, x.MemberId });
            });

        migrationBuilder.CreateTable(
            name: "EventResponses",
            columns: table => new
            {
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ResponseTypeId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventResponses", x => new { x.EventId, x.MemberId });
            });

        migrationBuilder.CreateTable(
            name: "Events",
            columns: table => new
            {
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AttendeeLimit = table.Column<int>(type: "int", nullable: true),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsPublic = table.Column<bool>(type: "bit", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PublishedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RsvpDeadlineUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RsvpDisabled = table.Column<bool>(type: "bit", nullable: false),
                Shortcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Time = table.Column<string>(type: "nvarchar(max)", nullable: true),
                VenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WaitlistDisabled = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Events", x => x.EventId);
            });

        migrationBuilder.CreateTable(
            name: "Features",
            columns: table => new
            {
                FeatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Features", x => x.FeatureId);
            });

        migrationBuilder.CreateTable(
            name: "InstagramFetchLog",
            columns: table => new
            {
                InstagramFetchLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                DelaySeconds = table.Column<int>(type: "int", nullable: false),
                Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Success = table.Column<bool>(type: "bit", nullable: false),
                Username = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InstagramFetchLog", x => x.InstagramFetchLogId);
            });

        migrationBuilder.CreateTable(
            name: "InstagramPosts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Caption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InstagramPosts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Members",
            columns: table => new
            {
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Activated = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                SuperAdmin = table.Column<bool>(type: "bit", nullable: false),
                TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Members", x => x.MemberId);
            });

        migrationBuilder.CreateTable(
            name: "PaymentProviderWebhookEvents",
            columns: table => new
            {
                ExternalId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                PaymentProviderId = table.Column<int>(type: "int", nullable: false),
                ReceivedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaymentProviderWebhookEvents", x => new { x.PaymentProviderId, x.ExternalId });
            });

        migrationBuilder.CreateTable(
            name: "SentEmails",
            columns: table => new
            {
                SentEmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                To = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SentEmails", x => x.SentEmailId);
            });

        migrationBuilder.CreateTable(
            name: "SiteContactMessages",
            columns: table => new
            {
                SiteContactMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                FromAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RecaptchaScore = table.Column<double>(type: "float", nullable: true),
                RepliedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SiteContactMessages", x => x.SiteContactMessageId);
            });

        migrationBuilder.CreateTable(
            name: "SiteEmailSettings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FromEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                FromName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PlatformTypeId = table.Column<int>(type: "int", nullable: false),
                PlatformTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SiteEmailSettings", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SitePaymentSettings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Active = table.Column<bool>(type: "bit", nullable: false),
                ApiPublicKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ApiSecretKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Commission = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                Enabled = table.Column<bool>(type: "bit", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Provider = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SitePaymentSettings", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TopicGroups",
            columns: table => new
            {
                TopicGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TopicGroups", x => x.TopicGroupId);
            });

        migrationBuilder.CreateTable(
            name: "Venues",
            columns: table => new
            {
                VenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ArchivedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MapQuery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Venues", x => x.VenueId);
            });

        migrationBuilder.CreateTable(
            name: "ChapterImages",
            columns: table => new
            {
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                MimeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                VersionInt = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterImages", x => x.ChapterId);
                table.ForeignKey(
                    name: "FK_ChapterImages_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterLocations",
            columns: table => new
            {
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Latitude = table.Column<double>(type: "float", nullable: false),
                Longitude = table.Column<double>(type: "float", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LatLong = table.Column<Point>(type: "geography", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterLocations", x => x.ChapterId);
                table.ForeignKey(
                    name: "FK_ChapterLocations_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterPages",
            columns: table => new
            {
                ChapterPageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PageTypeId = table.Column<int>(type: "int", nullable: false),
                Hidden = table.Column<bool>(type: "bit", nullable: false),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterPages", x => x.ChapterPageId);
                table.ForeignKey(
                    name: "FK_ChapterPages_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterPrivacySettings",
            columns: table => new
            {
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventResponseVisibility = table.Column<int>(type: "int", nullable: true),
                EventVisibility = table.Column<int>(type: "int", nullable: true),
                InstagramFeed = table.Column<bool>(type: "bit", nullable: true),
                MemberVisibility = table.Column<int>(type: "int", nullable: true),
                VenueVisibility = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterPrivacySettings", x => x.ChapterId);
                table.ForeignKey(
                    name: "FK_ChapterPrivacySettings_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterTexts",
            columns: table => new
            {
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RegisterText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                WelcomeText = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterTexts", x => x.ChapterId);
                table.ForeignKey(
                    name: "FK_ChapterTexts_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "QueuedEmails",
            columns: table => new
            {
                QueuedEmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                FromEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                FromName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                SendAfterUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                Subject = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QueuedEmails", x => x.QueuedEmailId);
                table.ForeignKey(
                    name: "FK_QueuedEmails_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId");
            });

        migrationBuilder.CreateTable(
            name: "ChapterPaymentSettings",
            columns: table => new
            {
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ExternalProductId = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterPaymentSettings", x => x.ChapterId);
                table.ForeignKey(
                    name: "FK_ChapterPaymentSettings_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChapterPaymentSettings_Currencies_CurrencyId",
                    column: x => x.CurrencyId,
                    principalTable: "Currencies",
                    principalColumn: "CurrencyId");
            });

        migrationBuilder.CreateTable(
            name: "Countries",
            columns: table => new
            {
                CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Continent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsoCode2 = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                IsoCode3 = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Countries", x => x.CountryId);
                table.ForeignKey(
                    name: "FK_Countries_Currencies_CurrencyId",
                    column: x => x.CurrencyId,
                    principalTable: "Currencies",
                    principalColumn: "CurrencyId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                SitePaymentSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.PaymentId);
                table.ForeignKey(
                    name: "FK_Payments_Currencies_CurrencyId",
                    column: x => x.CurrencyId,
                    principalTable: "Currencies",
                    principalColumn: "CurrencyId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ErrorProperties",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ErrorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ErrorProperties", x => x.Id);
                table.ForeignKey(
                    name: "FK_ErrorProperties_Errors_ErrorId",
                    column: x => x.ErrorId,
                    principalTable: "Errors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EventEmails",
            columns: table => new
            {
                EventEmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                JobId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                SentDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventEmails", x => x.EventEmailId);
                table.ForeignKey(
                    name: "FK_EventEmails_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "EventId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EventTicketSettings",
            columns: table => new
            {
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Deposit = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventTicketSettings", x => x.EventId);
                table.ForeignKey(
                    name: "FK_EventTicketSettings_Currencies_CurrencyId",
                    column: x => x.CurrencyId,
                    principalTable: "Currencies",
                    principalColumn: "CurrencyId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EventTicketSettings_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "EventId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "InstagramImages",
            columns: table => new
            {
                InstagramImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Alt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DisplayOrder = table.Column<int>(type: "int", nullable: true),
                ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Height = table.Column<int>(type: "int", nullable: true),
                ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                InstagramPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsVideo = table.Column<bool>(type: "bit", nullable: false),
                MimeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                VersionInt = table.Column<int>(type: "int", nullable: false),
                Width = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InstagramImages", x => x.InstagramImageId);
                table.ForeignKey(
                    name: "FK_InstagramImages_InstagramPosts_InstagramPostId",
                    column: x => x.InstagramPostId,
                    principalTable: "InstagramPosts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterAdminMembers",
            columns: table => new
            {
                ChapterAdminMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AdminEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReceiveContactEmails = table.Column<bool>(type: "bit", nullable: false),
                ReceiveEventCommentEmails = table.Column<bool>(type: "bit", nullable: false),
                ReceiveNewMemberEmails = table.Column<bool>(type: "bit", nullable: false),
                ChapterAdminRoleId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterAdminMembers", x => x.ChapterAdminMemberId);
                table.ForeignKey(
                    name: "FK_ChapterAdminMembers_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterContactMessageReplies",
            columns: table => new
            {
                ChapterContactMessageReplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChapterContactMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Message = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterContactMessageReplies", x => x.ChapterContactMessageReplyId);
                table.ForeignKey(
                    name: "FK_ChapterContactMessageReplies_ChapterContactMessages_ChapterContactMessageId",
                    column: x => x.ChapterContactMessageId,
                    principalTable: "ChapterContactMessages",
                    principalColumn: "ChapterContactMessageId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChapterContactMessageReplies_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterConversations",
            columns: table => new
            {
                ChapterConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ArchivedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Subject = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterConversations", x => x.ChapterConversationId);
                table.ForeignKey(
                    name: "FK_ChapterConversations_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChapterConversations_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EventComments",
            columns: table => new
            {
                EventCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Hidden = table.Column<bool>(type: "bit", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ParentEventCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventComments", x => x.EventCommentId);
                table.ForeignKey(
                    name: "FK_EventComments_EventComments_ParentEventCommentId",
                    column: x => x.ParentEventCommentId,
                    principalTable: "EventComments",
                    principalColumn: "EventCommentId");
                table.ForeignKey(
                    name: "FK_EventComments_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "EventId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EventComments_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EventHosts",
            columns: table => new
            {
                EventHostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventHosts", x => x.EventHostId);
                table.ForeignKey(
                    name: "FK_EventHosts_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "EventId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EventHosts_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EventWaitlistMembers",
            columns: table => new
            {
                EventWaitlistMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventWaitlistMembers", x => x.EventWaitlistMemberId);
                table.ForeignKey(
                    name: "FK_EventWaitlistMembers_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "EventId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EventWaitlistMembers_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "FeatureSeenByMembers",
            columns: table => new
            {
                FeatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FeatureSeenByMembers", x => new { x.FeatureId, x.MemberId });
                table.ForeignKey(
                    name: "FK_FeatureSeenByMembers_Features_FeatureId",
                    column: x => x.FeatureId,
                    principalTable: "Features",
                    principalColumn: "FeatureId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_FeatureSeenByMembers_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Issues",
            columns: table => new
            {
                IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClosedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IssueStatusTypeId = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IssueTypeId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Issues", x => x.IssueId);
                table.ForeignKey(
                    name: "FK_Issues_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberActivationTokens",
            columns: table => new
            {
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ActivationToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberActivationTokens", x => x.MemberId);
                table.ForeignKey(
                    name: "FK_MemberActivationTokens_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberAvatars",
            columns: table => new
            {
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CropX = table.Column<int>(type: "int", nullable: false),
                CropY = table.Column<int>(type: "int", nullable: false),
                CropWidth = table.Column<int>(type: "int", nullable: false),
                CropHeight = table.Column<int>(type: "int", nullable: false),
                ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                MimeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                VersionInt = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberAvatars", x => x.MemberId);
                table.ForeignKey(
                    name: "FK_MemberAvatars_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberChapters",
            columns: table => new
            {
                MemberChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Approved = table.Column<bool>(type: "bit", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                HideProfile = table.Column<bool>(type: "bit", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberChapters", x => x.MemberChapterId);
                table.ForeignKey(
                    name: "FK_MemberChapters_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MemberChapters_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberEmailAddressUpdateTokens",
            columns: table => new
            {
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ConfirmationToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                NewEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberEmailAddressUpdateTokens", x => x.MemberId);
                table.ForeignKey(
                    name: "FK_MemberEmailAddressUpdateTokens_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberEmailPreferences",
            columns: table => new
            {
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MemberEmailPreferenceTypeId = table.Column<int>(type: "int", nullable: false),
                Disabled = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberEmailPreferences", x => new { x.MemberId, x.MemberEmailPreferenceTypeId });
                table.ForeignKey(
                    name: "FK_MemberEmailPreferences_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberNotificationSettings",
            columns: table => new
            {
                MemberNotificationSettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Disabled = table.Column<bool>(type: "bit", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                NotificationTypeId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberNotificationSettings", x => x.MemberNotificationSettingsId);
                table.ForeignKey(
                    name: "FK_MemberNotificationSettings_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberPasswordResetRequests",
            columns: table => new
            {
                MemberPasswordResetRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                Expires = table.Column<DateTime>(type: "datetime2", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Token = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberPasswordResetRequests", x => x.MemberPasswordResetRequestId);
                table.ForeignKey(
                    name: "FK_MemberPasswordResetRequests_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberPasswords",
            columns: table => new
            {
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Algorithm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Iterations = table.Column<int>(type: "int", nullable: false),
                Salt = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberPasswords", x => x.MemberId);
                table.ForeignKey(
                    name: "FK_MemberPasswords_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberPaymentSettings",
            columns: table => new
            {
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberPaymentSettings", x => x.MemberId);
                table.ForeignKey(
                    name: "FK_MemberPaymentSettings_Currencies_CurrencyId",
                    column: x => x.CurrencyId,
                    principalTable: "Currencies",
                    principalColumn: "CurrencyId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MemberPaymentSettings_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberPreferences",
            columns: table => new
            {
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DistanceUnitTypeId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberPreferences", x => x.MemberId);
                table.ForeignKey(
                    name: "FK_MemberPreferences_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberProperties",
            columns: table => new
            {
                MemberPropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChapterPropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberProperties", x => x.MemberPropertyId);
                table.ForeignKey(
                    name: "FK_MemberProperties_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "NewChapterTopics",
            columns: table => new
            {
                NewChapterTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Topic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                TopicGroup = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NewChapterTopics", x => x.NewChapterTopicId);
                table.ForeignKey(
                    name: "FK_NewChapterTopics_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_NewChapterTopics_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "NewMemberTopics",
            columns: table => new
            {
                NewMemberTopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Topic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                TopicGroup = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NewMemberTopics", x => x.NewMemberTopicId);
                table.ForeignKey(
                    name: "FK_NewMemberTopics_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Notifications",
            columns: table => new
            {
                NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReadUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                NotificationTypeId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                table.ForeignKey(
                    name: "FK_Notifications_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId");
                table.ForeignKey(
                    name: "FK_Notifications_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PaymentCheckoutSessions",
            columns: table => new
            {
                PaymentCheckoutSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                ExpiredUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SessionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                StartedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaymentCheckoutSessions", x => x.PaymentCheckoutSessionId);
                table.ForeignKey(
                    name: "FK_PaymentCheckoutSessions_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SentEmailEvents",
            columns: table => new
            {
                SentEmailEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                EventName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                SentEmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SentEmailEvents", x => x.SentEmailEventId);
                table.ForeignKey(
                    name: "FK_SentEmailEvents_SentEmails_SentEmailId",
                    column: x => x.SentEmailId,
                    principalTable: "SentEmails",
                    principalColumn: "SentEmailId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SiteContactMessageReplies",
            columns: table => new
            {
                SiteContactMessageReplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteContactMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Message = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SiteContactMessageReplies", x => x.SiteContactMessageReplyId);
                table.ForeignKey(
                    name: "FK_SiteContactMessageReplies_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SiteContactMessageReplies_SiteContactMessages_SiteContactMessageId",
                    column: x => x.SiteContactMessageId,
                    principalTable: "SiteContactMessages",
                    principalColumn: "SiteContactMessageId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterPaymentAccounts",
            columns: table => new
            {
                ChapterPaymentAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CardPaymentsEnabledUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IdentityDocumentsProvidedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                OnboardingCompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                OnboardingUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SitePaymentSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterPaymentAccounts", x => x.ChapterPaymentAccountId);
                table.ForeignKey(
                    name: "FK_ChapterPaymentAccounts_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChapterPaymentAccounts_Members_OwnerId",
                    column: x => x.OwnerId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChapterPaymentAccounts_SitePaymentSettings_SitePaymentSettingId",
                    column: x => x.SitePaymentSettingId,
                    principalTable: "SitePaymentSettings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterSubscriptions",
            columns: table => new
            {
                ChapterSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<double>(type: "float", nullable: false),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Disabled = table.Column<bool>(type: "bit", nullable: false),
                ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ExternalProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Months = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Recurring = table.Column<bool>(type: "bit", nullable: false),
                SitePaymentSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                SubscriptionTypeId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterSubscriptions", x => x.ChapterSubscriptionId);
                table.ForeignKey(
                    name: "FK_ChapterSubscriptions_Currencies_CurrencyId",
                    column: x => x.CurrencyId,
                    principalTable: "Currencies",
                    principalColumn: "CurrencyId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChapterSubscriptions_SitePaymentSettings_SitePaymentSettingId",
                    column: x => x.SitePaymentSettingId,
                    principalTable: "SitePaymentSettings",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "SiteSubscriptions",
            columns: table => new
            {
                SiteSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Default = table.Column<bool>(type: "bit", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DisplayOrder = table.Column<int>(type: "int", nullable: true),
                Enabled = table.Column<bool>(type: "bit", nullable: false),
                ExternalProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                FallbackSiteSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                GroupLimit = table.Column<int>(type: "int", nullable: true),
                MemberLimit = table.Column<int>(type: "int", nullable: true),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PlatformTypeId = table.Column<int>(type: "int", nullable: false),
                SitePaymentSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SiteSubscriptions", x => x.SiteSubscriptionId);
                table.ForeignKey(
                    name: "FK_SiteSubscriptions_SitePaymentSettings_SitePaymentSettingId",
                    column: x => x.SitePaymentSettingId,
                    principalTable: "SitePaymentSettings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SiteSubscriptions_SiteSubscriptions_FallbackSiteSubscriptionId",
                    column: x => x.FallbackSiteSubscriptionId,
                    principalTable: "SiteSubscriptions",
                    principalColumn: "SiteSubscriptionId");
            });

        migrationBuilder.CreateTable(
            name: "Topics",
            columns: table => new
            {
                TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Public = table.Column<bool>(type: "bit", nullable: false),
                TopicGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Topics", x => x.TopicId);
                table.ForeignKey(
                    name: "FK_Topics_TopicGroups_TopicGroupId",
                    column: x => x.TopicGroupId,
                    principalTable: "TopicGroups",
                    principalColumn: "TopicGroupId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "VenueLocations",
            columns: table => new
            {
                VenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Latitude = table.Column<double>(type: "float", nullable: false),
                Longitude = table.Column<double>(type: "float", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LatLong = table.Column<Point>(type: "geography", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VenueLocations", x => x.VenueId);
                table.ForeignKey(
                    name: "FK_VenueLocations_Venues_VenueId",
                    column: x => x.VenueId,
                    principalTable: "Venues",
                    principalColumn: "VenueId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "QueuedEmailRecipients",
            columns: table => new
            {
                QueuedEmailRecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                QueuedEmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QueuedEmailRecipients", x => x.QueuedEmailRecipientId);
                table.ForeignKey(
                    name: "FK_QueuedEmailRecipients_QueuedEmails_QueuedEmailId",
                    column: x => x.QueuedEmailId,
                    principalTable: "QueuedEmails",
                    principalColumn: "QueuedEmailId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberLocations",
            columns: table => new
            {
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Latitude = table.Column<double>(type: "float", nullable: false),
                Longitude = table.Column<double>(type: "float", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LatLong = table.Column<Point>(type: "geography", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberLocations", x => x.MemberId);
                table.ForeignKey(
                    name: "FK_MemberLocations_Countries_CountryId",
                    column: x => x.CountryId,
                    principalTable: "Countries",
                    principalColumn: "CountryId");
                table.ForeignKey(
                    name: "FK_MemberLocations_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EventTicketPayments",
            columns: table => new
            {
                EventTicketPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventTicketPayments", x => x.EventTicketPaymentId);
                table.ForeignKey(
                    name: "FK_EventTicketPayments_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "EventId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EventTicketPayments_Payments_PaymentId",
                    column: x => x.PaymentId,
                    principalTable: "Payments",
                    principalColumn: "PaymentId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterConversationMessages",
            columns: table => new
            {
                ChapterConversationMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChapterConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReadByChapter = table.Column<bool>(type: "bit", nullable: false),
                ReadByMember = table.Column<bool>(type: "bit", nullable: false),
                RecaptchaScore = table.Column<double>(type: "float", nullable: true),
                Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterConversationMessages", x => x.ChapterConversationMessageId);
                table.ForeignKey(
                    name: "FK_ChapterConversationMessages_ChapterConversations_ChapterConversationId",
                    column: x => x.ChapterConversationId,
                    principalTable: "ChapterConversations",
                    principalColumn: "ChapterConversationId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChapterConversationMessages_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "IssueMessages",
            columns: table => new
            {
                IssueMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IssueMessages", x => x.IssueMessageId);
                table.ForeignKey(
                    name: "FK_IssueMessages_Issues_IssueId",
                    column: x => x.IssueId,
                    principalTable: "Issues",
                    principalColumn: "IssueId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_IssueMessages_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberChapterNotificationSettings",
            columns: table => new
            {
                MemberChapterNotificationSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Disabled = table.Column<bool>(type: "bit", nullable: false),
                MemberChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                NotificationTypeId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberChapterNotificationSettings", x => x.MemberChapterNotificationSettingId);
                table.ForeignKey(
                    name: "FK_MemberChapterNotificationSettings_MemberChapters_MemberChapterId",
                    column: x => x.MemberChapterId,
                    principalTable: "MemberChapters",
                    principalColumn: "MemberChapterId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberSubscriptions",
            columns: table => new
            {
                MemberChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ReminderEmailSentUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                SubscriptionTypeId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberSubscriptions", x => x.MemberChapterId);
                table.ForeignKey(
                    name: "FK_MemberSubscriptions_MemberChapters_MemberChapterId",
                    column: x => x.MemberChapterId,
                    principalTable: "MemberChapters",
                    principalColumn: "MemberChapterId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberSubscriptionLog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<double>(type: "float", nullable: false),
                CancelledUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChapterSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Months = table.Column<int>(type: "int", nullable: false),
                PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                SubscriptionTypeId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberSubscriptionLog", x => x.Id);
                table.ForeignKey(
                    name: "FK_MemberSubscriptionLog_ChapterSubscriptions_ChapterSubscriptionId",
                    column: x => x.ChapterSubscriptionId,
                    principalTable: "ChapterSubscriptions",
                    principalColumn: "ChapterSubscriptionId");
                table.ForeignKey(
                    name: "FK_MemberSubscriptionLog_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MemberSubscriptionLog_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MemberSubscriptionLog_Payments_PaymentId",
                    column: x => x.PaymentId,
                    principalTable: "Payments",
                    principalColumn: "PaymentId");
            });

        migrationBuilder.CreateTable(
            name: "SiteSubscriptionFeatures",
            columns: table => new
            {
                SiteSubscriptionFeatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteFeatureId = table.Column<int>(type: "int", nullable: false),
                SiteSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SiteSubscriptionFeatures", x => x.SiteSubscriptionFeatureId);
                table.ForeignKey(
                    name: "FK_SiteSubscriptionFeatures_SiteSubscriptions_SiteSubscriptionId",
                    column: x => x.SiteSubscriptionId,
                    principalTable: "SiteSubscriptions",
                    principalColumn: "SiteSubscriptionId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SiteSubscriptionPrices",
            columns: table => new
            {
                SiteSubscriptionPriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Frequency = table.Column<int>(type: "int", nullable: false),
                SiteSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SiteSubscriptionPrices", x => x.SiteSubscriptionPriceId);
                table.ForeignKey(
                    name: "FK_SiteSubscriptionPrices_Currencies_CurrencyId",
                    column: x => x.CurrencyId,
                    principalTable: "Currencies",
                    principalColumn: "CurrencyId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SiteSubscriptionPrices_SiteSubscriptions_SiteSubscriptionId",
                    column: x => x.SiteSubscriptionId,
                    principalTable: "SiteSubscriptions",
                    principalColumn: "SiteSubscriptionId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterTopics",
            columns: table => new
            {
                ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterTopics", x => new { x.ChapterId, x.TopicId });
                table.ForeignKey(
                    name: "FK_ChapterTopics_Chapters_ChapterId",
                    column: x => x.ChapterId,
                    principalTable: "Chapters",
                    principalColumn: "ChapterId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChapterTopics_Topics_TopicId",
                    column: x => x.TopicId,
                    principalTable: "Topics",
                    principalColumn: "TopicId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EventTopics",
            columns: table => new
            {
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventTopics", x => new { x.EventId, x.TopicId });
                table.ForeignKey(
                    name: "FK_EventTopics_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "EventId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EventTopics_Topics_TopicId",
                    column: x => x.TopicId,
                    principalTable: "Topics",
                    principalColumn: "TopicId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberTopics",
            columns: table => new
            {
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberTopics", x => new { x.MemberId, x.TopicId });
                table.ForeignKey(
                    name: "FK_MemberTopics_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MemberTopics_Topics_TopicId",
                    column: x => x.TopicId,
                    principalTable: "Topics",
                    principalColumn: "TopicId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberSiteSubscriptionLog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteSubscriptionPriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SiteSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberSiteSubscriptionLog", x => x.Id);
                table.ForeignKey(
                    name: "FK_MemberSiteSubscriptionLog_Payments_PaymentId",
                    column: x => x.PaymentId,
                    principalTable: "Payments",
                    principalColumn: "PaymentId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MemberSiteSubscriptionLog_SiteSubscriptionPrices_SiteSubscriptionPriceId",
                    column: x => x.SiteSubscriptionPriceId,
                    principalTable: "SiteSubscriptionPrices",
                    principalColumn: "SiteSubscriptionPriceId");
                table.ForeignKey(
                    name: "FK_MemberSiteSubscriptionLog_SiteSubscriptions_SiteSubscriptionId",
                    column: x => x.SiteSubscriptionId,
                    principalTable: "SiteSubscriptions",
                    principalColumn: "SiteSubscriptionId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MemberSiteSubscriptions",
            columns: table => new
            {
                MemberSiteSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteSubscriptionPriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberSiteSubscriptions", x => x.MemberSiteSubscriptionId);
                table.ForeignKey(
                    name: "FK_MemberSiteSubscriptions_Members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "Members",
                    principalColumn: "MemberId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MemberSiteSubscriptions_SiteSubscriptionPrices_SiteSubscriptionPriceId",
                    column: x => x.SiteSubscriptionPriceId,
                    principalTable: "SiteSubscriptionPrices",
                    principalColumn: "SiteSubscriptionPriceId");
                table.ForeignKey(
                    name: "FK_MemberSiteSubscriptions_SiteSubscriptions_SiteSubscriptionId",
                    column: x => x.SiteSubscriptionId,
                    principalTable: "SiteSubscriptions",
                    principalColumn: "SiteSubscriptionId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChapterAdminMembers_MemberId",
            table: "ChapterAdminMembers",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterContactMessageReplies_ChapterContactMessageId",
            table: "ChapterContactMessageReplies",
            column: "ChapterContactMessageId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterContactMessageReplies_MemberId",
            table: "ChapterContactMessageReplies",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterConversationMessages_ChapterConversationId",
            table: "ChapterConversationMessages",
            column: "ChapterConversationId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterConversationMessages_MemberId",
            table: "ChapterConversationMessages",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterConversations_ChapterId",
            table: "ChapterConversations",
            column: "ChapterId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterConversations_MemberId",
            table: "ChapterConversations",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterPages_ChapterId",
            table: "ChapterPages",
            column: "ChapterId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterPaymentAccounts_ChapterId",
            table: "ChapterPaymentAccounts",
            column: "ChapterId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ChapterPaymentAccounts_OwnerId",
            table: "ChapterPaymentAccounts",
            column: "OwnerId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterPaymentAccounts_SitePaymentSettingId",
            table: "ChapterPaymentAccounts",
            column: "SitePaymentSettingId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterPaymentSettings_CurrencyId",
            table: "ChapterPaymentSettings",
            column: "CurrencyId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterSubscriptions_CurrencyId",
            table: "ChapterSubscriptions",
            column: "CurrencyId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterSubscriptions_SitePaymentSettingId",
            table: "ChapterSubscriptions",
            column: "SitePaymentSettingId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterTopics_TopicId",
            table: "ChapterTopics",
            column: "TopicId");

        migrationBuilder.CreateIndex(
            name: "IX_Countries_CurrencyId",
            table: "Countries",
            column: "CurrencyId");

        migrationBuilder.CreateIndex(
            name: "IX_ErrorProperties_ErrorId",
            table: "ErrorProperties",
            column: "ErrorId");

        migrationBuilder.CreateIndex(
            name: "IX_EventComments_EventId",
            table: "EventComments",
            column: "EventId");

        migrationBuilder.CreateIndex(
            name: "IX_EventComments_MemberId",
            table: "EventComments",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_EventComments_ParentEventCommentId",
            table: "EventComments",
            column: "ParentEventCommentId");

        migrationBuilder.CreateIndex(
            name: "IX_EventEmails_EventId",
            table: "EventEmails",
            column: "EventId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_EventHosts_EventId",
            table: "EventHosts",
            column: "EventId");

        migrationBuilder.CreateIndex(
            name: "IX_EventHosts_MemberId",
            table: "EventHosts",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_EventTicketPayments_EventId",
            table: "EventTicketPayments",
            column: "EventId");

        migrationBuilder.CreateIndex(
            name: "IX_EventTicketPayments_PaymentId",
            table: "EventTicketPayments",
            column: "PaymentId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_EventTicketSettings_CurrencyId",
            table: "EventTicketSettings",
            column: "CurrencyId");

        migrationBuilder.CreateIndex(
            name: "IX_EventTopics_TopicId",
            table: "EventTopics",
            column: "TopicId");

        migrationBuilder.CreateIndex(
            name: "IX_EventWaitlistMembers_EventId",
            table: "EventWaitlistMembers",
            column: "EventId");

        migrationBuilder.CreateIndex(
            name: "IX_EventWaitlistMembers_MemberId",
            table: "EventWaitlistMembers",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_FeatureSeenByMembers_MemberId",
            table: "FeatureSeenByMembers",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_InstagramImages_InstagramPostId",
            table: "InstagramImages",
            column: "InstagramPostId");

        migrationBuilder.CreateIndex(
            name: "IX_IssueMessages_IssueId",
            table: "IssueMessages",
            column: "IssueId");

        migrationBuilder.CreateIndex(
            name: "IX_IssueMessages_MemberId",
            table: "IssueMessages",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_Issues_MemberId",
            table: "Issues",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberChapterNotificationSettings_MemberChapterId",
            table: "MemberChapterNotificationSettings",
            column: "MemberChapterId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberChapters_ChapterId",
            table: "MemberChapters",
            column: "ChapterId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberChapters_MemberId",
            table: "MemberChapters",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberLocations_CountryId",
            table: "MemberLocations",
            column: "CountryId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberNotificationSettings_MemberId",
            table: "MemberNotificationSettings",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberPasswordResetRequests_MemberId",
            table: "MemberPasswordResetRequests",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberPaymentSettings_CurrencyId",
            table: "MemberPaymentSettings",
            column: "CurrencyId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberProperties_MemberId",
            table: "MemberProperties",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberSiteSubscriptionLog_PaymentId",
            table: "MemberSiteSubscriptionLog",
            column: "PaymentId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberSiteSubscriptionLog_SiteSubscriptionId",
            table: "MemberSiteSubscriptionLog",
            column: "SiteSubscriptionId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberSiteSubscriptionLog_SiteSubscriptionPriceId",
            table: "MemberSiteSubscriptionLog",
            column: "SiteSubscriptionPriceId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberSiteSubscriptions_MemberId",
            table: "MemberSiteSubscriptions",
            column: "MemberId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_MemberSiteSubscriptions_SiteSubscriptionId",
            table: "MemberSiteSubscriptions",
            column: "SiteSubscriptionId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_MemberSiteSubscriptions_SiteSubscriptionPriceId",
            table: "MemberSiteSubscriptions",
            column: "SiteSubscriptionPriceId",
            unique: true,
            filter: "[SiteSubscriptionPriceId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_MemberSubscriptionLog_ChapterId",
            table: "MemberSubscriptionLog",
            column: "ChapterId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberSubscriptionLog_ChapterSubscriptionId",
            table: "MemberSubscriptionLog",
            column: "ChapterSubscriptionId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberSubscriptionLog_MemberId",
            table: "MemberSubscriptionLog",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_MemberSubscriptionLog_PaymentId",
            table: "MemberSubscriptionLog",
            column: "PaymentId",
            unique: true,
            filter: "[PaymentId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_MemberTopics_TopicId",
            table: "MemberTopics",
            column: "TopicId");

        migrationBuilder.CreateIndex(
            name: "IX_NewChapterTopics_ChapterId",
            table: "NewChapterTopics",
            column: "ChapterId");

        migrationBuilder.CreateIndex(
            name: "IX_NewChapterTopics_MemberId",
            table: "NewChapterTopics",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_NewMemberTopics_MemberId",
            table: "NewMemberTopics",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_ChapterId",
            table: "Notifications",
            column: "ChapterId");

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_MemberId",
            table: "Notifications",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_PaymentCheckoutSessions_MemberId",
            table: "PaymentCheckoutSessions",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_CurrencyId",
            table: "Payments",
            column: "CurrencyId");

        migrationBuilder.CreateIndex(
            name: "IX_QueuedEmailRecipients_QueuedEmailId",
            table: "QueuedEmailRecipients",
            column: "QueuedEmailId");

        migrationBuilder.CreateIndex(
            name: "IX_QueuedEmails_ChapterId",
            table: "QueuedEmails",
            column: "ChapterId");

        migrationBuilder.CreateIndex(
            name: "IX_SentEmailEvents_SentEmailId",
            table: "SentEmailEvents",
            column: "SentEmailId");

        migrationBuilder.CreateIndex(
            name: "IX_SiteContactMessageReplies_MemberId",
            table: "SiteContactMessageReplies",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_SiteContactMessageReplies_SiteContactMessageId",
            table: "SiteContactMessageReplies",
            column: "SiteContactMessageId");

        migrationBuilder.CreateIndex(
            name: "IX_SiteSubscriptionFeatures_SiteSubscriptionId",
            table: "SiteSubscriptionFeatures",
            column: "SiteSubscriptionId");

        migrationBuilder.CreateIndex(
            name: "IX_SiteSubscriptionPrices_CurrencyId",
            table: "SiteSubscriptionPrices",
            column: "CurrencyId");

        migrationBuilder.CreateIndex(
            name: "IX_SiteSubscriptionPrices_SiteSubscriptionId",
            table: "SiteSubscriptionPrices",
            column: "SiteSubscriptionId");

        migrationBuilder.CreateIndex(
            name: "IX_SiteSubscriptions_FallbackSiteSubscriptionId",
            table: "SiteSubscriptions",
            column: "FallbackSiteSubscriptionId");

        migrationBuilder.CreateIndex(
            name: "IX_SiteSubscriptions_SitePaymentSettingId",
            table: "SiteSubscriptions",
            column: "SitePaymentSettingId");

        migrationBuilder.CreateIndex(
            name: "IX_Topics_TopicGroupId",
            table: "Topics",
            column: "TopicGroupId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ChapterAdminMembers");

        migrationBuilder.DropTable(
            name: "ChapterContactMessageReplies");

        migrationBuilder.DropTable(
            name: "ChapterConversationMessages");

        migrationBuilder.DropTable(
            name: "ChapterEmails");

        migrationBuilder.DropTable(
            name: "ChapterEventSettings");

        migrationBuilder.DropTable(
            name: "ChapterImages");

        migrationBuilder.DropTable(
            name: "ChapterLinks");

        migrationBuilder.DropTable(
            name: "ChapterLocations");

        migrationBuilder.DropTable(
            name: "ChapterMembershipSettings");

        migrationBuilder.DropTable(
            name: "ChapterPages");

        migrationBuilder.DropTable(
            name: "ChapterPaymentAccounts");

        migrationBuilder.DropTable(
            name: "ChapterPaymentSettings");

        migrationBuilder.DropTable(
            name: "ChapterPrivacySettings");

        migrationBuilder.DropTable(
            name: "ChapterProperties");

        migrationBuilder.DropTable(
            name: "ChapterPropertyOptions");

        migrationBuilder.DropTable(
            name: "ChapterQuestions");

        migrationBuilder.DropTable(
            name: "ChapterTexts");

        migrationBuilder.DropTable(
            name: "ChapterTopics");

        migrationBuilder.DropTable(
            name: "Emails");

        migrationBuilder.DropTable(
            name: "ErrorProperties");

        migrationBuilder.DropTable(
            name: "EventComments");

        migrationBuilder.DropTable(
            name: "EventEmails");

        migrationBuilder.DropTable(
            name: "EventHosts");

        migrationBuilder.DropTable(
            name: "EventInvites");

        migrationBuilder.DropTable(
            name: "EventResponses");

        migrationBuilder.DropTable(
            name: "EventTicketPayments");

        migrationBuilder.DropTable(
            name: "EventTicketSettings");

        migrationBuilder.DropTable(
            name: "EventTopics");

        migrationBuilder.DropTable(
            name: "EventWaitlistMembers");

        migrationBuilder.DropTable(
            name: "FeatureSeenByMembers");

        migrationBuilder.DropTable(
            name: "InstagramFetchLog");

        migrationBuilder.DropTable(
            name: "InstagramImages");

        migrationBuilder.DropTable(
            name: "IssueMessages");

        migrationBuilder.DropTable(
            name: "MemberActivationTokens");

        migrationBuilder.DropTable(
            name: "MemberAvatars");

        migrationBuilder.DropTable(
            name: "MemberChapterNotificationSettings");

        migrationBuilder.DropTable(
            name: "MemberEmailAddressUpdateTokens");

        migrationBuilder.DropTable(
            name: "MemberEmailPreferences");

        migrationBuilder.DropTable(
            name: "MemberLocations");

        migrationBuilder.DropTable(
            name: "MemberNotificationSettings");

        migrationBuilder.DropTable(
            name: "MemberPasswordResetRequests");

        migrationBuilder.DropTable(
            name: "MemberPasswords");

        migrationBuilder.DropTable(
            name: "MemberPaymentSettings");

        migrationBuilder.DropTable(
            name: "MemberPreferences");

        migrationBuilder.DropTable(
            name: "MemberProperties");

        migrationBuilder.DropTable(
            name: "MemberSiteSubscriptionLog");

        migrationBuilder.DropTable(
            name: "MemberSiteSubscriptions");

        migrationBuilder.DropTable(
            name: "MemberSubscriptionLog");

        migrationBuilder.DropTable(
            name: "MemberSubscriptions");

        migrationBuilder.DropTable(
            name: "MemberTopics");

        migrationBuilder.DropTable(
            name: "NewChapterTopics");

        migrationBuilder.DropTable(
            name: "NewMemberTopics");

        migrationBuilder.DropTable(
            name: "Notifications");

        migrationBuilder.DropTable(
            name: "PaymentCheckoutSessions");

        migrationBuilder.DropTable(
            name: "PaymentProviderWebhookEvents");

        migrationBuilder.DropTable(
            name: "QueuedEmailRecipients");

        migrationBuilder.DropTable(
            name: "SentEmailEvents");

        migrationBuilder.DropTable(
            name: "SiteContactMessageReplies");

        migrationBuilder.DropTable(
            name: "SiteEmailSettings");

        migrationBuilder.DropTable(
            name: "SiteSubscriptionFeatures");

        migrationBuilder.DropTable(
            name: "VenueLocations");

        migrationBuilder.DropTable(
            name: "ChapterContactMessages");

        migrationBuilder.DropTable(
            name: "ChapterConversations");

        migrationBuilder.DropTable(
            name: "Errors");

        migrationBuilder.DropTable(
            name: "Events");

        migrationBuilder.DropTable(
            name: "Features");

        migrationBuilder.DropTable(
            name: "InstagramPosts");

        migrationBuilder.DropTable(
            name: "Issues");

        migrationBuilder.DropTable(
            name: "Countries");

        migrationBuilder.DropTable(
            name: "SiteSubscriptionPrices");

        migrationBuilder.DropTable(
            name: "ChapterSubscriptions");

        migrationBuilder.DropTable(
            name: "Payments");

        migrationBuilder.DropTable(
            name: "MemberChapters");

        migrationBuilder.DropTable(
            name: "Topics");

        migrationBuilder.DropTable(
            name: "QueuedEmails");

        migrationBuilder.DropTable(
            name: "SentEmails");

        migrationBuilder.DropTable(
            name: "SiteContactMessages");

        migrationBuilder.DropTable(
            name: "Venues");

        migrationBuilder.DropTable(
            name: "SiteSubscriptions");

        migrationBuilder.DropTable(
            name: "Currencies");

        migrationBuilder.DropTable(
            name: "Members");

        migrationBuilder.DropTable(
            name: "TopicGroups");

        migrationBuilder.DropTable(
            name: "Chapters");

        migrationBuilder.DropTable(
            name: "SitePaymentSettings");
    }
}
