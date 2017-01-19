using System;
using System.Text;
using System.Web.Security;
using Newtonsoft.Json;
using NLog;

namespace SFA.DAS.EmployerUsers.Web.Authentication
{
    public class IdsContext
    {
        public string ReturnUrl { get; set; }
        public string ClientId { get; set; }
        public static string CookieName => "IDS";


        public static IdsContext ReadFrom(string data)
        {
            try
            {
                var unEncData = Encoding.UTF8.GetString(MachineKey.Unprotect(Convert.FromBase64String(data)));
                return JsonConvert.DeserializeObject<IdsContext>(unEncData);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, ex.Message); ;
                return new IdsContext();
            }

        }
    }
}