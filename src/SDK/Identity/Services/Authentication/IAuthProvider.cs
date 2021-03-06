﻿using Aiursoft.Scanner.Interfaces;
using Aiursoft.XelNaga.Models;
using System.Threading.Tasks;

namespace Aiursoft.Identity.Services.Authentication
{
    public interface IAuthProvider : IScopedDependency
    {
        bool IsEnabled();
        string GetName();
        string GetSettingsPage();
        string GetButtonColor();
        string GetButtonIcon();
        string GetSignInRedirectLink(AiurUrl state);
        string GetBindRedirectLink();
        Task<IUserDetail> GetUserDetail(string code, bool isBinding = false);
    }
}
