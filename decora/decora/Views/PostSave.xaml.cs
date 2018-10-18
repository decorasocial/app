using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using decora.Models;
using decora.Helpers;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PostSave : ContentPage
    {

        private int postId { get; set; }
        private int userId { get; set; }
        private int projectId { get; set; }
        private string origin { get; set; }

        public PostSave (int idPost, string pageOrigin, int idUser, int idProject)
        {
            origin = pageOrigin;

            postId = idPost;
            userId = idUser;
            projectId = idProject;

            InitializeComponent();

        }

        public void close()
        {

            if (origin == "post")
            {
                App.Current.MainPage = new NavigationPage(new Post(postId));
            }
            else
            {

                IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(userId) },
                       { "module", "project" },
                       { "idModule", Convert.ToString(projectId) },
                    };

                App.Current.MainPage = new NavigationPage(new Profile(args));
            }
        }

        protected override async void OnAppearing()
        {

            base.OnAppearing();

            string endpoint = "portalib-dev-book";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idPost", postId },
                { "idUser", Convert.ToInt32(Settings.config_user) }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "bookstosave" },
                { "mod", "book" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            int tt = 0;

            foreach (var item in res["books"])
                tt++;

            if (tt > 0)
            {

                List<BookModel> book = new List<BookModel>();

                foreach (var item in res["books"])
                {
                    book.Add(new BookModel() { idBook = (int)item["idBook"], title = (string)item["title"] });
                }

                comboBook.Title = "Livros de idéias";
                comboBook.ItemsSource = book;

                comboBook.SelectedIndexChanged += (object sender, EventArgs e) =>
                {
                    if (comboBook.SelectedIndex != -1)
                    {
                        bt_save_post.IsVisible = true;
                    }
                    else
                    {
                        bt_save_post.IsVisible = false;
                    }
                };

                bt_save_post.Clicked += delegate
                {
                    if (comboBook.SelectedIndex != -1)
                        savePost(Convert.ToInt32(res["books"][comboBook.SelectedIndex]["idBook"]));
                };
            }
            else
            {
                msgEmpty.IsVisible = true;
                comboBook.IsVisible = false;
                txtInstruct.IsVisible = false;
            }
        }

        public async Task<bool> savePost(int idBook)
        {
            string endpoint = "portalib-dev-post";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idPost", postId },
                { "idBook", idBook }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "save" },
                { "mod", "post" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "POST", parameters);

            if (origin == "post")
            {
                App.Current.MainPage = new NavigationPage(new Post(postId));
            }
            else
            {

                IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(userId) },
                       { "module", "project" },
                       { "idModule", Convert.ToString(projectId) },
                    };

                App.Current.MainPage = new NavigationPage(new Profile(args));
            }

            return true;
        }

    }
}