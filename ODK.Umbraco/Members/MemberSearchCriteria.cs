using System;
using System.Collections.Generic;
using ODK.Core.Members;
using OdkMember = ODK.Core.Members.IMember;

namespace ODK.Umbraco.Members
{
    public class MemberSearchCriteria
    {
        public MemberSearchCriteria(int chapterId)
        {
            ChapterId = chapterId;
        }

        public int ChapterId { get; }

        public int? MaxItems { get; set; }

        public bool ShowAll { get; set; }

        public Func<IEnumerable<OdkMember>, IEnumerable<OdkMember>> Sort { get; set; }

        public IReadOnlyCollection<MemberTypes> Types { get; set; }
}
}
