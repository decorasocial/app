using Plugin.Connectivity;
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
	public partial class FailConnection : ContentPage
    {

        int time_exp { get; set; }
        int max_try { get; set; }

        public FailConnection ()
        {
            time_exp = 1000;
            max_try = 3;
            InitializeComponent();
            // checkConnection();
        }

        private void checkConnection(int try_count = 0)
        {
            if (try_count < max_try)
            {
                if (CheckConnection.validate())
                {

                    time_exp = time_exp * 3;

                    Task.Delay(time_exp);

                    try_count++;
                    checkConnection(try_count);

                }
                else
                {
                    App.Current.MainPage = new NavigationPage(new Login());
                }
            }
        }
    }
}