﻿using ODK.Core.Payments;

namespace ODK.Services.Payments;

public interface IPaymentProviderFactory
{
    IPaymentProvider GetPaymentProvider(IPaymentSettings settings);
}
