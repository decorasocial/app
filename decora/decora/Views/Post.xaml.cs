using decora.ViewModels;
using ImageCircle.Forms.Plugin.Abstractions;
using Plugin.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using decora.Helpers;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Post : ContentPage
    {
        int postId { get; set; }

        public Post (int post)
        {
            postId = post;

            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            bool validate = await CheckAuth.validate();
            if (!validate)
            {
                App.Current.MainPage = new NavigationPage(new Login());
            }
            else
            {
                MainRelative.HeightRequest = Application.Current.MainPage.Height;

                makePost(postId);
            }
            base.OnAppearing();
        }

        public async void makePost(int idPost)
        {

            bt_veja.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(async () =>
                {
                    await scrollView.ScrollToAsync(0, Application.Current.MainPage.Height, true);
                })
            });

            string endpoint = "portalib-dev-post";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idPost", idPost },
                { "idUser", Convert.ToInt32(Settings.config_user) }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "data" },
                { "mod", "post" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            await Task.Delay(500);

            var post = res["post"];

            postImage.Source = ImageRender.display("post", (string)post["image"]);

            string idUser = Convert.ToString(Convert.ToInt32(post["idUser"]));

            bt_new_message.Clicked += delegate {
                Navigation.PushModalAsync(new PostComment(idPost));
            };

            actionBack.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => {

                    switch (App.lp.page)
                    {
                        case "timeline":
                            App.Current.MainPage = new NavigationPage(new Timeline());
                            break;
                        case "explorer":
                            App.Current.MainPage = new NavigationPage(new Explorer());
                            break;
                        case "category":
                            App.Current.MainPage = new NavigationPage(new Category(Convert.ToInt32(App.lp.page_id)));
                            break;
                        case "profile":

                            IDictionary<string, string> args = new Dictionary<string, string>()
                                {
                                   { "idUser", Convert.ToString(App.lp.page_id) },
                                   { "module", Convert.ToString(App.lp.module)},
                                   { "idModule", Convert.ToString(App.lp.module_id)},
                                };

                            App.Current.MainPage = new NavigationPage(new Profile(args));
                            break;
                    }
                })
            });

            bt_share.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(async () => {

                    await CrossShare.Current.Share(new Plugin.Share.Abstractions.ShareMessage()
                    {

                        Text = string.Format("Baixe o app Portal Decoração, e veja o projeto {0}, de {1}.", (string)post["title"], (string)post["userName"]),
                        Title = "Portal Decoração"
                    });
                })
            });

            tt_comments.Text = (string)post["tt_comments"];
            tt_likes.Text = (string)post["tt_likes"];
            postTile.Text = (string)post["title"];

            count_post.Text = string.Format("FOTO ({0} DE {1})", (string)post["current"], (string)post["total"]);

            if (!string.IsNullOrEmpty((string)post["next"]))
            {
                bt_next.IsVisible = true;
                bt_next.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(post["next"])));
                    })
                });
            }

            if (!string.IsNullOrEmpty((string)post["previous"]))
            {
                bt_previous.IsVisible = true;
                bt_previous.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(post["previous"])));
                    })
                });
            }


            postCategories.Children.Clear();

            int ttCat = 0;
            int cCat = 0;

            foreach (var item in res["categories"])
                ttCat++;

            foreach (var item in res["categories"])
            {
                cCat++;

                Label catLabel = new Label
                {
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    TextColor = Color.FromHex("#667653"),
                    Text = (string)item["title"]
                };

                catLabel.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        App.Current.MainPage = new NavigationPage(new Category(Convert.ToInt32(item["idCategory"])));
                    })
                });

                postCategories.Children.Add(catLabel);

                if (ttCat > cCat)
                {
                    postCategories.Children.Add(new Label
                    {
                        Text = ">",
                        TextColor = Color.FromHex("#667653")
                    });
                }
            };

            postUserImage.Source = ImageRender.display("user", (string)post["imgUser"]);
            postUserImage.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() => {
                    IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", (string)post["idUser"] },
                       { "module", "general"},
                       { "idModule", null},
                    };

                    App.Current.MainPage = new NavigationPage(new Profile(args));
                })
            });

            postUserName.Text = (string)post["userName"];
            postUserName.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() => {
                    IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", (string)post["idUser"] },
                       { "module", "general"},
                       { "idModule", null},
                    };

                    App.Current.MainPage = new NavigationPage(new Profile(args));
                })
            });

            postDescription.Text = (string)post["description"];
            postPhotographer.Text = string.Format("@{0}", (string)post["photographer"]);

            if (Convert.ToInt32(post["ttBooks"]) > 1)
            {
                tt_books.IsVisible = true;
                tt_books.Text = string.Format("Essa foto foi adicionada a {0} livros de ideias", Convert.ToInt32(post["ttBooks"]));
            }

            await Task.Delay(500);

            // check if is like
            if (Convert.ToInt32(post["liked"]) > 0)
            {
                bt_like.IsVisible = false;
                bt_like_active.IsVisible = true;
            }
            else
            {
                bt_like.IsVisible = true;
                bt_like_active.IsVisible = false;
            }

            bt_like.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => {
                    likePost(idPost, true);

                })
            });

            bt_like_active.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => {
                    likePost(Convert.ToInt32(post["idPost"]), false);

                })
            });

            bt_save.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => {
                    Navigation.PushModalAsync(new PostSave(Convert.ToInt32(post["idPost"]), "post", Convert.ToInt32(post["idUser"]), Convert.ToInt32(post["idProject"])));
                })
            });

            string rating = "0";

            if ((int)res["post"]["userTtRating"] > 0)
                rating = Convert.ToString((int)res["post"]["userTtRating"]);

            postTotalRating.Text = string.Format("{0} Avaliações", rating);

            string star;

            for (int i = 0; i < 5; i++)
            {
                star = "star_null";

                if ((float)res["post"]["userRating"] >= (i + 1))
                    star = "star_green";

                block_rating.Children.Add(new Image
                {
                    Source = star,
                    HeightRequest = 10
                });

            }

            var gridExplorer = new Grid();

            int col = 0;
            int row = 0;

            gridExplorer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridExplorer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridExplorer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            int tt_posts = 0;

            foreach (var item in res["project"]["posts"])
                tt_posts++;

            if (tt_posts > 0)
            {

                blockOtherPosts.IsVisible = true;
                blockOtherPosts_div.IsVisible = true;

                for (int i = 0; i < ((tt_posts / 3) + ((tt_posts % 3) > 0 ? 1 : 0)); i++)
                    gridExplorer.RowDefinitions.Add(new RowDefinition { Height = 100 });

                int g = 0;
                foreach (var item in res["project"]["posts"])
                {

                    Image explorerImage = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 150,
                        Source = ImageRender.display("post", (string)item["image"])
                    };

                    explorerImage.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(() =>
                        {
                            App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(item["idPost"])));
                        })
                    });

                    gridExplorer.Children.Add(explorerImage, col, row);

                    if (((g + 1) % 3) == 0)
                    {
                        col = 0;
                        row++;
                    }
                    else if (((g + 1) % 2) == 0)
                    {
                        col = 1;
                    }
                    else
                    {
                        col = 2;
                    }

                    g++;

                }

                otherPosts.Children.Add(gridExplorer);
            }

            StackLayout block_bt_projectlist = new StackLayout
            {
                Padding = new Thickness(20, 0),
                Margin = new Thickness(0, 10),
                Orientation = StackOrientation.Horizontal
            };

            Button bt_projectlist = new Button
            {

                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                WidthRequest = 300,
                FontSize = 14,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Ver todos os projetos"
            };

            bt_projectlist.Clicked += delegate
            {

                IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(idUser) },
                       { "module", "projectlist" },
                       { "idModule", null },
                    };
                App.Current.MainPage = new NavigationPage(new Profile(args));

            };


            block_bt_projectlist.Children.Add(bt_projectlist);
            blockOtherPosts.Children.Add(block_bt_projectlist);



            int ttProjectsRelated = 0;

            foreach (var item in res["projects_related"])
                ttProjectsRelated++;

            if (ttProjectsRelated > 0)
            {
                ProjRelated_block.IsVisible = true;
                ProjRelated_div.IsVisible = true;

                foreach (var item in res["projects_related"])
                {
                    StackLayout dataProjRelated = new StackLayout
                    {
                        HeightRequest = 150,
                    };

                    BlockProjRelated.Children.Add(dataProjRelated);

                    dataProjRelated.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(() => {
                            IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", Convert.ToString(Convert.ToInt32(item["idUser"])) },
                       { "module", "project" },
                       { "idModule", Convert.ToString(Convert.ToInt32(item["idProject"])) },
                    };
                            App.Current.MainPage = new NavigationPage(new Profile(args));
                        })
                    });

                    RelativeLayout block_rel = new RelativeLayout();

                    dataProjRelated.Children.Add(block_rel);

                    Image categoryImage = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 150,
                        WidthRequest = 150,
                        Source = ImageRender.display("post", (string)item["image"])
                    };

                    block_rel.Children.Add(categoryImage,
                       xConstraint: Constraint.RelativeToParent((parent) => {
                           return parent.X;
                       }),

                        yConstraint: Constraint.RelativeToParent((parent) => {
                            return parent.Y;
                        })
                    );
                    BoxView categoryBox = new BoxView
                    {
                        Opacity = 0.5,
                        BackgroundColor = Color.Black
                    };

                    block_rel.Children.Add(categoryBox,
                        widthConstraint: Constraint.RelativeToParent((parent) => {
                            return parent.Width;
                        }),

                        heightConstraint: Constraint.RelativeToParent((parent) => {
                            return parent.Height;
                        }),

                        xConstraint: Constraint.RelativeToParent((parent) => {
                            return parent.X;
                        }),

                        yConstraint: Constraint.RelativeToParent((parent) => {
                            return parent.Y;
                        })
                    );

                    StackLayout projRelatedData = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical
                    };

                    projRelatedData.Children.Add(new Label
                    {
                        Text = (string)item["title"],
                        HeightRequest = 150,
                        WidthRequest = 150,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Bold" :
                        Device.RuntimePlatform == Device.Android ? "Jura-Bold.ttf#Jura-Bold" : "Assets/Fonts/Jura-Bold.ttf#Jura-Bold",
                        FontSize = 21,
                        TextColor = Color.White

                    });

                    block_rel.Children.Add(projRelatedData,
                        heightConstraint: Constraint.RelativeToParent((parent) => {
                            return parent.Height;
                        }),

                        xConstraint: Constraint.RelativeToParent((parent) => {
                            return parent.X;
                        }),

                        yConstraint: Constraint.RelativeToParent((parent) => {
                            return parent.Y;
                        })
                    );
                }
            }

            int ttComents = 0;

            foreach (var item in res["comments"])
                ttComents++;

            if (ttComents > 0)
            {
                comments_div.IsVisible = true;
                comments_title.IsVisible = true;

                foreach (var item in res["comments"])
                {

                    StackLayout commentBlock = new StackLayout
                    {
                        VerticalOptions = LayoutOptions.End,
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 0
                    };

                    commentBlock.Children.Add(new CircleImage
                    {
                        WidthRequest = 50,
                        HeightRequest = 50,
                        VerticalOptions = LayoutOptions.Start,
                        BorderColor = Color.DarkGray,
                        BorderThickness = 1,
                        Aspect = Aspect.AspectFill,
                        HorizontalOptions = LayoutOptions.Fill,
                        Source = ImageRender.display("user", (string)item["imgUser"])
                    });

                    StackLayout commentData = new StackLayout
                    {
                        Padding = new Thickness(5, 0, 0, 0),
                        Orientation = StackOrientation.Vertical
                    };

                    commentData.Children.Add(new Label
                    {
                        Text = (string)item["userName"],
                        FontAttributes = FontAttributes.Bold,
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-SemiboldItalic" :
                            Device.RuntimePlatform == Device.Android ? "OpenSans-SemiboldItalic.ttf#OpenSans-SemiboldItalic" : "Assets/Fonts/OpenSans-SemiboldItalic.ttf#OpenSans-SemiboldItalic"
                    });

                    commentData.Children.Add(new Label
                    {
                        Text = (string)item["comment"],
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-LightItalic" :
                            Device.RuntimePlatform == Device.Android ? "OpenSans-LightItalic.ttf#OpenSans-LightItalic" : "Assets/Fonts/OpenSans-LightItalic.ttf#OpenSans-LightItalic"
                    });

                    commentBlock.Children.Add(commentData);
                    postComments.Children.Add(commentBlock);

                    StackLayout messageDivisor = new StackLayout
                    {
                        Margin = new Thickness(0, 5)
                    };

                    messageDivisor.Children.Add(new BoxView
                    {
                        HeightRequest = 1,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.EndAndExpand,
                        Color = Color.FromHex("#e1e1e1")
                    });

                    postComments.Children.Add(messageDivisor);

                }
            }

        }

        public async void likePost(int idPost, bool status)
        {
            string endpoint = "portalib-dev-post";

            IDictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "idPost", Convert.ToString(idPost) },
                { "idUser", Convert.ToString(Settings.config_user) },
                { "status", Convert.ToString(status).ToLower()}
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "like" },
                { "mod", "post" }
            };

            await decora.Service.Run(endpoint, call, "POST", parameters);

            if (!status)
            {
                bt_like.IsVisible = true;
                bt_like_active.IsVisible = false;
            }
            else
            {
                bt_like.IsVisible = false;
                bt_like_active.IsVisible = true;
            }
        }

        public void goToHome()
        {
            App.Current.MainPage = new NavigationPage(new Timeline());
        }

        public void goToExplorer()
        {

            App.Current.MainPage = new NavigationPage(new Explorer());
        }

        public void goToProfile()
        {

            IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", "1" },
                       { "module", "general" },
                       { "idModule", null }
                    };

            App.Current.MainPage = new NavigationPage(new Profile(args));
        }

    }
}