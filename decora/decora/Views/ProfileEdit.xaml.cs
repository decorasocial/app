using decora.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ProfileEdit : ContentPage
    {
        public int idEdit { get; set; }

        public ProfileEdit (int idUser)
        {
            InitializeComponent();

            idEdit = idUser;

            makeForm();

        }

        public void close()
        {
            IDictionary<string, string> args = new Dictionary<string, string>()
                                {
                                { "idUser",Convert.ToString(idEdit) },
                                { "module", "general" },
                                {"idModule", null }
                                };

            App.Current.MainPage = new NavigationPage(new Profile(args));
        }

        public async void makeForm()
        {

            string endpoint = "portalib-dev-profile";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idUserProfile", idEdit },
                { "idUser", Convert.ToInt32(Settings.config_user) }
            };
            
            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "data" },
                { "mod", "profile" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            if (res["phone"] != null)
                dataPhone.Text = Convert.ToString(res["phone"]);

            if (res["website"] != null)
                dataWebsite.Text = Convert.ToString(res["website"]);

            if (res["link_instagram"] != null)
                dataInstagram.Text = Convert.ToString(res["link_instagram"]);

            if (res["link_facebook"] != null)
                dataFacebook.Text = Convert.ToString(res["link_facebook"]);

            if (res["link_linkedin"] != null)
                dataLinkedin.Text = Convert.ToString(res["link_linkedin"]);

            if (res["link_twitter"] != null)
                dataTwitter.Text = Convert.ToString(res["link_twitter"]);

            if (res["link_gplus"] != null)
                dataGplus.Text = Convert.ToString(res["link_gplus"]);

            if (res["biography"] != null)
                dataBio.Text = Convert.ToString(res["biography"]);

        }

        public async void saveProfile()
        {

            string endpoint = "portalib-dev-profile";

            IDictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "idUser", Convert.ToString(idEdit) },
                { "biography", dataBio.Text },
                { "phone", dataPhone.Text },
                { "website", dataWebsite.Text },
                { "link_instagram", dataInstagram.Text },
                { "link_facebook", dataFacebook.Text },
                { "link_linkedin", dataLinkedin.Text },
                { "link_twitter", dataTwitter.Text },
                { "link_gplus", dataGplus.Text }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "data" },
                    { "mod", "profile" }
                };

            dynamic res = await decora.Service.Run(endpoint, call, "POST", parameters);

            IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(idEdit) },
                       { "module", "general" },
                       { "idModule", null },
                    };

            App.Current.MainPage = new NavigationPage(new Profile(args));

        }
    }
}