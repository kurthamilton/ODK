﻿namespace ODK.Services.Members
{
    public class CreateMemberProfile : UpdateMemberProfile
    {
        public string EmailAddress { get; set; }

        public UpdateMemberImage Image { get; set; }
    }
}
