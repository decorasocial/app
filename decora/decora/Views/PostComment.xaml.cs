using decora.Helpers;
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
	public partial class PostComment : ContentPage
    {
        public int postId { get; set; }

        public PostComment (int idPost)
        {

            postId = idPost;

            InitializeComponent();
        }

        public void close()
        {
            App.Current.MainPage = new NavigationPage(new Post(postId));
        }

        public async void saveComment()
        {

            if (
                (String.IsNullOrEmpty(commentContent.Text)) ||
                (commentContent.Text.Length < 3)
                )
            {
                await DisplayAlert("Preencha os campos", "Você deve preencher todos os campos.", "Ok");
            }
            else
            {

                string endpoint = "portalib-dev-post";

                IDictionary<string, string> parameters = new Dictionary<string, string>()
                    {
                       { "idUser", (string)Settings.config_user },
                       { "idPost", Convert.ToString(postId) },
                       { "comment", commentContent.Text }
                    };

                IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "postcomment" },
                    { "mod", "post" }
                };

                dynamic res = await decora.Service.Run(endpoint, call, "PUT", parameters);

                App.Current.MainPage = new NavigationPage(new Post(postId));

            }
        }
    }
}