﻿namespace SFA.DAS.EmployerUsers.Domain
{
    public class RelyingParty
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool RequireConsent { get; set; }
        public string ApplicationUrl { get; set; }
        public string LogoutUrl { get; set; }
    }
}
