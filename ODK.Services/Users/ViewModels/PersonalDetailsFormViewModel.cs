﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Services.Users.ViewModels;

public class PersonalDetailsFormViewModel
{
    [DisplayName("Email address")]
    public string EmailAddress { get; set; } = "";

    [DisplayName("Receive event emails")]
    public bool EmailOptIn { get; set; } = true;

    [Required]
    [DisplayName("First name")]
    public string FirstName { get; set; } = "";

    [Required]
    [DisplayName("Last name")]
    public string LastName { get; set; } = "";

    [Required]
    [DisplayName("Privacy policy")]
    public bool PrivacyPolicy { get; set; }
}
