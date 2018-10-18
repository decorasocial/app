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
	public partial class BlogList : ContentPage
    {
        public int categoryId { get; set; }
        public string categoryName { get; set; }

        public BlogList (int idCategory, string category)
        {
            categoryId = idCategory;
            categoryName = category;

            InitializeComponent();

            makeBlog(false, "", true);
        }

        protected override async void OnAppearing()
        {
            bool validate = await CheckAuth.validate();
            if (!validate)
                App.Current.MainPage = new NavigationPage(new Login());

            base.OnAppearing();

            img_category.Source = ImageRender.display("post", "foto5.jpg");
        }

        public async void makeBlog(bool Load = false, string arg = "", bool init = false)
        {

            contentIndicator.IsVisible = true;

            string endpoint = "portalib-dev-blog";

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
                    { "act", "list" },
                    { "mod", "blog" }
                };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            int tt = 0;

            foreach (var item in res)
                tt++;

            category_tt.Text = Convert.ToString(tt);
            category_title.Text = categoryName;

            if (tt > 0)
            {

                foreach (var item in res)
                {

                    StackLayout postBlock = new StackLayout
                    {
                        HeightRequest = 200,

                    };

                    RelativeLayout rel = new RelativeLayout();

                    postBlock.Children.Add(rel);

                    Image imgPost = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 200,
                        Source = ImageRender.display("blog", (string)item["image"]),
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
                        Opacity = 0.3,
                        BackgroundColor = Color.Black,
                        Margin = new Thickness(5),
                        HeightRequest = 60
                    };

                    rel.Children.Add(boxlight,
                        widthConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Width;
                        }),
                        heightConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Height;
                        }),

                        xConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.X;
                        }),

                        yConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Y;
                        })
                    );

                    StackLayout dataPost = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Margin = new Thickness(5, 0)
                    };

                    dataPost.Children.Add(new Label
                    {
                        Text = (string)item["title"],
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = 18,
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Bold" :
                            Device.RuntimePlatform == Device.Android ? "Jura-Bold.ttf#Jura-Bold" : "Assets/Fonts/Jura-Bold.ttf#Jura-Bold",
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextColor = Color.White
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
                            return parent.Y;
                        }),

                       heightConstraint: Constraint.RelativeToParent((parent) =>
                       {
                           return parent.Height;
                       })
                    );

                    postBlock.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            App.Current.MainPage = new NavigationPage(new Blog(Convert.ToInt32(item["idBlog"])));
                        })
                    });

                    pageContent.Children.Add(postBlock);

                }

            }

            contentIndicator.IsVisible = false;

        }

        public void openMenu()
        {
            Navigation.PushModalAsync(new Menu());
        }

        public void goToBlogAll()
        {

            App.Current.MainPage = new NavigationPage(new BlogAll());

        }

        private void checkScroll(object sender, ScrolledEventArgs e)
        {
            double scrollingSpace = scrollExplorer.ContentSize.Height - scrollExplorer.ScrollY - 60;

            if (scrollingSpace < scrollExplorer.Height)
                makeBlog(true);

        }

        public void goToTimeline()
        {
            App.Current.MainPage = new NavigationPage(new Timeline());
        }
    }
}