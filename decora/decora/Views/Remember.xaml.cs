using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Remember : ContentPage
	{
		public Remember ()
        {
            InitializeComponent();
        }

        public async void sendRemember()
        {

            if (
                String.IsNullOrEmpty(Mail.Text) || (Mail.Text.Length < 6)
                )
            {
                await DisplayAlert("Preencha os campos", "Você deve preencher todos os campos.", "Ok");
            }
            else
            {
                loader.IsVisible = true;
                formRemember.IsVisible = false;

                string endpoint = "portalib-dev-remember";

                IDictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "email", Mail.Text },
                };

                IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "sendRemember" },
                    { "mod", "login" }
                };

                dynamic res = await decora.Service.Run(endpoint, call, "POST", parameters);

                loader.IsVisible = false;
                formRemember.IsVisible = true;

                DisplayAlert("Registro", "Enviamos um link para o seu e-mail, caso esteja vinculado a uma conta.", "Ok");

                Navigation.PushAsync(new Login());

            }

        }

    }
}