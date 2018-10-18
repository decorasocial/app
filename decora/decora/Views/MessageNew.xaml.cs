using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using decora.Helpers;
namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MessageNew : ContentPage
    {
        public int userId { get; set; }

        public MessageNew (int idUser)
        {

            userId = idUser;

            InitializeComponent();
        }

        public void close()
        {
            IDictionary<string, string> args = new Dictionary<string, string>()
                                {
                                { "idUser",Convert.ToString(userId) },
                                { "module", "general" },
                                {"idModule", null }
                                };

            App.Current.MainPage = new NavigationPage(new Profile(args));
        }

        public async void saveMessage()
        {

            if (
                (String.IsNullOrEmpty(messageContent.Text)) ||
                (messageContent.Text.Length < 3)
                )
            {
                await DisplayAlert("Preencha os campos", "Você deve preencher todos os campos.", "Ok");
            }
            else
            {

                string endpoint = "portalib-dev-message";

                IDictionary<string, string> parameters = new Dictionary<string, string>()
                    {
                       { "idUserSender", (string)Settings.config_user },
                       { "idUserRecipient", Convert.ToString(userId) },
                       { "message", messageContent.Text }
                    };

                IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "new" },
                    { "mod", "message" }
                };

                dynamic res = await decora.Service.Run(endpoint, call, "PUT", parameters);

                IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", userId.ToString() },
                       { "module", "general" },
                       { "idModule", null }
                    };

                App.Current.MainPage = new NavigationPage(new Profile(args));

            }
        }
    }
}