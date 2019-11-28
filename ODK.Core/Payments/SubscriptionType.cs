using System;

namespace ODK.Core.Payments
{
    public class SubscriptionType
    {
        public SubscriptionType(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; }

        public string Name { get; }
    }
}
