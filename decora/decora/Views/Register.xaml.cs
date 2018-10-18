using decora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Register : ContentPage
	{
        public List<Categories> categories { get; set; }

        public Register ()
        {
            InitializeComponent();

            List<TypeModel> type = new List<TypeModel>();

            type.Add(new TypeModel() { idType = 0, title = "Sim" });
            type.Add(new TypeModel() { idType = 1, title = "Não" });

            comboType.Title = "- - -";
            comboType.ItemsSource = type;

            comboType.SelectedIndexChanged += (object sender, EventArgs e) =>
            {
                if (comboType.SelectedIndex != -1)
                {
                    if (comboType.SelectedIndex == 0)
                        TypeUser.Text = "professional";

                    if (comboType.SelectedIndex == 1)
                        TypeUser.Text = "common";
                }
                else
                {
                    TypeUser.Text = "";
                }
            };

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            bool validate = await CheckAuth.validate();
            if (validate)
                await Navigation.PushAsync(new Timeline());

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

            makeCategories(res);

        }

        private void makeCategories(dynamic res)
        {

            int c = 0;
            int i = 0;

            foreach (var item in res)
                c++;

            if (c > 0)
            {

                categories = new List<Categories>();

                foreach (var item in res)
                {
                    StackLayout stk = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Margin = new Thickness(0, 7)
                    };

                    Image img = new Image
                    {
                        Source = "unchecked_checkbox",
                        HeightRequest = 20,
                        WidthRequest = 20,
                        HorizontalOptions = LayoutOptions.Start
                    };

                    Label lbl = new Label
                    {
                        Text = string.Format("{0}", (string)item["title"]),
                        HorizontalOptions = LayoutOptions.FillAndExpand
                    };

                    Label hi = new Label
                    {
                        Text = "false"
                    };

                    hi.IsVisible = false;

                    stk.Children.Add(img);
                    stk.Children.Add(lbl);
                    stk.Children.Add(hi);

                    stk.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(() =>
                        {
                            changeImage(img, hi, Convert.ToInt32(item["idCategory"]));
                        })
                    });

                    preferences.Children.Add(stk);

                    preferences.Children.Add(new BoxView
                    {
                        BackgroundColor = Color.LightGray,
                        HeightRequest = 1,
                        HorizontalOptions = LayoutOptions.FillAndExpand
                    });

                    categories.Add(new Categories { id = Convert.ToInt32(item["idCategory"]), sts = 0 });

                    i++;
                }

            }

        }

        public void changeImage(Image img, Label lbl, int id)
        {

            int c = 0;

            foreach (var item in categories)
                c++;

            if (lbl.Text == "true")
            {
                img.Source = "unchecked_checkbox";
                lbl.Text = "false";

                for (int i = 0; i < c; i++)
                    if (categories[i].id == id)
                        categories[i].sts = 0;
            }
            else
            {
                img.Source = "checked_checkbox";
                lbl.Text = "true";

                for (int i = 0; i < c; i++)
                    if (categories[i].id == id)
                        categories[i].sts = 1;
            }
        }

        public async void register()
        {

            string cat = "[";
            bool cat_validate = false;

            foreach (var item in categories)
            {

                if (item.sts > 0)
                {

                    cat = cat + "," + Convert.ToString(item.id);
                    cat_validate = true;

                }

            }

            cat = cat + "]";

            if (
                (String.IsNullOrEmpty(NameFull.Text) || String.IsNullOrEmpty(Mail.Text) || String.IsNullOrEmpty(Mail.Text) || String.IsNullOrEmpty(TypeUser.Text)) ||
                ((NameFull.Text.Length < 3) || (Mail.Text.Length < 6) || (Password.Text.Length < 6))
                )
            {
                await DisplayAlert("Preencha os campos", "Você deve preencher todos os campos.", "Ok");
            }
            else if (!cat_validate)
            {
                await DisplayAlert("Preencha os campos", "Selecione pelo menos uma preferência.", "Ok");
            }
            else
            {

                loader.IsVisible = true;
                formRegister.IsVisible = false;
                string[] name = NameFull.Text.Split(" ".ToCharArray());

                string endpoint = "portalib-dev-login";

                IDictionary<string, string> parameters = new Dictionary<string, string>()
                {
                   { "name", name[0] },
                   { "lastname", NameFull.Text.Replace(string.Format("{0} ",name[0]), "") },
                   { "email", Mail.Text },
                   { "type", TypeUser.Text },
                   { "pass", Password.Text },
                   { "categories", cat.Replace("[,","[")}
                };

                IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "register" },
                    { "mod", "login" }
                };

                dynamic res = await decora.Service.Run(endpoint, call, "PUT", parameters);


                if (!string.IsNullOrEmpty((string)res["user_exist"]))
                {
                    loader.IsVisible = false;
                    formRegister.IsVisible = true;

                    await DisplayAlert("Registro", "Já existe um usuário com este e-mail.", "Ok");

                }
                else
                {

                    bool login = await decora.Views.Login.ExecuteLogin(Mail.Text, Password.Text);

                    if (!login)
                    {
                        loader.IsVisible = false;
                        formRegister.IsVisible = true;
                        await DisplayAlert("Falha ao registrar", "Ocorreu algum problema ao efetuar o login", "Ok");
                    }
                    else
                    {

                        App.Current.MainPage = new NavigationPage(new Timeline());

                    }

                }

            }

        }
    }
}