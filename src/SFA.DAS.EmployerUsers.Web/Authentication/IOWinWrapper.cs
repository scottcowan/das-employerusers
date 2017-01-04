using IdentityServer3.Core.Models;
using Microsoft.Owin.Security.Provider;

namespace SFA.DAS.EmployerUsers.Web.Authentication
{
    public interface IOwinWrapper
    {
        SignInMessage GetSignInMessage(string id);
        void IssueLoginCookie(string id, string displayName);
        void RemovePartialLoginCookie();
        void ClearSignInMessageCookie();
        void SetIdsContext(string returnUrl, string clientId, string redirectUrl);
        string GetIdsReturnUrl();
        string GetIdsClientId();
    }
}