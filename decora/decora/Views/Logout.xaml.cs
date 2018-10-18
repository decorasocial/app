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
	public partial class Logout : ContentPage
	{
		public Logout ()
        {
            InitializeComponent();

            Settings.deleteAll();

        }

        protected override async void OnAppearing()
        {
            await Navigation.PushAsync(new Login());
        }
    }
}