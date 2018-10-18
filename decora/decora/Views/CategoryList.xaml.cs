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
	public partial class CategoryList : ContentPage
	{
		public CategoryList ()
        {
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
                    { "", "" }
                };

            }
            else
            {
                parameters = new Dictionary<string, string>()
                {
                    { "ignoreClear", "true" }
                };
            }

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "list" },
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

                    StackLayout CategoryBlock = new StackLayout
                    {
                        HeightRequest = 200,
                        Spacing = 0
                    };

                    RelativeLayout block_rel = new RelativeLayout();

                    CategoryBlock.Children.Add(block_rel);

                    Image projectImage = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 200,
                        Source = ImageRender.display("category", (string)item["image"])
                    };

                    block_rel.Children.Add(projectImage,
                        widthConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Width;
                        }),

                        heightConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Height;
                        })
                    );

                    BoxView projectBox = new BoxView
                    {
                        Opacity = 0.6,
                        BackgroundColor = Color.Black
                    };


                    block_rel.Children.Add(projectBox,
                        widthConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Width;
                        }),

                        heightConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Height - (parent.Height - 50);
                        }),

                        xConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.X;
                        }),

                        yConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Height - 50;
                        })
                    );

                    StackLayout projectData = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        Margin = new Thickness(10, 0, 0, 0)
                    };

                    projectData.Children.Add(new Label
                    {
                        Text = (string)item["title"],
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        HorizontalTextAlignment = TextAlignment.Start,
                        Margin = new Thickness(5, 0),
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Medium" :
                        Device.RuntimePlatform == Device.Android ? "Jura-Medium.ttf#Jura-Medium" : "Assets/Fonts/Jura-Medium.ttf#Jura-Medium",
                        FontSize = 12,
                        TextColor = Color.White

                    });

                    projectData.Children.Add(new Label
                    {
                        Text = string.Format("{0} Foto{1}", (string)item["tt"], (Convert.ToInt32(item["tt"]) == 1 ? "" : "s")),
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        HorizontalTextAlignment = TextAlignment.Start,
                        Margin = new Thickness(5, 0),
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Medium" :
                        Device.RuntimePlatform == Device.Android ? "Jura-Medium.ttf#Jura-Medium" : "Assets/Fonts/Jura-Medium.ttf#Jura-Medium",
                        FontSize = 9,
                        TextColor = Color.White

                    });


                    block_rel.Children.Add(projectData,
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
                            return parent.Height - 45;
                        })
                    );

                    CategoryBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(() =>
                        {
                            App.Current.MainPage = new NavigationPage(new Category(Convert.ToInt32(item["idCategory"])));
                        })
                    });

                    pageContent.Children.Add(CategoryBlock);

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