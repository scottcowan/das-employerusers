﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerUsers.Data.User
{
    public interface IUserRepository
    {
        Task Create(Domain.User registerUser);
    }
}
