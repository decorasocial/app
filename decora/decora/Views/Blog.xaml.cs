using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageCircle.Forms.Plugin.Abstractions;

using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Blog : ContentPage
    {
        private int blogId { get; set; }

        public Blog (int idBlog)
        {
            blogId = idBlog;

            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            bool validate = await CheckAuth.validate();
            if (!validate)
                App.Current.MainPage = new NavigationPage(new Login());

            base.OnAppearing();

            makeBlog();
        }

        public void openMenu()
        {
            Navigation.PushModalAsync(new Menu());
        }

        public async void makeBlog()
        {
            string endpoint = "portalib-dev-blog";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idBlog", blogId }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "data" },
                { "mod", "blog" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            var blog = res["blog"];

            blog_img.Source = ImageRender.display("blog", (string)blog["image"]);
            blog_title.Text = (string)blog["title"];
            blog_desc.Text = (string)blog["description"];
            blog_users.Text = (string)blog["users"];

            var comments = res["comments"];

            int c = 0;

            foreach (var item in comments)
                c++;

            if (c > 0)
            {

                block_comment.IsVisible = true;
                block_comment_div.IsVisible = true;
                title_comments.Text = string.Format("{0} Comentário{1}", c, (c > 1 ? "s" : ""));

                foreach (var item in comments)
                {
                    StackLayout stk = new StackLayout
                    {
                        VerticalOptions = LayoutOptions.End,
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 0
                    };

                    CircleImage img = new CircleImage
                    {
                        Source = ImageRender.display("user", (string)item["imgUser"]),
                        HeightRequest = 50,
                        WidthRequest = 50,
                        VerticalOptions = LayoutOptions.Start,
                        BorderColor = Color.DarkGray,
                        BorderThickness = 1,
                        HorizontalOptions = LayoutOptions.Fill,
                        Aspect = Aspect.AspectFill
                    };

                    stk.Children.Add(img);

                    StackLayout stkData = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        Padding = new Thickness(5, 0, 0, 0)
                    };

                    stkData.Children.Add(new Label
                    {
                        Text = (string)item["userName"],
                        FontAttributes = FontAttributes.Bold,
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold"
                    });

                    stkData.Children.Add(new Label
                    {
                        Text = (string)item["comment"],
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-LightItalic" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-LightItalic.ttf#OpenSans-LightItalic" : "Assets/Fonts/OpenSans-LightItalic.ttf#OpenSans-LightItalic"
                    });

                    stk.Children.Add(stkData);

                    block_comment.Children.Add(stk);
                }

            }

            var related = res["blog_related"];

            c = 0;

            foreach (var item in related)
                c++;

            if (c > 0)
            {

                block_related.IsVisible = true;
                block_related_div.IsVisible = true;

                foreach (var item in related)
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

                    block_related.Children.Add(postBlock);
                }

            }

        }

        public void goToTimeline()
        {
            App.Current.MainPage = new NavigationPage(new Timeline());
        }
    }
}