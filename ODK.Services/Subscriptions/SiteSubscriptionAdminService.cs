using ODK.Core;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Services.Html;
using ODK.Services.Payments;
using ODK.Services.Platforms;
using ODK.Services.Subscriptions.Models;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionAdminService : OdkAdminServiceBase, ISiteSubscriptionAdminService
{
    private readonly IHtmlValidator _htmlValidator;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IPlatformProvider _platformProvider;
    private readonly SiteSubscriptionCooldown _siteSubscriptionCooldown;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionAdminService(
        IUnitOfWork unitOfWork,
        IHtmlValidator htmlValidator,
        IPaymentProviderFactory paymentProviderFactory,
        IPlatformProvider platformProvider,
        SiteSubscriptionCooldown siteSubscriptionCooldown)
        : base(unitOfWork)
    {
        _htmlValidator = htmlValidator;
        _paymentProviderFactory = paymentProviderFactory;
        _platformProvider = platformProvider;
        _siteSubscriptionCooldown = siteSubscriptionCooldown;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<Guid>> AddSiteSubscription(
        IMemberServiceRequest request, SiteSubscriptionCreateModel model)
    {
        var platform = request.Platform;

        var (paymentSettings, existing, existingProduct) = await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetById(model.SitePaymentSettingId),
            x => x.SiteSubscriptionRepository.GetAll(platform),
            x => x.SitePaymentProductRepository.GetByPlatform(platform, model.SitePaymentSettingId));

        if (existing.Any(x =>
            x.Platform == platform &&
            x.SitePaymentSettingId == model.SitePaymentSettingId &&
            string.Equals(x.Name, model.Name, StringComparison.InvariantCultureIgnoreCase)))
        {
            return ServiceResult<Guid>.Failure($"Subscription '{model.Name}' already exists");
        }

        if (model.FallbackSiteSubscriptionId != null &&
            existing.All(x => x.Id != model.FallbackSiteSubscriptionId))
        {
            return ServiceResult<Guid>.Failure($"Fallback subscription not found");
        }

        var htmlResult = _htmlValidator.Validate(model.Description, DefaultHtmlValidatorOptions);
        if (!htmlResult.Success)
        {
            return ServiceResult<Guid>.Failure(htmlResult.Message ?? string.Empty);
        }

        /* Every one of a platform's prices sits under one product, so a new subscription joins the
           product its payment settings account already has, and only the first creates one. */
        var sitePaymentProduct = existingProduct;
        if (sitePaymentProduct == null)
        {
            var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(paymentSettings);
            var externalProductId = await paymentProvider.CreateProduct(
                $"{_platformProvider.GetName(platform)} Platform");
            if (string.IsNullOrEmpty(externalProductId))
            {
                return ServiceResult<Guid>.Failure("Error creating payment provider product");
            }

            sitePaymentProduct = new SitePaymentProduct
            {
                ExternalId = externalProductId,
                Id = _unitOfWork.NewId(),
                Platform = platform,
                SitePaymentSettingId = paymentSettings.Id
            };

            _unitOfWork.SitePaymentProductRepository.Add(sitePaymentProduct);
        }

        var subscription = new SiteSubscription
        {
            ExternalProductId = sitePaymentProduct.ExternalId,
            Id = _unitOfWork.NewId(),
            Platform = platform,
            SitePaymentProductId = sitePaymentProduct.Id,
            SitePaymentSettingId = paymentSettings.Id
        };

        UpdateSiteSubscription(model, subscription, []);

        _unitOfWork.SiteSubscriptionRepository.Add(subscription);

        await _unitOfWork.SaveChanges();

        return ServiceResult<Guid>.Successful(subscription.Id);
    }

    public async Task<ServiceResult> AddSiteSubscriptionPrice(
        IMemberServiceRequest request,
        Guid siteSubscriptionId,
        SiteSubscriptionPriceCreateModel model)
    {
        // Ahead of the queries because one of them looks the currency up by id and requires a match.
        if (model.CurrencyId == default)
        {
            return ServiceResult.Failure("Currency is required");
        }

        var (sitePaymentSettings, siteSubscription, existing, currency) = await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.SiteSubscriptionRepository.GetById(siteSubscriptionId),
            x => x.SiteSubscriptionPriceRepository.GetBySiteSubscriptionId(siteSubscriptionId),
            x => x.CurrencyRepository.GetById(model.CurrencyId));

        if (siteSubscription.Free)
        {
            return ServiceResult.Failure("A free subscription cannot have prices");
        }

        if (existing.Any(x => x.CurrencyId == model.CurrencyId && x.Frequency == model.Frequency))
        {
            return ServiceResult.Failure($"Subscription already has a price for currency '{currency.Code}'");
        }

        if (model.Frequency == SiteSubscriptionFrequency.None || !Enum.IsDefined(model.Frequency))
        {
            return ServiceResult.Failure("Invalid frequency");
        }

        var price = new SiteSubscriptionPrice
        {
            Amount = model.Amount,
            CurrencyId = model.CurrencyId,
            Frequency = model.Frequency,
            SiteSubscriptionId = siteSubscriptionId
        };

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
            sitePaymentSettings,
            siteSubscription.SitePaymentSettingId);

        var sitePaymentProduct = siteSubscription.SitePaymentProductId != null
            ? await GetSiteAdminRestrictedContent(request,
                x => x.SitePaymentProductRepository.GetById(siteSubscription.SitePaymentProductId.Value))
            : null;

        var externalProductId = sitePaymentProduct?.ExternalId ?? siteSubscription.ExternalProductId;

        if (!string.IsNullOrEmpty(externalProductId) && model.Amount > 0)
        {
            price.ExternalId = await paymentProvider.CreateSubscriptionPlan(
                new ExternalSubscriptionPlan
                {
                    Amount = model.Amount,
                    CurrencyCode = currency.Code,
                    ExternalId = string.Empty,
                    ExternalProductId = externalProductId,
                    Frequency = model.Frequency,
                    Name = $"{siteSubscription.Name} - {model.Frequency} [{currency.Code}]",
                    NumberOfMonths = model.Frequency == SiteSubscriptionFrequency.Yearly ? 12 : 1,
                    Recurring = true
                });
        }

        _unitOfWork.SiteSubscriptionPriceRepository.Add(price);
        await _unitOfWork.SaveChanges();

        if (!string.IsNullOrEmpty(price.ExternalId))
        {
            await paymentProvider.ActivateSubscriptionPlan(price.ExternalId);
        }

        return ServiceResult.Successful();
    }

    public async Task DeleteSiteSubscriptionPrice(
        IMemberServiceRequest request, Guid siteSubscriptionId, Guid siteSubscriptionPriceId)
    {
        var (sitePaymentSettings, siteSubscription, price) = await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.SiteSubscriptionRepository.GetById(siteSubscriptionId),
            x => x.SiteSubscriptionPriceRepository.GetById(siteSubscriptionPriceId));

        OdkAssertions.MeetsCondition(price, x => x.SiteSubscriptionId == siteSubscriptionId);

        _unitOfWork.SiteSubscriptionPriceRepository.Delete(price);
        await _unitOfWork.SaveChanges();

        if (!string.IsNullOrEmpty(price.ExternalId))
        {
            var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
                sitePaymentSettings,
                siteSubscription.SitePaymentSettingId);
            await paymentProvider.DeactivateSubscriptionPlan(price.ExternalId);
        }
    }

    public async Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(
        IMemberServiceRequest request)
    {
        var platform = request.Platform;

        return await GetSiteAdminRestrictedContent(request,
            x => x.SiteSubscriptionRepository.GetAll(platform));
    }

    public async Task<SiteAdminMembersViewModel> GetSiteAdminMembersViewModel(
        IMemberServiceRequest request)
    {
        var platform = request.Platform;

        var (subscriptionDtos, chapters, currencies) = await GetSiteAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRecordRepository.GetAllChapterOwnerSubscriptionDtos(platform),
            x => x.ChapterRepository.GetAll(platform, includeUnpublished: true),
            x => x.CurrencyRepository.GetAll());

        // Only owners whose subscription has an expiry date - placeholder subscriptions (null expiry) are
        // ignored. A member repeats once per owned chapter, so collapse to one subscription per member.
        var subscriptionsByMember = subscriptionDtos
            .Where(x => x.MemberSiteSubscription.ExpiresUtc != null)
            .GroupBy(x => x.MemberSiteSubscription.MemberId)
            .ToDictionary(x => x.Key, x => x.First());

        if (subscriptionsByMember.Count == 0)
        {
            return new SiteAdminMembersViewModel
            {
                Platform = platform,
                Rows = []
            };
        }

        var members = await GetSiteAdminRestrictedContent(request,
            x => x.MemberRepository.GetByIds(subscriptionsByMember.Keys.ToArray()));

        var membersById = members.ToDictionary(x => x.Id);
        var currenciesById = currencies.ToDictionary(x => x.Id);
        var chapterNamesByOwner = chapters
            .GroupBy(x => x.OwnerId)
            .ToDictionary(x => x.Key, x => x.OrderBy(c => c.Name).Select(c => c.Name).ToArray());

        var now = DateTime.UtcNow;

        var rows = subscriptionsByMember
            .Where(x => membersById.ContainsKey(x.Key))
            .Select(x =>
            {
                var (memberId, dto) = (x.Key, x.Value);
                var price = dto.SiteSubscriptionPrice;
                var currency = price != null && currenciesById.TryGetValue(price.CurrencyId, out var found)
                    ? found
                    : null;
                var expiresUtc = dto.MemberSiteSubscription.ExpiresUtc;

                return new SiteAdminMemberRowViewModel
                {
                    Amount = price?.Amount,
                    ChapterNames = chapterNamesByOwner.TryGetValue(memberId, out var names) ? names : [],
                    Currency = currency,
                    ExpiresUtc = expiresUtc,
                    Frequency = price?.Frequency ?? SiteSubscriptionFrequency.None,
                    FullName = membersById[memberId].FullName,
                    IsActive = _siteSubscriptionCooldown.IsActive(expiresUtc, now),
                    SubscriptionName = dto.SiteSubscription.Name
                };
            })
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.ExpiresUtc)
            .ThenBy(x => x.FullName)
            .ToArray();

        return new SiteAdminMembersViewModel
        {
            Platform = platform,
            Rows = rows
        };
    }

    public async Task<IReadOnlyCollection<SiteSubscriptionSiteAdminListItemViewModel>> GetSiteSubscriptionSiteAdminListItems(
        IMemberServiceRequest request)
    {
        var platform = request.Platform;

        var (sitePaymentSettings, siteSubscriptionSummaries, prices) = await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.SiteSubscriptionRepository.GetSummaries(platform, _siteSubscriptionCooldown),
            x => x.SiteSubscriptionPriceRepository.GetAll(platform));

        var priceDictionary = prices
            .GroupBy(x => x.SiteSubscriptionId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var siteSubscriptionDictionary = siteSubscriptionSummaries
            .ToDictionary(x => x.SiteSubscription.Id);

        var sitePaymentSettingsDictionary = sitePaymentSettings
            .ToDictionary(x => x.Id);

        return siteSubscriptionSummaries
            .Select(x => new SiteSubscriptionSiteAdminListItemViewModel
            {
                ActiveCount = x.ActiveMemberSiteSubscriptionCount,
                Default = x.SiteSubscription.Default,
                Enabled = x.SiteSubscription.Enabled,
                Features = x.Features.Select(x => x.Feature).ToArray(),
                Free = x.SiteSubscription.Free,
                GroupLimit = x.SiteSubscription.GroupLimit,
                Id = x.SiteSubscription.Id,
                MemberLimit = x.SiteSubscription.MemberLimit,
                Name = x.SiteSubscription.Name,
                PaymentSettingsName = sitePaymentSettingsDictionary[x.SiteSubscription.SitePaymentSettingId].Name,
                Prices = priceDictionary.TryGetValue(x.SiteSubscription.Id, out var prices)
                    ? prices
                        .Select(x => new SiteSubscriptionSiteAdminListItemPriceViewModel
                        {
                            Amount = x.Amount,
                            Currency = x.Currency,
                            Frequency = x.Frequency
                        })
                        .OrderBy(x => x.Currency.Code)
                        .ThenBy(x => x.Amount)
                        .ToArray()
                    : []
            })
            .OrderBy(x => x.PaymentSettingsName)
            .ThenBy(x => x.Name)
            .ToArray();
    }

    public async Task<SiteSubscriptionViewModel> GetSubscriptionViewModel(
        IMemberServiceRequest request, Guid siteSubscriptionId)
    {
        var (siteSubscriptionDto, prices, currencies, sitePaymentSettings) = await GetSiteAdminRestrictedContent(request,
            x => x.SiteSubscriptionRepository
                .Query()
                .ById(siteSubscriptionId)
                .WithFeatures()
                .GetSingle(),
            x => x.SiteSubscriptionPriceRepository.GetBySiteSubscriptionId(siteSubscriptionId),
            x => x.CurrencyRepository.GetAllDtos(),
            x => x.SitePaymentSettingsRepository.GetAll());

        return new SiteSubscriptionViewModel
        {
            Currencies = currencies,
            CurrentMemberExternalSubscription = null,
            CurrentMemberSiteSubscription = null,
            Features = siteSubscriptionDto.Features,
            Prices = prices,
            SitePaymentSettings = sitePaymentSettings,
            Subscription = siteSubscriptionDto.SiteSubscription
        };
    }

    public async Task<ServiceResult> MakeDefault(IMemberServiceRequest request, Guid siteSubscriptionId)
    {
        var platform = request.Platform;

        var (subscriptions, prices) = await GetSiteAdminRestrictedContent(request,
            x => x.SiteSubscriptionRepository.GetAll(platform),
            x => x.SiteSubscriptionPriceRepository.GetAll(platform));

        var subscription = subscriptions.FirstOrDefault(x => x.Id == siteSubscriptionId);
        OdkAssertions.Exists(subscription);

        /* Every new account is put on the default, so a default nobody can be on would fail every sign-up.
           Payment settings are not consulted: the check is about the plan being usable at all, and a paid
           plan whose provider is switched off is a temporary state rather than a broken default. */
        if (!subscription.Free && !prices.Any(x => x.SiteSubscriptionId == subscription.Id))
        {
            return ServiceResult.Failure("A subscription with no prices must be free to be the default");
        }

        var existingDefaults = subscriptions
            .Where(x => x.Default && x.SitePaymentSettingId == subscription.SitePaymentSettingId)
            .ToArray();

        foreach (var existingDefault in existingDefaults)
        {
            existingDefault.Default = false;
            _unitOfWork.SiteSubscriptionRepository.Update(existingDefault);
        }

        subscription.Default = true;
        _unitOfWork.SiteSubscriptionRepository.Update(subscription);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateSiteSubscription(
        IMemberServiceRequest request, Guid siteSubscriptionId, SiteSubscriptionCreateModel model)
    {
        var platform = request.Platform;

        var (dtos, prices) = await GetSiteAdminRestrictedContent(request,
            x => x.SiteSubscriptionRepository
                .Query()
                .ForPlatform(platform)
                .WithFeatures()
                .GetAll(),
            x => x.SiteSubscriptionPriceRepository.GetBySiteSubscriptionId(siteSubscriptionId));

        var dto = dtos
            .FirstOrDefault(x => x.SiteSubscription.Id == siteSubscriptionId);
        OdkAssertions.Exists(dto);

        var (subscription, features) = (dto.SiteSubscription, dto.Features);

        /* Only a paid price conflicts with being free. A zero-amount price does not: that is how a free plan
           was expressed before this flag, and such a plan has to be flaggable without deleting it first. */
        if (model.Free && prices.Any(x => x.Amount > 0))
        {
            return ServiceResult.Failure("A subscription with a paid price cannot be free");
        }

        if (model.FallbackSiteSubscriptionId != subscription.FallbackSiteSubscriptionId &&
            model.FallbackSiteSubscriptionId != null)
        {
            var fallback = dtos
                .Select(x => x.SiteSubscription)
                .FirstOrDefault(x => x.Id == model.FallbackSiteSubscriptionId);
            if (fallback == null)
            {
                return ServiceResult.Failure("Fallback subscription not found");
            }

            if (fallback.Id == subscription.Id)
            {
                return ServiceResult.Failure("Subscription cannot fallback to itself");
            }
        }

        var htmlResult = _htmlValidator.Validate(model.Description, DefaultHtmlValidatorOptions);
        if (!htmlResult.Success)
        {
            return htmlResult;
        }
        UpdateSiteSubscription(model, subscription, features);

        _unitOfWork.SiteSubscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateSiteSubscriptionEnabled(
        IMemberServiceRequest request, Guid siteSubscriptionId, bool enabled)
    {
        var subscription = await GetSiteAdminRestrictedContent(request,
            x => x.SiteSubscriptionRepository.GetById(siteSubscriptionId));

        subscription.Enabled = enabled;

        _unitOfWork.SiteSubscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    private void UpdateSiteSubscription(
        SiteSubscriptionCreateModel model,
        SiteSubscription subscription,
        IReadOnlyCollection<SiteSubscriptionFeature> existingFeatures)
    {
        subscription.Description = model.Description;
        subscription.Enabled = model.Enabled;
        subscription.FallbackSiteSubscriptionId = model.FallbackSiteSubscriptionId;
        subscription.Free = model.Free;
        subscription.GroupLimit = model.GroupLimit;
        subscription.MemberLimit = model.MemberLimit;
        subscription.Name = model.Name;

        var modelFeatures = model.Features.ToHashSet();

        // add new features
        foreach (var feature in modelFeatures)
        {
            if (!existingFeatures.Any(x => x.Feature == feature))
            {
                _unitOfWork.SiteSubscriptionFeatureRepository.Add(new SiteSubscriptionFeature
                {
                    Feature = feature,
                    Id = _unitOfWork.NewId(),
                    SiteSubscriptionId = subscription.Id
                });
            }
        }

        // remove old features
        foreach (var siteSubscriptionFeature in existingFeatures)
        {
            if (!modelFeatures.Contains(siteSubscriptionFeature.Feature))
            {
                _unitOfWork.SiteSubscriptionFeatureRepository.Delete(siteSubscriptionFeature);
            }
        }
    }
}