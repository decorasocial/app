using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using decora.Helpers;
using Newtonsoft.Json;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Login : ContentPage
	{
		public Login ()
        {

            InitializeComponent();
            loader.IsVisible = true;
            formLogin.IsVisible = false;
            btRemember.IsVisible = false;
        }

        protected override async void OnAppearing()
        {
        
            if (CheckConnection.validate())
            {

                bool validate = await CheckAuth.validate();

                if (validate)
                {
                    await Navigation.PushModalAsync(new Timeline());
                }
                else
                {
                    loader.IsVisible = false;
                    formLogin.IsVisible = true;
                    btRemember.IsVisible = true;
                }
            }
            else

            {
                App.Current.MainPage = new NavigationPage(new FailConnection());
            }
            
            base.OnAppearing();
        }

        public void goToRegister()
        {
            Navigation.PushAsync(new Register());
        }

        public void goToRemember()
        {
            Navigation.PushAsync(new Remember());
        }

        public async void GoLogin()
        {

            if (
                (String.IsNullOrEmpty(Email.Text) || String.IsNullOrEmpty(Password.Text)) ||
                ((Email.Text.Length < 6) || (Email.Text.Length < 6))
                )
            {
                await DisplayAlert("Preencha os campos", "Você deve seu e-mail e senha.", "Ok");
            }
            else
            {

                loader.IsVisible = true;
                formLogin.IsVisible = false;
                btRemember.IsVisible = false;
                bool login = await Login.ExecuteLogin(Email.Text, Password.Text);
                
                if (!login)
                {

                    await DisplayAlert("Login inválido", "Verifique seu e-mail e senha.", "Ok");
                    loader.IsVisible = false;
                    formLogin.IsVisible = true;
                    btRemember.IsVisible = true;
                }
                else
                {
                    await Navigation.PushModalAsync(new Timeline());
                }
            }

        }

        public async static Task<bool> ExecuteLogin(string email, string password)
        {

            string endpoint = "portalib-dev-login";

            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(Login.genKey());
            byte[] hash = md5.ComputeHash(inputBytes);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            IDictionary<string, string> parameters = new Dictionary<string, string>()
            {
               { "email", email },
               { "pass", password },
               { "codeAuth", sb.ToString()}
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "validate" },
                { "mod", "login" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "POST", parameters);
            
            bool validate = Convert.ToBoolean(string.Format("{0}", res["validate"]));

            if (validate == true)
            {

                Settings.config_user = Convert.ToString(res["idUser"]);
                Settings.config_code = sb.ToString();
                Settings.config_email = email;
                Settings.config_image = Convert.ToString(res["image"]);
                Settings.config_type = Convert.ToString(res["type"]);
                Settings.config_screen = (Application.Current.MainPage.Height + 70).ToString();

            }
            
            return validate;
            
        }

        private static string genKey()
        {
            string lmin = "abcdefghijklmnopqrstuvwxyz";
            string lmai = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string num = "1234567890";
            string retorno = "";
            string caracteres = "";
            int rand;

            caracteres += lmin;

            caracteres += lmai;
            caracteres += num;

            int len = caracteres.Length;

            Random rnd = new Random();

            for (int n = 1; n <= 8; n++)
            {

                rand = rnd.Next(1, len);
                retorno += caracteres[rand - 1];

            }

            return retorno;
        }

    }
}