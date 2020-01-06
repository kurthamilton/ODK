using System;

namespace ODK.Core.Chapters
{
    public class ChapterAdminMember
    {
        public ChapterAdminMember(Guid chapterId, Guid memberId)
        {
            ChapterId = chapterId;
            MemberId = memberId;
        }

        public ChapterAdminMember(Guid chapterId, Guid memberId, string firstName, string lastName, string adminEmailAddress, 
            bool superAdmin, bool receiveContactEmails, bool receiveNewMemberEmails, bool sendNewMemberEmails)
            : this(chapterId, memberId)
        {
            AdminEmailAddress = adminEmailAddress;
            FirstName = firstName;
            LastName = lastName;
            ReceiveContactEmails = receiveContactEmails;
            ReceiveNewMemberEmails = receiveNewMemberEmails;
            SendNewMemberEmails = sendNewMemberEmails;
            SuperAdmin = superAdmin;
        }

        public string AdminEmailAddress { get; set; }

        public Guid ChapterId { get; }

        public string FirstName { get; }

        public string LastName { get; }

        public Guid MemberId { get; }

        public bool ReceiveContactEmails { get; set; }

        public bool ReceiveNewMemberEmails { get; set; }

        public bool SendNewMemberEmails { get; set; }

        public bool SuperAdmin { get; }
    }
}
