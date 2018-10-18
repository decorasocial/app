
using decora.Helpers;
using decora.Models;
using Firebase.Storage;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PostNew : ContentPage
    {
        public int projectId { get; set; }

        public PostNew (int idProject)
        {
            InitializeComponent();

            projectId = idProject;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            string endpoint = "portalib-dev-category";

            IDictionary<string, string> parameters = new Dictionary<string, string>()
                {
                   { "", "" }
                };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "categoryall" },
                { "mod", "category" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            int c = 0;
            int i = 0;

            foreach (var item in res)
                c++;

            if (c > 0)
            {


                List<CategoryModel> category = new List<CategoryModel>();

                Dictionary<int, string> valuess = new Dictionary<int, string>();

                foreach (var item in res)
                {
                    comboCategory.Items.Add((string)item["title"]);
                    category.Add(new CategoryModel { id = Convert.ToInt32((string)item["idCategory"]), title = (string)item["title"] });
                }


                comboCategory.Title = "- - -";


                comboCategory.SelectedIndexChanged += (object sender, EventArgs e) =>
                {
                    if (comboCategory.SelectedIndex != -1)
                    {
                        i = 0;
                        foreach (var itemCat in category)
                        {
                            if (comboCategory.SelectedIndex == i)
                                categoryText.Text = Convert.ToString(itemCat.id);

                            i++;

                        }
                    }
                    else
                    {
                        categoryText.Text = "";
                    }
                };

            }

        }

        public void close()
        {
            IDictionary<string, string> args = new Dictionary<string, string>()
                                {
                                { "idUser",Convert.ToString(Settings.config_user)},
                                { "module", "project" },
                                {"idModule", Convert.ToString(projectId) }
                                };

            App.Current.MainPage = new NavigationPage(new Profile(args));
        }

        public async void selPicture()
        {

            await CrossMedia.Current.Initialize();

            var imgData = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions()
            {
                CompressionQuality = 40,
                PhotoSize = PhotoSize.Custom,
                CustomPhotoSize = 50
            });

            if (imgData != null)
            {

                Stream fileStream = imgData.GetStream();

                string newName = ImageRender.newName(fileStream);

                imagePost.Text = newName;

                string url = saveImage("post", newName, fileStream);

                imageCharged.Source = ImageSource.FromStream(imgData.GetStream);

            }
        }

        public async void picture()
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("Seleção de imagem", "Seu aparelho não suporta esta ação", "Ok");
            }
            else
            {
                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Directory = "my_images",
                    CompressionQuality = 40,
                    PhotoSize = PhotoSize.Custom,
                    CustomPhotoSize = 50
                });

                if (file != null)
                {

                    Stream fileStream = file.GetStream();

                    string newName = ImageRender.newName(fileStream);

                    imagePost.Text = newName;

                    string url = saveImage("post", newName, fileStream);

                    imageCharged.Source = ImageSource.FromStream(file.GetStream);
                }

            }
        }

        public string saveImage(string path, string name, Stream imgStream)
        {
            var storageImage = new FirebaseStorage("decora-social.appspot.com")
                .Child(path)
                .Child(name)
                .PutAsync(imgStream);

            return Convert.ToString(storageImage.TargetUrl);
        }

        public async void savePost()
        {

            if (
                (String.IsNullOrEmpty(descPost.Text)) || String.IsNullOrEmpty(categoryText.Text) ||
                (descPost.Text.Length < 3)
                )
            {
                await DisplayAlert("Preencha os campos", "Você deve preencher todos os campos.", "Ok");
            }
            else
            {

                string endpoint = "portalib-dev-post";

                IDictionary<string, string> parameters = new Dictionary<string, string>()
                {
                   { "idUser", Convert.ToString(Settings.config_user) },
                   { "idProject", Convert.ToString(projectId) },
                   { "photographer", photogrPost.Text },
                   { "description", descPost.Text },
                   { "title", titlePost.Text },
                   { "image", imagePost.Text },
                   { "idCategory", categoryText.Text }
                };

                IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "new" },
                    { "mod", "post" }
                };

                dynamic res = await decora.Service.Run(endpoint, call, "PUT", parameters);

                IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(res["idUser"]) },
                       { "module", "project" },
                       { "idModule", Convert.ToString(projectId) },
                    };

                App.Current.MainPage = new NavigationPage(new Profile(args));

            }
        }
    }
}