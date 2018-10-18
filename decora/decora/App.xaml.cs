using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using decora.Views;
using Xamarin.Forms;

namespace decora
{
	public partial class App : Application
	{

        public static string config_code = "config_code";
        public static string config_user = "config_user";
        public static string config_image = "config_image";
        public static string config_screen = "config_screen";
        public static string config_email = "config_email";
        public static string config_type = "config_type";

        public struct last_page
        {
            public string page;
            public int? page_id;
            public string module;
            public int? module_id;
        };

        public static last_page lp;

        public App ()
		{
			InitializeComponent();

            MainPage = new NavigationPage(new Login());

        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
        {
            
            // Handle when your app resumes
        }
	}
}
