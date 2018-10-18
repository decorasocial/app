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
	public partial class ProjectNewModal : ContentPage
    {
        public int userId { get; set; }

        public ProjectNewModal (int IdUser)
        {
            userId = IdUser;
            InitializeComponent();
        }

        public void close()
        {
            base.OnBackButtonPressed();
        }

        public async void selPicture()
        {

            await CrossMedia.Current.Initialize();

            var imgData = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions()
            {
                CompressionQuality = 40,
                PhotoSize = PhotoSize.Custom,
                CustomPhotoSize = 40
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
                    CustomPhotoSize = 40
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

        public async void saveProject()
        {

            if (
                (String.IsNullOrEmpty(projectName.Text)) || (String.IsNullOrEmpty(projectDesc.Text)) ||
                (projectName.Text.Length < 3) || (projectDesc.Text.Length < 3)
                )
            {
                await DisplayAlert("Preencha os campos", "Você deve preencher todos os campos.", "Ok");
            }
            else
            {

                string endpoint = "portalib-dev-project";

                IDictionary<string, string> parameters = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(userId) },
                       { "idCategory", "2" },
                       { "title", projectName.Text },
                       { "description", projectDesc.Text },
                       { "image", imagePost.Text }
                    };

                IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "new" },
                    { "mod", "project" }
                };

                dynamic res = await decora.Service.Run(endpoint, call, "PUT", parameters);

                IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(userId) },
                       { "module", "project" },
                       { "idModule", Convert.ToString(res) },
                    };

                App.Current.MainPage = new NavigationPage(new Profile(args));

                base.OnBackButtonPressed();

            }
        }
    }
}