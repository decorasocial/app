using decora.Models;
using decora.ViewModels;
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
	public partial class Menu : ContentPage
	{
		public Menu ()
        {
            InitializeComponent();

            //auth = new AuthViewModels().auth;
        }

        protected override void OnAppearing()
        {
            goTimeline.Clicked += delegate
            {
                App.Current.MainPage = new NavigationPage(new Timeline());
            };


            IDictionary<string, string> args = new Dictionary<string, string>()
                {
                    { "idUser", Convert.ToString(Settings.config_user) },
                    { "module", "general" },
                    { "idModule", null }
                };

            goProfile.Clicked += delegate
            {
                App.Current.MainPage = new NavigationPage(new Profile(args));
            };
            goBlog.Clicked += delegate
            {
                App.Current.MainPage = new NavigationPage(new BlogAll());
            };
            goCategoryList.Clicked += delegate
            {
                App.Current.MainPage = new NavigationPage(new CategoryList());
            };
            goExplorer.Clicked += delegate
            {
                App.Current.MainPage = new NavigationPage(new Explorer());
            };
        }

        public void close()
        {
            base.OnBackButtonPressed();
        }

        public void goLogout()
        {
            App.Current.MainPage = new NavigationPage(new Logout());

        }
    }
}