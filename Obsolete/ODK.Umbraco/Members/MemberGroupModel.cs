namespace ODK.Umbraco.Members
{
    public class MemberGroupModel
    {
        public MemberGroupModel(int groupId, string name)
        {
            GroupId = groupId;
            Name = name;
        }

        public int GroupId { get; }

        public string Name { get; }
    }
}
