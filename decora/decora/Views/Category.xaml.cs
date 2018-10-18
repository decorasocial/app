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
	public partial class Category : ContentPage
    {
        private int categoryId { get; set; }

        public Category (int idCategory)
        {

            App.lp.page = "category";
            App.lp.page_id = idCategory;
            App.lp.module = null;
            App.lp.module_id = null;

            categoryId = idCategory;

            InitializeComponent();

            makeCategory(false, "", true);
        }

        public void openMenu()
        {
            Navigation.PushModalAsync(new Menu());
        }

        public async void makeCategory(bool Load = false, string arg = "", bool init = false)
        {

            contentIndicator.IsVisible = true;

            string endpoint = "portalib-dev-category";

            IDictionary<string, string> parameters;

            if (!Load)
            {
                parameters = new Dictionary<string, string>()
                    {
                       { "idCategory", Convert.ToString(categoryId) }
                    };

            }
            else
            {
                parameters = new Dictionary<string, string>()
                    {
                       { "idCategory", Convert.ToString(categoryId) },
                       { "ignoreClear", "true" }
                    };
            }

            IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "category" },
                    { "mod", "category" }
                };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            int tt = 0;

            foreach (var item in res)
                tt++;

            if (tt > 0)
            {

                foreach (var item in res)
                {
                    StackLayout postBlock = new StackLayout
                    {
                        HeightRequest = 300,

                    };

                    RelativeLayout rel = new RelativeLayout();

                    postBlock.Children.Add(rel);

                    Image imgPost = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 300,
                        Source = ImageRender.display("post", (string)item["image"]),
                        Margin = new Thickness(5)
                    };

                    rel.Children.Add(imgPost,
                        widthConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Width;
                        }),

                        heightConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Height;
                        })
                    );

                    BoxView boxlight = new BoxView
                    {
                        Opacity = 0.6,
                        BackgroundColor = Color.Black,
                        Margin = new Thickness(5),
                        HeightRequest = 60
                    };

                    rel.Children.Add(boxlight,
                        widthConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Width;
                        }),

                        xConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.X;
                        }),

                        yConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Height - 70;
                        })
                    );

                    StackLayout dataPost = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Margin = new Thickness(5, 0)
                    };

                    dataPost.Children.Add(new Label
                    {
                        Text = (string)item["title"],
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        FontSize = 18,
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Bold" :
                            Device.RuntimePlatform == Device.Android ? "Jura-Bold.ttf#Jura-Bold" : "Assets/Fonts/Jura-Bold.ttf#Jura-Bold",
                        HorizontalTextAlignment = TextAlignment.Start,
                        Margin = new Thickness(10, 11, 0, 0),
                        TextColor = Color.White
                    });

                    dataPost.Children.Add(new Image
                    {
                        Source = "plus",
                        HorizontalOptions = LayoutOptions.End,
                        Margin = new Thickness(0, 15, 20, 0)
                    });

                    rel.Children.Add(dataPost,
                        widthConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Width;
                        }),

                        xConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.X;
                        }),

                        yConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Height - 60;
                        })
                    );

                    postBlock.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(item["idPost"])));
                        })
                    });

                    pageContent.Children.Add(postBlock);

                }

            }

            contentIndicator.IsVisible = false;

        }

        protected override async void OnAppearing()
        {
            bool validate = await CheckAuth.validate();
            if (!validate)
                App.Current.MainPage = new NavigationPage(new Login());

            base.OnAppearing();
        }

        private void checkScroll(object sender, ScrolledEventArgs e)
        {
            double scrollingSpace = scrollExplorer.ContentSize.Height - scrollExplorer.ScrollY - 60;

            if (scrollingSpace < scrollExplorer.Height)
                makeCategory(true);

        }

        public void goToTimeline()
        {
            App.Current.MainPage = new NavigationPage(new Timeline());
        }
    }
}