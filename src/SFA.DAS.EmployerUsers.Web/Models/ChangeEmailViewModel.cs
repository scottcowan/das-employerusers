﻿namespace SFA.DAS.EmployerUsers.Web.Models
{
    public class ChangeEmailViewModel : ViewModelBase
    {
        public string NewEmailAddress { get; set; }
        public string ConfirmEmailAddress { get; set; }

        public string UserId { get; set; }
        public string ReturnUrl { get; set; }
    }
}