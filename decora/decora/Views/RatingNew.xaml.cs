using decora.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RatingNew : ContentPage
    {
        public int userId { get; set; }
        public RatingNew (int idUser)
        {
            userId = idUser;

            InitializeComponent();

        }

        protected override void OnAppearing()
        {

            for (int i = 0; i < 5; i++)
            {
                Image star = new Image
                {
                    Source = "star_green",
                    HeightRequest = 40,
                    WidthRequest = 20,
                    Margin = new Thickness(2, 0)
                };

                star.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(setStar),
                    CommandParameter = i
                });

                rating_stars.Children.Add(star);

            }
        }

        private void setStar(object idx)
        {

            string imgStar;

            int _idx = Convert.ToInt32(idx);

            qtdRating.Text = (_idx + 1).ToString();

            for (int i = 0; i < 5; i++)
            {
                if (i <= _idx)
                    imgStar = "star_green";
                else
                    imgStar = "star_null";

                Image tmp = (Image)rating_stars.Children[i];
                Image tst = new Image { Source = imgStar };

                if (tmp.Source != tst.Source)
                    tmp.Source = imgStar;

            }

        }

        public void close()
        {
            IDictionary<string, string> args = new Dictionary<string, string>()
                                {
                                { "idUser",Convert.ToString(userId) },
                                { "module", "ratinglist" },
                                {"idModule", null }
                                };

            App.Current.MainPage = new NavigationPage(new Profile(args));
        }

        public async void saveRating()
        {

            if (
                (String.IsNullOrEmpty(commentRating.Text)) ||
                (commentRating.Text.Length < 3)
                )
            {
                await DisplayAlert("Preencha os campos", "Você deve preencher todos os campos.", "Ok");
            }
            else
            {

                string endpoint = "portalib-dev-rating";

                IDictionary<string, string> parameters = new Dictionary<string, string>()
                {
                   { "idUser", userId.ToString() },
                   { "idRater", Convert.ToString(Settings.config_user)},
                   { "rating", qtdRating.Text },
                   { "comment", commentRating.Text }
                };

                IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "new" },
                    { "mod", "rating" }
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