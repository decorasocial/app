using Firebase.Storage;
using Plugin.Media;
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
	public partial class BookEdit : ContentPage
    {

        int bookId { get; set; }
        int userId { get; set; }
        string origin { get; set; }

        public BookEdit (int idUser, int idBook, string o)
        {
            bookId = idBook;
            userId = idUser;
            origin = o;

            InitializeComponent();
        }

        protected override async void OnAppearing()
        {

            string endpoint = "portalib-dev-book";

            IDictionary<string, string> parameters = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(userId) },
                       { "idBook", Convert.ToString(bookId) }
                    };

            IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "data" },
                    { "mod", "book" }
                };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            imagePost.Text = (string)res["image"];
            bookTitle.Text = (string)res["title"];
            imageCharged.Source = ImageRender.display("post", (string)res["image"]);

        }

        public void close()
        {
            IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(userId) },
                       { "module", origin },
                       { "idModule", null },
                    };
            App.Current.MainPage = new NavigationPage(new Profile(args));
        }

        public async void selPicture()
        {

            await CrossMedia.Current.Initialize();

            var imgData = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions()
            {
                CompressionQuality = 40,
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Custom,
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
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Custom,
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

        public async void saveBook()
        {

            if (
                (String.IsNullOrEmpty(bookTitle.Text)) ||
                (bookTitle.Text.Length < 3)
                )
            {
                await DisplayAlert("Preencha os campos", "Você deve preencher todos os campos.", "Ok");
            }
            else
            {

                string endpoint = "portalib-dev-book";

                IDictionary<string, string> parameters = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(userId) },
                       { "idBook", Convert.ToString(bookId) },
                       { "title", bookTitle.Text },
                        { "image", imagePost.Text }
                    };

                IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "bookupdate" },
                    { "mod", "book" }
                };

                dynamic res = await decora.Service.Run(endpoint, call, "POST", parameters);


                IDictionary<string, string> args = new Dictionary<string, string>()
                        {
                           { "idUser", Convert.ToString(userId) },
                           { "module", "book" },
                           { "idModule", Convert.ToString(bookId) },
                        };

                App.Current.MainPage = new NavigationPage(new Profile(args));

            }
        }
    }
}