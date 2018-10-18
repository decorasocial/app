using decora.Helpers;
using Firebase.Storage;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ProfileImageEdit : ContentPage
    {
        int userId { get; set; }

        public ProfileImageEdit (int idUser)
        {
            userId = idUser;
            InitializeComponent();
        }


        public void close()
        {
            IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(userId) },
                       { "module", "general" },
                       { "idModule", null },
                    };

            App.Current.MainPage = new NavigationPage(new Profile(args));
        }

        public async void selPicturePerfil()
        {

            await CrossMedia.Current.Initialize();

            var imgData = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions()
            {
                CompressionQuality = 40,
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Custom,
                CustomPhotoSize = 30
            });

            if (imgData != null)
            {

                Stream fileStream = imgData.GetStream();

                string newName = ImageRender.newName(fileStream);

                Settings.config_image = newName;

                string url = await saveImage("user", newName, fileStream);

                imageChargedPerfil.Source = ImageSource.FromStream(imgData.GetStream);

            }

        }

        public async void selPictureCover()
        {

            await CrossMedia.Current.Initialize();

            var imgData = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions()
            {
                CompressionQuality = 50,
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Custom,
                CustomPhotoSize = 40
            });

            if (imgData != null)
            {

                Stream fileStream = imgData.GetStream();

                string newName = ImageRender.newName(fileStream);

                imageCover.Text = newName;

                string url = await saveImage("cover", newName, fileStream);

                imageChargedCover.Source = ImageSource.FromStream(imgData.GetStream);

            }

        }

        public async void picturePerfil()
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
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Custom,
                    CustomPhotoSize = 30
                });

                if (file != null)
                {

                    Stream fileStream = file.GetStream();

                    string newName = ImageRender.newName(fileStream);

                    Settings.config_image = newName;

                    string url = await saveImage("user", newName, fileStream);

                    imageChargedPerfil.Source = ImageSource.FromStream(file.GetStream);

                }


            }
        }

        public async void pictureCover()
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
                    CompressionQuality = 50,
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Custom,
                    CustomPhotoSize = 40
                });

                if (file != null)
                {

                    Stream fileStream = file.GetStream();

                    string newName = ImageRender.newName(fileStream);

                    string url = await saveImage("cover", newName, fileStream);

                    imageChargedCover.Source = ImageSource.FromStream(file.GetStream);

                }


            }
        }



        public async Task<string> saveImage(string path, string name, Stream imgStream)
        {
            var storageImage = new FirebaseStorage("decora-social.appspot.com")
                .Child(path)
                .Child(name)
                .PutAsync(imgStream);

            if (path == "cover")
            {
                Application.Current.Properties["app_config_image"] = name;

                string endpoint = "portalib-dev-profile";

                IDictionary<string, string> parameters = new Dictionary<string, string>()
                {
                   { "idUser", Convert.ToString(userId) },
                   { "cover", name }
                };

                IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "cover" },
                    { "mod", "profile" }
                };

                await decora.Service.Run(endpoint, call, "POST", parameters);
            }
            else
            {
                string endpoint = "portalib-dev-profile";

                IDictionary<string, string> parameters = new Dictionary<string, string>()
                {
                   { "idUser", Convert.ToString(userId) },
                   { "image", name }
                };

                IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "perfil" },
                    { "mod", "profile" }
                };

                await decora.Service.Run(endpoint, call, "POST", parameters);
            }

            return Convert.ToString(storageImage.TargetUrl);
        }

    }
}