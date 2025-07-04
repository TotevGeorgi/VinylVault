﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Services
{
    public interface IUserProfileService
    {
        Task<bool> UpdateUserProfileAsync(string email, string fullName, string address);
        Task<bool> DeleteUserAsync(string email);
    }
}
