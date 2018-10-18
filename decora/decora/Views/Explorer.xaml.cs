using decora.Models;
using decora.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using decora.Helpers;
using System.Diagnostics;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Explorer : ContentPage
    {

        public Auth auth;
        public int count { get; set; }
        public int col { get; set; }
        public int row { get; set; }
        public int c { get; set; }

        public Explorer ()
        {

            App.lp.page = "explorer";
            App.lp.page_id = null;
            App.lp.module = null;
            App.lp.module_id = null;

            InitializeComponent();

            count = 1;

            col = 0;
            row = 0;
            c = 0;

            makeExplorer(false, "", true);
        }
        /*
        protected override bool OnBackButtonPressed()
        {
            return true;
        }*/

        public void openMenu()
        {
            Navigation.PushModalAsync(new Menu());
        }

        public void searchAction(object sender, ScrolledEventArgs e)
        {

            gridExplorer.Children.Clear();

            string txt = txtSearch.Text;

            count = 1;

            col = 0;
            row = 0;
            c = 0;

            if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
            {
                makeExplorer();
            }
            else
            {
                makeExplorer(false, txt);
            }
        }

        public async void makeExplorer(bool Load = false, string arg = "", bool init = false)
        {

            contentIndicator.IsVisible = true;

            string endpoint = "portalib-dev-general";

            IDictionary<string, string> parameters;

            if (!Load)
            {
                parameters = new Dictionary<string, string>()
                    {
                       { "idUser", (string)Settings.config_user },
                       { "arg" , arg }
                    };

                gridExplorer.Children.Clear();
                scrollExplorer.Scrolled += checkScroll;

                if (init)
                {
                    gridExplorer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    gridExplorer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    gridExplorer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

            }
            else
            {
                parameters = new Dictionary<string, string>()
                    {
                       { "idUser", (string)Settings.config_user },
                       { "arg" , arg },
                       { "ignoreClear", "true" }
                    };
            }

            IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "explorer" },
                    { "mod", "general" }
                };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            int tt = 0;

            foreach (var item in res)
                tt++;

            if (tt > 0)
            {

                txtEmpty.IsVisible = false;

                foreach (var item in res)
                {

                    if (((c + 1) % 3) == 0)
                        gridExplorer.RowDefinitions.Add(new RowDefinition { Height = 120 });

                    Image explorerImage = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 150,
                        Source = ImageRender.display("post", (string)item["image"])
                    };

                    explorerImage.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(() => {
                            App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(item["idPost"])));
                        })
                    });

                    gridExplorer.Children.Add(explorerImage, col, row);

                    if (((c + 1) % 3) == 0)
                    {
                        col = 0;
                        row++;
                    }
                    else if (((c + 1) % 2) == 0)
                    {
                        col = 1;
                    }
                    else
                    {
                        col = 2;
                    }

                    c++;

                }

                //if (!Load)
                //pageContent.Children.Add(gridExplorer);

            }
            else
            {
                txtEmpty.IsVisible = true;
            }

            contentIndicator.IsVisible = false;

        }

        private void checkScroll(object sender, ScrolledEventArgs e)
        {
            double scrollingSpace = scrollExplorer.ContentSize.Height - scrollExplorer.ScrollY - 60;

            if (scrollingSpace < scrollExplorer.Height)
                makeExplorer(true, txtSearch.Text);

        }

        protected override async void OnAppearing()
        {
            if (CheckConnection.validate())
            {
                bool validate = await CheckAuth.validate();
                if (!validate)
                    App.Current.MainPage = new NavigationPage(new Login());
            }
            else
            {
                App.Current.MainPage = new NavigationPage(new FailConnection());
            }

            base.OnAppearing();
        }

        public void goToTimeline()
        {
            App.Current.MainPage = new NavigationPage(new Timeline());
        }
    }
}