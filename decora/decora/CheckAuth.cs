
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using decora.Helpers;
using System.Globalization;

namespace decora
{
    public class CheckAuth
    {

        public static string code { get; set; }

        private static readonly string SettingsDefault = string.Empty;

        public static async Task<bool> validate()
        {

            code = Settings.config_code;

            if (Settings.IsCodeSet)
            {
                
                string endpoint = "portalib-dev-login";

                IDictionary<string, string> parameters = new Dictionary<string, string>()
                    {
                       { "email",Settings.config_email },
                       { "idUser", Settings.config_user },
                       { "code",Settings.config_code}
                    };

                IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "session" },
                    { "mod", "login" }
                };

                dynamic res = await decora.Service.Run(endpoint, call, "POST", parameters);

                try
                {
                    bool validate = Convert.ToBoolean(string.Format("{0}", res["validate"]));

                    if (!validate)
                        return false;

                    return true;
                }
                catch
                {
                    return await validate();
                }

            }

            return false;
        }
    }
}
