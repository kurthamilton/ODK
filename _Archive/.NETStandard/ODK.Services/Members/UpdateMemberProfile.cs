using System.Collections.Generic;
using System.Linq;

namespace ODK.Services.Members
{
    public class UpdateMemberProfile
    {
        public bool? EmailOptIn { get; set; }

        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public IEnumerable<UpdateMemberProperty> Properties { get; set; } = Enumerable.Empty<UpdateMemberProperty>();
    }
}
