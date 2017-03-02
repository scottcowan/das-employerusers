﻿namespace SFA.DAS.EmployerUsers.Domain
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string PasswordProfileId { get; set; }
        public bool IsActive { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsLocked { get; set; }
        public SecurityCode[] SecurityCodes { get; set; }
        public HistoricalPassword[] PasswordHistory { get; set; }
        public bool RequiresPasswordReset { get; set; }
    }
}
