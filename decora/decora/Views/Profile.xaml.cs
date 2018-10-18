using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ImageCircle.Forms.Plugin.Abstractions;
using decora.Helpers;
using Plugin.Share;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Profile : ContentPage
    {

        public string usrType { get; set; }
        public string bio { get; set; }
        public int userId { get; set; }
        public int projectId { get; set; }
        public int bookId { get; set; }
        public IDictionary<string, string> profileArgs { get; set; }

        public Profile (IDictionary<string, string> args)
        {

            profileArgs = args;

            App.lp.page = "profile";
            App.lp.page_id = Convert.ToInt32(profileArgs["idUser"]);
            App.lp.module = profileArgs["module"];
            App.lp.module_id = Convert.ToInt32(profileArgs["idModule"]);

            InitializeComponent();

        }

        protected override async void OnAppearing()
        {
            if (CheckConnection.validate())
            {
                bool validate = await CheckAuth.validate();
                if (!validate)
                {

                    App.Current.MainPage = new NavigationPage(new Login());
                }
                else
                {
                    userId = Convert.ToInt32(profileArgs["idUser"]);

                    makeProfile(Convert.ToInt32(profileArgs["idUser"]), profileArgs["module"], Convert.ToInt32(profileArgs["idModule"]));
                }
            }
            else
            {
                App.Current.MainPage = new NavigationPage(new FailConnection());
            }

            base.OnAppearing();
        }

        public void openMenu()
        {
            Navigation.PushModalAsync(new Menu());
        }

        public async void makeProfile(int IdUser, string module, int idModule = 0)
        {

            string endpoint = "portalib-dev-profile";
            string star;



            menuHome.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(goToHome),
                CommandParameter = IdUser
            });

            menuProject.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(goToProjectList),
                CommandParameter = IdUser
            });

            menuBook.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(goToBookList),
                CommandParameter = IdUser
            });

            menuRating.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(goToRating),
                CommandParameter = IdUser
            });

            menuActivity.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(goToActivity),
                CommandParameter = IdUser
            });

            menuMessage.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(goToMessageList),
                CommandParameter = IdUser
            });

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idUserProfile", IdUser },
                { "idUser", Convert.ToInt32(Settings.config_user) }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "data" },
                { "mod", "profile" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            bio = (string)res["biography"];

            usrType = (string)res["userType"];

            if (usrType == "professional")
            {
                menu_project_div.IsVisible = true;
                menu_rating_div.IsVisible = true;
                menu_activity_div.IsVisible = true;
                menu_message_div.IsVisible = true;
                menu_project.IsVisible = true;
                menu_rating.IsVisible = true;
                menu_activity.IsVisible = true;
                menu_message.IsVisible = true;

                block_ratin_profile.IsVisible = true;
            }
            else
            {
                profileName.Margin = new Thickness(0, 20, 0, 0);
            }

            if (string.IsNullOrEmpty(bio))
                bio = string.Format("Nenhuma biografia para {0}.", (string)res["name"]);

            profileName.Text = (string)res["name"];

            imageProfile.Source = ImageRender.display("user", (string)res["image"]);
            coverProfile.Source = ImageRender.display("cover", (string)res["cover"]);

            await Task.Delay(500);

            bt_share.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(async () => {

                    await CrossShare.Current.Share(new Plugin.Share.Abstractions.ShareMessage()
                    {

                        Text = string.Format("Baixe o app Portal Decoração, e veja o perfil de {0}, eu gostei.", (string)res["name"]),
                        Title = "Portal Decoração"
                    });
                })
            });

            bt_email.Clicked += delegate
            {
                Device.OpenUri(new Uri($"mailto:{(string)res["email"]}"));
            };

            bt_phone.Clicked += delegate
            {
                Device.OpenUri(new Uri($"tel:{string.Format("+55{0}", (string)res["phone"])}"));
            };

            if (usrType == "professional")
            {

                string rating = "0";

                if ((int)res["ttRating"] > 0)
                    rating = Convert.ToString((int)res["ttRating"]);

                tt_rating.Text = string.Format("{0} Avaliações", rating);

                tt_rating.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        goToRating(IdUser);
                    })
                });

                blockStarts.Children.Clear();

                for (int i = 0; i < 5; i++)
                {
                    star = "star";

                    if ((float)res["rating"] >= (i + 1))
                        star = "star_dark";

                    blockStarts.Children.Add(new Image
                    {
                        Source = star,
                        HeightRequest = 30,
                        WidthRequest = 20,
                        Margin = new Thickness(3, 0)
                    });

                }

            }

            if (IdUser == Convert.ToInt32(Settings.config_user))
            {
                bt_follow_edit.Text = "EDITAR";
                editPhoto.IsVisible = true;

                imageProfile.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                    {
                        await Navigation.PushModalAsync(new ProfileImageEdit(IdUser));
                    })
                });

                bt_follow_edit.Clicked += delegate
                {
                    Navigation.PushModalAsync(new ProfileEdit(IdUser));
                };

            }
            else
            {
                bt_follow_edit.Text = "SIGA-ME";
                bt_follow_edit.IsVisible = false;

                if (Convert.ToInt32(res["follow"]) > 0)
                {
                    bt_save.IsVisible = false;
                    bt_follow_edit.IsVisible = false;
                    bt_save_out.IsVisible = true;
                    bt_follow_out.IsVisible = true;
                }
                else
                {
                    bt_save.IsVisible = true;
                    bt_follow_edit.IsVisible = true;
                    bt_save_out.IsVisible = false;
                    bt_follow_out.IsVisible = false;
                }

                bt_share.IsVisible = true;
                bar_ratingProfile.IsVisible = true;
                bt_ratingProfile.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(goToRatingNew),
                    CommandParameter = IdUser
                });

                bt_ratingProfile.IsVisible = true;

                bt_save.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => {
                        followUser(IdUser, true);

                    })
                });

                bt_follow_edit.Clicked += delegate
                {
                    followUser(IdUser, true);
                };

                bt_save_out.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => {
                        followUser(IdUser, false);

                    })
                });

                bt_follow_out.Clicked += delegate
                {
                    followUser(IdUser, false);
                };

            }

            if (!string.IsNullOrEmpty((string)res["website"]))
            {

                string website = string.Format("{0}", (string)res["website"]);

                if (website.IndexOf("http") < 0)
                    website = string.Format("http://{0}", website);

                stack_bt_website.IsVisible = true;
                bt_website.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        Device.OpenUri(new Uri(website));
                    })
                });
            }

            if (!string.IsNullOrEmpty((string)res["link_instagram"]))
            {

                string website_instagram = string.Format("{0}", (string)res["link_instagram"]);

                if (website_instagram.IndexOf("http") < 0)
                    website_instagram = string.Format("http://{0}", website_instagram);

                link_instagram.IsVisible = true;
                link_instagram.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        Device.OpenUri(new Uri(website_instagram));
                    })
                });
            }

            if (!string.IsNullOrEmpty((string)res["link_facebook"]))
            {

                string website_facebook = string.Format("{0}", (string)res["link_facebook"]);

                if (website_facebook.IndexOf("http") < 0)
                    website_facebook = string.Format("http://{0}", website_facebook);

                link_facebook.IsVisible = true;
                link_facebook.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        Device.OpenUri(new Uri(website_facebook));
                    })
                });
            }

            if (!string.IsNullOrEmpty((string)res["link_linkedin"]))
            {

                string website_linkedin = string.Format("{0}", (string)res["link_linkedin"]);

                if (website_linkedin.IndexOf("http") < 0)
                    website_linkedin = string.Format("http://{0}", website_linkedin);

                link_linkedin.IsVisible = true;
                link_linkedin.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        Device.OpenUri(new Uri(website_linkedin));
                    })
                });
            }

            if (!string.IsNullOrEmpty((string)res["link_twitter"]))
            {

                string website_twitter = string.Format("{0}", (string)res["link_twitter"]);

                if (website_twitter.IndexOf("http") < 0)
                    website_twitter = string.Format("http://{0}", website_twitter);

                link_twitter.IsVisible = true;
                link_twitter.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        Device.OpenUri(new Uri(website_twitter));
                    })
                });
            }

            if (!string.IsNullOrEmpty((string)res["link_gplus"]))
            {

                string website_gplus = string.Format("{0}", (string)res["link_gplus"]);

                if (website_gplus.IndexOf("http") < 0)
                    website_gplus = string.Format("http://{0}", website_gplus);

                link_gplus.IsVisible = true;
                link_gplus.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        Device.OpenUri(new Uri(website_gplus));
                    })
                });
            }

            switch (module)
            {
                case "general": await makeGeneralVision(IdUser); break;
                case "projectlist": await makeProjectList(IdUser); break;
                case "project": await makeProject(idModule); break;
                case "booklist": await makeBookList(IdUser); break;
                case "book": await makeBook(idModule); break;
                case "ratinglist": await makeRatingList(IdUser); break;
                case "rating": await makeRating(idModule); break;
                case "activity": await makeActivity(IdUser); break;
                case "messagelist": await makeMessageList(IdUser); break;
                case "message": await makeMessage(idModule); break;
            }

        }

        public async void followUser(int IdUser, bool status)
        {
            string endpoint = "portalib-dev-profile";

            IDictionary<string, string> parameters = new Dictionary<string, string>()
                     {
                        { "idUser", Convert.ToString(IdUser) },
                        { "idUserFollower", Convert.ToString(Settings.config_user) },
                        { "status", Convert.ToString(status).ToLower()}
                     };


            IDictionary<string, string> call = new Dictionary<string, string>
                {
                    { "act", "follow" },
                    { "mod", "profile" }
                };

            await decora.Service.Run(endpoint, call, "POST",parameters);

            if (!status)
            {
                bt_save.IsVisible = true;
                bt_follow_edit.IsVisible = true;
                bt_save_out.IsVisible = false;
                bt_follow_out.IsVisible = false;
            }
            else
            {
                bt_save.IsVisible = false;
                bt_follow_edit.IsVisible = false;
                bt_save_out.IsVisible = true;
                bt_follow_out.IsVisible = true;

            }
        }

        public async void likePost(int idPost, bool status, Image like, Image like_active)
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
                like.IsVisible = true;
                like_active.IsVisible = false;
            }
            else
            {
                like.IsVisible = false;
                like_active.IsVisible = true;
            }
        }

        public async Task<bool> makeGeneralVision(int IdUser)
        {

            contentIndicator.IsVisible = true;

            pageContent.Children.Clear();

            string endpoint = "portalib-dev-profile";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
            { "idUser", IdUser }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "general" },
                { "mod", "profile" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            /* BLOCK DIVISOR */
            StackLayout divisor = new StackLayout
            {
                Margin = new Thickness(0, 5)
            };

            divisor.Children.Add(new BoxView
            {
                HeightRequest = 15,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand,
                Color = Color.FromHex("#e1e1e1")
            });

            /* BLOCK BIO */
            StackLayout bioBlock = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Margin = new Thickness(10, 0)
            };

            bioBlock.Children.Add(new Label
            {
                Text = "BIO",
                FontAttributes = FontAttributes.Bold,
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Regular" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Regular.ttf#OpenSans-Regular" : "Assets/Fonts/OpenSans-Regular.ttf#OpenSans-Regular",
                FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                    Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this)
            });

            bioBlock.Children.Add(new Label
            {
                Text = bio,
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
            });

            /* bioBlock.Children.Add(new Label
             {
                 Text = "Veja Mais [+]",
                 Margin = new Thickness(0, 10),
                 HorizontalOptions = LayoutOptions.Start,
                 FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Italic" :
                     Device.RuntimePlatform == Device.Android ? "OpenSans-Italic.ttf#OpenSans-Italic" : "Assets/Fonts/OpenSans-Italic.ttf#OpenSans-Italic"
             });
             */
            /* BLOCK DIVISOR */


            /* ADD BLOCKS */
            pageContent.Children.Add(bioBlock);
            pageContent.Children.Add(divisor);

            StackLayout Pdivisor = new StackLayout
            {
                Margin = new Thickness(0, 5)
            };

            Pdivisor.Children.Add(new BoxView
            {
                HeightRequest = 15,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand,
                Color = Color.FromHex("#e1e1e1")
            });

            /* BLOCK PROJECT */

            int tt = 0;

            int c = 0;

            foreach (var item in res["projects"])
                tt++;

            if ((tt > 0) && (usrType == "professional"))
            {

                StackLayout projectBlock = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    Margin = new Thickness(10, 0)
                };

                projectBlock.Children.Add(new Label
                {
                    Text = string.Format("{0} PROJETOS >", tt),
                    Margin = new Thickness(0, 5),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.Start
                });

                //grid

                var gridProjects = new Grid();

                int col = 0;
                int row = 0;



                if (tt > 0)
                {


                    for (int i = 0; i < ((tt / 2) + ((tt % 2) > 0 ? 1 : 0)); i++)
                    {
                        gridProjects.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        gridProjects.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    }

                    foreach (var item in res["projects"])
                    {

                        StackLayout gridProjectBlock = new StackLayout
                        {
                            HeightRequest = 150,
                            Spacing = 0
                        };

                        RelativeLayout block_rel = new RelativeLayout();

                        gridProjectBlock.Children.Add(block_rel);

                        Image projectImage = new Image
                        {
                            Aspect = Aspect.AspectFill,
                            HeightRequest = 150,
                            Source = ImageRender.display("post", (string)item["image"])
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

                        gridProjectBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                        {
                            Command = new Command(async () =>
                            {
                                await makeProject(Convert.ToInt32(item["idProject"]));
                            })
                        });

                        gridProjects.Children.Add(gridProjectBlock, row, col);

                        if (((c + 1) % 2) == 0)
                        {
                            col = 0;
                            row++;
                        }
                        else
                        {
                            col = 1;
                        }

                        c++;

                    }

                }


                projectBlock.Children.Add(gridProjects);

                StackLayout moreProjectsBlock = new StackLayout
                {
                    Margin = new Thickness(0, 10),
                    Orientation = StackOrientation.Horizontal
                };

                Button btMoreProjects = new Button
                {
                    Text = "Ver todos os projetos",
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 14 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    WidthRequest = 300,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                };

                btMoreProjects.Clicked += delegate
                {
                    goToProjectList(IdUser);
                };

                moreProjectsBlock.Children.Add(btMoreProjects);

                projectBlock.Children.Add(moreProjectsBlock);

                pageContent.Children.Add(projectBlock);
                pageContent.Children.Add(Pdivisor);

            }

            /* BLOCK BOOK */

            tt = 0;
            c = 0;

            foreach (var item in res["books"])
                tt++;

            if (tt > 0)
            {

                /* BLOCK DIVISOR */
                StackLayout Bdivisor = new StackLayout
                {
                    Margin = new Thickness(0, 5)
                };

                Bdivisor.Children.Add(new BoxView
                {
                    HeightRequest = 15,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.EndAndExpand,
                    Color = Color.FromHex("#e1e1e1")
                });

                StackLayout bookBlock = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    Margin = new Thickness(10, 0)
                };

                bookBlock.Children.Add(new Label
                {
                    Text = string.Format("{0} LIVROS DE IDEIAS >", tt),
                    Margin = new Thickness(0, 5),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.Start
                });

                //grid

                var gridBook = new Grid();

                gridBook.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                gridBook.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                gridBook.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                gridBook.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                int rowB = 0;

                if (tt > 0)
                {

                    foreach (var item in res["books"])
                    {

                        StackLayout gridBookBlock = new StackLayout
                        {
                            HeightRequest = 150,
                            Spacing = 0
                        };

                        RelativeLayout block_rel = new RelativeLayout();

                        gridBookBlock.Children.Add(block_rel);

                        Image projectImage = new Image
                        {
                            Aspect = Aspect.AspectFill,
                            HeightRequest = 150,
                            Source = ImageRender.display("post", (string)item["image"])
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

                        BoxView bookBox = new BoxView
                        {
                            Opacity = 0.6,
                            BackgroundColor = Color.Black
                        };

                        block_rel.Children.Add(bookBox,
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

                        gridBookBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                        {
                            Command = new Command(async () =>
                            {
                                await makeBook(Convert.ToInt32(item["idBook"]));
                            })
                        });

                        gridBook.Children.Add(gridBookBlock, 0, rowB);

                        if (Convert.ToInt32(Settings.config_user) == IdUser)
                        {

                            CircleImage editBook = new CircleImage
                            {
                                Source = "edit",
                                Aspect = Aspect.AspectFill,
                                WidthRequest = 25,
                                HeightRequest = 25,
                                BorderColor = Color.White,
                                HorizontalOptions = LayoutOptions.End,
                                Margin = new Thickness(10)
                            };

                            editBook.GestureRecognizers.Add(new TapGestureRecognizer()
                            {
                                Command = new Command(() =>
                                {
                                    Navigation.PushModalAsync(new BookEdit(Convert.ToInt32(IdUser), Convert.ToInt32(item["idBook"]), "general"));
                                })
                            });

                            block_rel.Children.Add(editBook,
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
                                })
                            );

                        }

                        rowB++;

                    }

                }

                bookBlock.Children.Add(gridBook);

                StackLayout moreBookBlock = new StackLayout
                {
                    Margin = new Thickness(0, 10),
                    Orientation = StackOrientation.Horizontal
                };

                Button btMoreBooks = new Button
                {
                    Text = "Ver todos os livros de ideias",
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 14 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    WidthRequest = 300,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                };

                btMoreBooks.Clicked += delegate
                {
                    goToBookList(IdUser);
                };

                moreBookBlock.Children.Add(btMoreBooks);

                bookBlock.Children.Add(moreBookBlock);

                pageContent.Children.Add(bookBlock);
                pageContent.Children.Add(Bdivisor);

            }
            /* BLOCK RATING */
            tt = 0;
            c = 0;

            foreach (var item in res["ratings"])
                tt++;

            if ((tt > 0) && (usrType == "professional"))
            {

                /* BLOCK DIVISOR */
                StackLayout Rdivisor = new StackLayout
                {
                    Margin = new Thickness(0, 5)
                };

                Rdivisor.Children.Add(new BoxView
                {
                    HeightRequest = 15,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.EndAndExpand,
                    Color = Color.FromHex("#e1e1e1")
                });

                StackLayout ratingBlock = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    Margin = new Thickness(10, 0)
                };

                ratingBlock.Children.Add(new Label
                {
                    Text = string.Format("AVALIAÇÕES >", tt),
                    Margin = new Thickness(0, 5),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.Start
                });

                if (tt > 0)
                {

                    foreach (var item in res["ratings"])
                    {

                        StackLayout ratingDataBlock = new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal
                        };

                        CircleImage userRating = new CircleImage
                        {
                            WidthRequest = 50,
                            HeightRequest = 50,
                            BorderColor = Color.LightGray,
                            BorderThickness = 1,
                            HorizontalOptions = LayoutOptions.Fill,
                            VerticalOptions = LayoutOptions.Start,
                            Aspect = Aspect.AspectFill,
                            Source = ImageRender.display("user", (string)item["imgsrater"])
                        };

                        userRating.GestureRecognizers.Add(new TapGestureRecognizer()
                        {
                            Command = new Command(() =>
                            {
                                IDictionary<string, string> args = new Dictionary<string, string>()
                                {
                                { "idUser", Convert.ToString(item["idRater"]) },
                                { "module", "general" },
                                {"idModule", null }
                                };

                                App.Current.MainPage = new NavigationPage(new Profile(args));
                            })
                        });

                        ratingDataBlock.Children.Add(userRating);

                        StackLayout ratingDetailBlock = new StackLayout
                        {
                            Orientation = StackOrientation.Vertical,
                            Padding = new Thickness(5, 0, 0, 0)
                        };

                        StackLayout ratingStartsBlock = new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            Padding = new Thickness(0, -3),
                            Spacing = 0
                        };

                        string star;

                        for (int i = 0; i < 5; i++)
                        {
                            star = "star";

                            if ((float)item["rating"] >= (i + 1))
                                star = "star_dark";

                            ratingStartsBlock.Children.Add(new Image
                            {
                                Source = star,
                                HeightRequest = 24,
                                WidthRequest = 20,
                                Margin = new Thickness(3, 0)
                            });

                        }

                        ratingDetailBlock.Children.Add(ratingStartsBlock);

                        StackLayout ratingNameUserBlock = new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            Spacing = 0
                        };

                        ratingNameUserBlock.Children.Add(
                            new Label
                            {
                                Text = "Avaliado por ",
                                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
                            }
                        );

                        ratingNameUserBlock.Children.Add(
                            new Label
                            {
                                Text = (string)item["raterName"],
                                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                                    Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold"
                            }
                        );

                        ratingNameUserBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                        {
                            Command = new Command(() =>
                            {
                                IDictionary<string, string> args = new Dictionary<string, string>()
                                {
                               { "idUser", Convert.ToString(item["idRater"]) },
                               { "module", "general" },
                               { "module", null },
                                };

                                App.Current.MainPage = new NavigationPage(new Profile(args));
                            })
                        });

                        ratingDetailBlock.Children.Add(ratingNameUserBlock);

                        ratingDetailBlock.Children.Add(new Label
                        {
                            Text = (string)item["comment"],
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
                        });

                        Label goRating = new Label
                        {
                            Text = "[Saiba mais]",
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                            HorizontalOptions = LayoutOptions.Start
                        };

                        goRating.GestureRecognizers.Add(new TapGestureRecognizer()
                        {
                            Command = new Command(async () =>
                            {
                                await makeRating(Convert.ToInt32(item["idUserRating"]));
                            })
                        });

                        ratingDetailBlock.Children.Add(goRating);

                        ratingDataBlock.Children.Add(ratingDetailBlock);

                        ratingBlock.Children.Add(ratingDataBlock);

                        StackLayout ratingDivisor = new StackLayout
                        {
                            Margin = new Thickness(0, 5)
                        };

                        ratingDivisor.Children.Add(new BoxView
                        {
                            HeightRequest = 1,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            VerticalOptions = LayoutOptions.EndAndExpand,
                            Color = Color.FromHex("#e1e1e1")
                        });

                        ratingBlock.Children.Add(ratingDivisor);

                    }

                }

                pageContent.Children.Add(ratingBlock);
                pageContent.Children.Add(Rdivisor);

            }

            /* BLOCK ACTIVITY */

            tt = 0;
            c = 0;

            foreach (var item in res["activities"])
                tt++;

            if ((tt > 0) && (usrType == "professional"))
            {

                /* BLOCK DIVISOR */
                StackLayout Adivisor = new StackLayout
                {
                    Margin = new Thickness(0, 5)
                };

                Adivisor.Children.Add(new BoxView
                {
                    HeightRequest = 15,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.EndAndExpand,
                    Color = Color.FromHex("#e1e1e1")
                });

                StackLayout activityBlock = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    Margin = new Thickness(10, 0)
                };

                activityBlock.Children.Add(new Label
                {
                    Text = string.Format("ATIVIDADES >", 6),
                    Margin = new Thickness(0, 5),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.Start
                });

                foreach (var item in res["activities"])
                {


                    StackLayout activityDataBlock = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal
                    };

                    CircleImage userActivity = new CircleImage
                    {
                        WidthRequest = 50,
                        HeightRequest = 50,
                        BorderColor = Color.LightGray,
                        BorderThickness = 1,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Start,
                        Aspect = Aspect.AspectFill,
                        Source = ImageRender.display("user", (string)item["imgAction"])
                    };

                    userActivity.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(() =>
                        {
                            IDictionary<string, string> args = new Dictionary<string, string>()
                        {
                       { "idUser", Convert.ToString(item["idUserAction"]) },
                       { "module", "general" },
                       { "idModule", null },
                        };

                            App.Current.MainPage = new NavigationPage(new Profile(args));
                        })
                    });

                    activityDataBlock.Children.Add(userActivity);

                    StackLayout activityDetailBlock = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        Padding = new Thickness(5, 0, 0, 0)
                    };

                    string txtActivity;

                    switch ((string)item["action"])
                    {
                        case "like":
                            txtActivity = string.Format("{0} curtiu a sua foto '{1}'.", (string)item["userAction"], (string)item["title"]);
                            break;
                        case "save":
                            txtActivity = string.Format("{0} adicionou a sua foto '{1}' em um de seus livros de idéias.", (string)item["userAction"], (string)item["title"]);
                            break;
                        case "follow":
                            txtActivity = string.Format("{0} começou a te seguir.", (string)item["userAction"]);
                            break;
                        case "rating":
                            txtActivity = string.Format("{0} enviou uma avaliação.", (string)item["userAction"]);
                            break;
                        case "message":
                            txtActivity = string.Format("{0} enviou uma mensagem.", (string)item["userAction"]);
                            break;
                        default:
                            txtActivity = "Atividade realizada.";
                            break;
                    }

                    activityDetailBlock.Children.Add(
                        new Label
                        {
                            Text = txtActivity,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold"
                        }
                    );

                    activityDetailBlock.Children.Add(
                        new Label
                        {
                            Text = Date.sinceAgo((string)item["dtRegister"], Convert.ToInt32(item["since"])),
                            FontSize = 12,
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
                        }
                    );

                    switch ((string)item["action"])
                    {
                        case "like":

                            activityDetailBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                            {
                                Command = new Command(() =>
                                {
                                    App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(item["idAction"])));
                                })
                            });

                            break;
                        case "save":

                            activityDetailBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                            {
                                Command = new Command(() =>
                                {
                                    App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(item["idAction"])));
                                })
                            });

                            break;
                        case "rating":
                            activityDetailBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                            {
                                Command = new Command(async () =>
                                {
                                    await makeRating(Convert.ToInt32(item["idAction"]));
                                })
                            });
                            break;
                        case "message":

                            activityDetailBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                            {
                                Command = new Command(async () =>
                                {
                                    await makeMessage(Convert.ToInt32(item["idAction"]));
                                })
                            });
                            break;
                    }

                    activityDataBlock.Children.Add(activityDetailBlock);

                    activityBlock.Children.Add(activityDataBlock);

                    StackLayout activityDivisor = new StackLayout
                    {
                        Margin = new Thickness(0, 5)
                    };

                    activityDivisor.Children.Add(new BoxView
                    {
                        HeightRequest = 1,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.EndAndExpand,
                        Color = Color.FromHex("#e1e1e1")
                    });

                    activityBlock.Children.Add(activityDivisor);

                }

                pageContent.Children.Add(activityBlock);
                pageContent.Children.Add(Adivisor);

            }

            /* BLOCK MESSAGE */
            tt = 0;

            foreach (var item in res["messages"])
                tt++;

            if ((tt > 0) && (usrType == "professional"))
            {

                /* BLOCK DIVISOR */
                StackLayout Mdivisor = new StackLayout
                {
                    Margin = new Thickness(0, 5)
                };

                Mdivisor.Children.Add(new BoxView
                {
                    HeightRequest = 15,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.EndAndExpand,
                    Color = Color.FromHex("#e1e1e1")
                });

                StackLayout messageBlock = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    Margin = new Thickness(10, 0)
                };

                messageBlock.Children.Add(new Label
                {
                    Text = string.Format("MENSAGENS >", 6),
                    Margin = new Thickness(0, 5),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.Start
                });

                foreach (var item in res["messages"])
                {

                    StackLayout messageDataBlock = new StackLayout
                    {
                        VerticalOptions = LayoutOptions.End,
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 0
                    };

                    StackLayout messageDetailBlock = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        Padding = new Thickness(5, 0, 0, 0)
                    };

                    messageDetailBlock.Children.Add(
                        new Label
                        {
                            Text = "- - -",
                            FontAttributes = FontAttributes.Bold,
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-SemiboldItalic" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-SemiboldItalic.ttf#OpenSans-SemiboldItalic" : "Assets/Fonts/OpenSans-SemiboldItalic.ttf#OpenSans-SemiboldItalic"
                        }
                    );

                    messageDetailBlock.Children.Add(
                        new Label
                        {
                            Text = string.Format("{0}:", (string)item["senderName"]),
                            FontAttributes = FontAttributes.Bold,
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-SemiboldItalic" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-SemiboldItalic.ttf#OpenSans-SemiboldItalic" : "Assets/Fonts/OpenSans-SemiboldItalic.ttf#OpenSans-SemiboldItalic"
                        }
                    );

                    messageDetailBlock.Children.Add(
                        new Label
                        {
                            Text = (string)item["message"],
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-LightItalic" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-LightItalic.ttf#OpenSans-LightItalic" : "Assets/Fonts/OpenSans-LightItalic.ttf#OpenSans-LightItalic"
                        }
                    );

                    messageDetailBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(async () =>
                        {
                            await makeMessage(Convert.ToInt32(item["idMessage"]));
                        })
                    });

                    messageDetailBlock.Children.Add(
                        new Label
                        {
                            Text = Date.sinceAgo((string)item["dtRegister"], Convert.ToInt32(item["since"])),
                            FontSize = 12,
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
                        }
                    );

                    messageDataBlock.Children.Add(messageDetailBlock);

                    messageDataBlock.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(async () =>
                        {
                            await makeMessage(Convert.ToInt32(item["idMessage"]));
                        })
                    });

                    messageBlock.Children.Add(messageDataBlock);

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

                    messageBlock.Children.Add(messageDivisor);

                }

                StackLayout messagebtBlock = new StackLayout { };

                Button btNewMessage = new Button
                {
                    Text = "FAÇA UMA PERGUNTA",
                    WidthRequest = 200,
                    HeightRequest = 50,
                    TextColor = Color.White,
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Regular" :
                                Device.RuntimePlatform == Device.Android ? "Jura-Regular.ttf#Jura-Regular" : "Assets/Fonts/Jura-Regular.ttf#Jura-Regular",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    BackgroundColor = Color.FromHex("#667653")
                };

                btNewMessage.Clicked += delegate
                {
                    Navigation.PushModalAsync(new MessageNew(IdUser));
                };

                messagebtBlock.Children.Add(btNewMessage);

                messageBlock.Children.Add(messagebtBlock);

                pageContent.Children.Add(messageBlock);
                pageContent.Children.Add(Mdivisor);

            }

            contentIndicator.IsVisible = false;

            return true;

        }

        public async Task<bool> makeProjectList(int IdUser)
        {

            contentIndicator.IsVisible = true;

            pageContent.Children.Clear();

            string endpoint = "portalib-dev-project";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idUser", IdUser }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "list" },
                { "mod", "project" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            int tt = 0;

            foreach (var item in res)
                tt++;

            StackLayout title = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            if (tt > 0)
            {

                title.Children.Add(new Label
                {
                    Text = string.Format("{0} PROJETOS >", tt),
                    Margin = new Thickness(0, 0, 0, 10),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 24 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.StartAndExpand

                });

            }
            else
            {

                title.Children.Add(new Label
                {
                    Text = "Nenhum projeto encontrado.",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.StartAndExpand

                });

            }

            if (IdUser == Convert.ToInt32(Settings.config_user))
            {

                Button btNewProject = new Button
                {
                    Text = "+ Novo Projeto",
                    HeightRequest = 50,
                    TextColor = Color.White,
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Regular" :
                            Device.RuntimePlatform == Device.Android ? "Jura-Regular.ttf#Jura-Regular" : "Assets/Fonts/Jura-Regular.ttf#Jura-Regular",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.End,
                    BackgroundColor = Color.FromHex("#667653")
                };

                btNewProject.Clicked += goToProjectNew;

                title.Children.Add(btNewProject);
            }

            pageContent.Children.Add(title);

            if (tt > 0)
            {

                foreach (var item in res)
                {

                    StackLayout block = new StackLayout
                    {
                        HeightRequest = 300,
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    pageContent.Children.Add(block);

                    block.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(async () =>
                        {
                            await makeProject(Convert.ToInt32(item["idProject"]));
                        })
                    });

                    RelativeLayout block_rel = new RelativeLayout();

                    block.Children.Add(block_rel);

                    Image projectImage = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 300,
                        Source = ImageRender.display("post", (string)item["image"])
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
                            return parent.Height - (parent.Height - 70);
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
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Medium" :
                        Device.RuntimePlatform == Device.Android ? "Jura-Medium.ttf#Jura-Medium" : "Assets/Fonts/Jura-Medium.ttf#Jura-Medium",
                        FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
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
                        FontSize = Device.RuntimePlatform == Device.iOS ? 14 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
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
                            return parent.Height - 60;
                        })
                    );

                }
            }

            contentIndicator.IsVisible = false;

            return true;

        }

        public async Task<bool> makeBookList(int IdUser)
        {

            contentIndicator.IsVisible = true;

            pageContent.Children.Clear();

            string endpoint = "portalib-dev-book";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idUser", IdUser }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "list" },
                { "mod", "book" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            StackLayout title = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            int tt = 0;

            foreach (var item in res)
                tt++;

            if (tt > 0)
            {

                title.Children.Add(new Label
                {
                    Text = string.Format("{0} LIVRO DE IDEIAS >", tt),
                    Margin = new Thickness(0, 5),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.StartAndExpand
                });

            }
            else
            {

                title.Children.Add(new Label
                {
                    Text = "Nenhum livro de idéias encontrado.",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.StartAndExpand

                });

            }

            if (IdUser == Convert.ToInt32(Settings.config_user))
            {

                Button btNewBook = new Button
                {
                    Text = "+ Novo Livro",
                    HeightRequest = 50,
                    TextColor = Color.White,
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Regular" :
                            Device.RuntimePlatform == Device.Android ? "Jura-Regular.ttf#Jura-Regular" : "Assets/Fonts/Jura-Regular.ttf#Jura-Regular",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.End,
                    BackgroundColor = Color.FromHex("#667653")
                };

                btNewBook.Clicked += goToBookNew;

                title.Children.Add(btNewBook);

            }
            pageContent.Children.Add(title);

            if (tt > 0)
            {

                var gridBook = new Grid();

                int col = 0;
                int row = 0;

                gridBook.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                gridBook.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                for (int i = 0; i < ((tt / 2) + ((tt % 2) > 0 ? 1 : 0)); i++)
                    gridBook.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });


                int count = 0;
                foreach (var item in res)
                {

                    StackLayout gridBookBlock = new StackLayout
                    {
                        HeightRequest = 150,
                        Spacing = 0
                    };

                    RelativeLayout block_rel = new RelativeLayout();

                    gridBookBlock.Children.Add(block_rel);

                    Image bookImage = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 150,
                        Source = ImageRender.display("post", (string)item["image"])
                    };

                    block_rel.Children.Add(bookImage,
                        widthConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Width;
                        }),

                        heightConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Height;
                        })
                    );

                    BoxView bookBox = new BoxView
                    {
                        Opacity = 0.6,
                        BackgroundColor = Color.Black
                    };


                    block_rel.Children.Add(bookBox,
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

                    StackLayout bookData = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        Margin = new Thickness(10, 0, 0, 0)
                    };

                    bookData.Children.Add(new Label
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

                    bookData.Children.Add(new Label
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


                    block_rel.Children.Add(bookData,
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

                    gridBookBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(async () =>
                        {
                            await makeBook(Convert.ToInt32(item["idBook"]));
                        })
                    });


                    if (Convert.ToInt32(Settings.config_user) == IdUser)
                    {

                        CircleImage editBook = new CircleImage
                        {
                            Source = "edit",
                            Aspect = Aspect.AspectFill,
                            WidthRequest = 25,
                            HeightRequest = 25,
                            BorderColor = Color.White,
                            HorizontalOptions = LayoutOptions.End,
                            Margin = new Thickness(10)
                        };

                        editBook.GestureRecognizers.Add(new TapGestureRecognizer()
                        {
                            Command = new Command(() =>
                            {
                                Navigation.PushModalAsync(new BookEdit(Convert.ToInt32(IdUser), Convert.ToInt32(item["idBook"]), "booklist"));
                            })
                        });

                        block_rel.Children.Add(editBook,
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
                            })
                        );

                    }

                    gridBook.Children.Add(gridBookBlock, col, row);

                    if (((count + 1) % 2) == 0)
                    {
                        col = 0;
                        row++;
                    }
                    else
                    {
                        col = 1;
                    }

                    count++;

                }

                pageContent.Children.Add(gridBook);
            }

            contentIndicator.IsVisible = false;

            return true;
        }

        public async Task<bool> makeRatingList(int IdUser)
        {

            contentIndicator.IsVisible = true;

            pageContent.Children.Clear();

            string endpoint = "portalib-dev-rating";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idUser", IdUser }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "data" },
                { "mod", "rating" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            int tt = 0;

            foreach (var item in res)
                tt++;

            if (tt > 0)
            {

                pageContent.Children.Add(new Label
                {
                    Text = string.Format("AVALIAÇÕES >", 6),
                    Margin = new Thickness(0, 5),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                        Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.Start
                });
            }
            else
            {

                pageContent.Children.Add(new Label
                {
                    Text = "Nenhuma avaliação realizada até o momento.",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.StartAndExpand

                });

            }

            if (tt > 0)
            {

                foreach (var item in res)
                {

                    StackLayout ratingDataBlock = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal
                    };

                    CircleImage userRating = new CircleImage
                    {
                        WidthRequest = 50,
                        HeightRequest = 50,
                        BorderColor = Color.LightGray,
                        BorderThickness = 1,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Start,
                        Aspect = Aspect.AspectFill,
                        Source = ImageRender.display("user", (string)item["imgsrater"])
                    };

                    userRating.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(() =>
                        {
                            IDictionary<string, string> args = new Dictionary<string, string>()
                            {
                                { "idUser", Convert.ToString(item["idRater"]) },
                                { "module", "general" },
                                {"idModule", null }
                            };

                            App.Current.MainPage = new NavigationPage(new Profile(args));
                        })
                    });

                    ratingDataBlock.Children.Add(userRating);

                    StackLayout ratingDetailBlock = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        Padding = new Thickness(5, 0, 0, 0)
                    };

                    StackLayout ratingStartsBlock = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Padding = new Thickness(0, -3),
                        Spacing = 0
                    };

                    string star;

                    for (int i = 0; i < 5; i++)
                    {
                        star = "star";

                        if ((float)item["rating"] >= (i + 1))
                            star = "star_dark";

                        ratingStartsBlock.Children.Add(new Image
                        {
                            Source = star,
                            HeightRequest = 24,
                            WidthRequest = 20,
                            Margin = new Thickness(3, 0)
                        });

                    }

                    ratingDetailBlock.Children.Add(ratingStartsBlock);

                    StackLayout ratingNameUserBlock = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 0
                    };

                    ratingNameUserBlock.Children.Add(
                        new Label
                        {
                            Text = "Avaliado por ",
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
                        }
                    );

                    ratingNameUserBlock.Children.Add(
                        new Label
                        {
                            Text = (string)item["raterName"],
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold"
                        }
                    );

                    ratingNameUserBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(() =>
                        {
                            IDictionary<string, string> args = new Dictionary<string, string>()
                            {
                               { "idUser", Convert.ToString(item["idRater"]) },
                               { "module", "general" },
                               { "module", null },
                            };

                            App.Current.MainPage = new NavigationPage(new Profile(args));
                        })
                    });

                    ratingDetailBlock.Children.Add(ratingNameUserBlock);

                    ratingDetailBlock.Children.Add(new Label
                    {
                        Text = (string)item["comment"],
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
                    });

                    Label goRating = new Label
                    {
                        Text = "[Saiba mais]",
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                        HorizontalOptions = LayoutOptions.Start
                    };

                    goRating.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(async () =>
                        {
                            await makeRating(Convert.ToInt32(item["idUserRating"]));
                        })
                    });

                    ratingDetailBlock.Children.Add(goRating);

                    ratingDataBlock.Children.Add(ratingDetailBlock);

                    pageContent.Children.Add(ratingDataBlock);

                    StackLayout ratingDivisor = new StackLayout
                    {
                        Margin = new Thickness(0, 5)
                    };

                    ratingDivisor.Children.Add(new BoxView
                    {
                        HeightRequest = 1,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.EndAndExpand,
                        Color = Color.FromHex("#e1e1e1")
                    });

                    pageContent.Children.Add(ratingDivisor);

                }

            }

            contentIndicator.IsVisible = false;

            return true;

        }

        public async Task<bool> makeActivity(int IdUser)
        {

            contentIndicator.IsVisible = true;

            pageContent.Children.Clear();

            string endpoint = "portalib-dev-profile";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idUser", IdUser }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "activity" },
                { "mod", "profile" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);


            int tt = 0;

            foreach (var item in res)
                tt++;

            if (tt > 0)
            {

                pageContent.Children.Add(new Label
                {
                    Text = string.Format("ATIVIDADES RECENTES >", 6),
                    Margin = new Thickness(0, 5),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                    Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.Start
                });

            }
            else
            {

                pageContent.Children.Add(new Label
                {
                    Text = "Nenhum atividade registrada.",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.StartAndExpand

                });

            }

            StackLayout blockActivity = new StackLayout
            {
                Orientation = StackOrientation.Vertical
            };

            if (tt > 0)
            {

                foreach (var item in res)
                {


                    StackLayout activityDataBlock = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal
                    };

                    CircleImage userActivity = new CircleImage
                    {
                        WidthRequest = 50,
                        HeightRequest = 50,
                        BorderColor = Color.LightGray,
                        BorderThickness = 1,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Start,
                        Aspect = Aspect.AspectFill,
                        Source = ImageRender.display("user", (string)item["imgAction"])
                    };

                    userActivity.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(() =>
                        {
                            IDictionary<string, string> args = new Dictionary<string, string>()
                        {
                           { "idUser", Convert.ToString(item["idUserAction"]) },
                           { "module", "general" },
                           { "idModule", null },
                        };

                            App.Current.MainPage = new NavigationPage(new Profile(args));
                        })
                    });

                    activityDataBlock.Children.Add(userActivity);

                    StackLayout activityDetailBlock = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        Padding = new Thickness(5, 0, 0, 0)
                    };

                    string txtActivity;

                    switch ((string)item["action"])
                    {
                        case "like":
                            txtActivity = string.Format("{0} curtiu a sua foto '{1}'.", (string)item["userAction"], (string)item["title"]);
                            break;
                        case "save":
                            txtActivity = string.Format("{0} adicionou a sua foto '{1}' em um de seus livros de idéias.", (string)item["userAction"], (string)item["title"]);
                            break;
                        case "rating":
                            txtActivity = string.Format("{0} enviou uma avaliação.", (string)item["userAction"]);
                            break;
                        case "follow":
                            txtActivity = string.Format("{0} começou a te seguir.", (string)item["userAction"]);
                            break;
                        case "message":
                            txtActivity = string.Format("{0} enviou uma mensagem.", (string)item["userAction"]);
                            break;
                        default:
                            txtActivity = "Atividade realizada.";
                            break;
                    }

                    activityDetailBlock.Children.Add(
                        new Label
                        {
                            Text = txtActivity,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold"
                        }
                    );

                    activityDetailBlock.Children.Add(
                        new Label
                        {
                            Text = Date.sinceAgo((string)item["dtRegister"], Convert.ToInt32(item["since"])),
                            FontSize = 12,
                            FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
                        }
                    );

                    switch ((string)item["action"])
                    {
                        case "like":

                            activityDetailBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                            {
                                Command = new Command(() =>
                                {
                                    App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(item["idAction"])));
                                })
                            });

                            break;
                        case "save":

                            activityDetailBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                            {
                                Command = new Command(() =>
                                {
                                    App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(item["idAction"])));
                                })
                            });

                            break;
                        case "rating":
                            activityDetailBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                            {
                                Command = new Command(async () =>
                                {
                                    await makeRating(Convert.ToInt32(item["idAction"]));
                                })
                            });
                            break;
                        case "message":

                            activityDetailBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                            {
                                Command = new Command(async () =>
                                {
                                    await makeMessage(Convert.ToInt32(item["idAction"]));
                                })
                            });
                            break;
                    }

                    activityDataBlock.Children.Add(activityDetailBlock);

                    pageContent.Children.Add(activityDataBlock);

                    StackLayout activityDivisor = new StackLayout
                    {
                        Margin = new Thickness(0, 5)
                    };

                    activityDivisor.Children.Add(new BoxView
                    {
                        HeightRequest = 1,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.EndAndExpand,
                        Color = Color.FromHex("#e1e1e1")
                    });

                    pageContent.Children.Add(activityDivisor);

                }

            }

            contentIndicator.IsVisible = false;

            return true;

        }

        public async Task<bool> makeMessageList(int IdUser)
        {

            contentIndicator.IsVisible = true;

            pageContent.Children.Clear();

            string endpoint = "portalib-dev-message";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idUser", IdUser }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "list" },
                { "mod", "message" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            int tt = 0;

            foreach (var item in res)
                tt++;

            if (tt > 0)
            {

                pageContent.Children.Add(new Label
                {
                    Text = string.Format("Mensagens >", 6),
                    Margin = new Thickness(0, 5),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                    Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.Start
                });

            }
            else
            {

                pageContent.Children.Add(new Label
                {
                    Text = "Nenhum atividade registrada.",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.StartAndExpand

                });

            }

            if (tt > 0)
            {

                foreach (var item in res)
                {

                    StackLayout messageDataBlock = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical
                    };


                    StackLayout messageDataTop = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical
                    };

                    messageDataTop.Children.Add(new Label
                    {
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-SemiboldItalic" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-SemiboldItalic.ttf#OpenSans-SemiboldItalic" : "Assets/Fonts/OpenSans-SemiboldItalic.ttf#OpenSans-SemiboldItalic",
                        Text = "Mensagem",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromHex("#4e5d3d")
                    });

                    messageDataTop.Children.Add(new Label
                    {
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-SemiboldItalic" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                        Text = Date.sinceAgo((string)item["dtRegister"], Convert.ToInt32(item["since"])),
                        FontSize = 10,
                        FontAttributes = FontAttributes.Italic
                    });

                    messageDataTop.Children.Add(new Label
                    {
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold",
                        Text = "Enviado por",
                        TextColor = Color.DarkGray
                    });

                    StackLayout messageDetail = new StackLayout
                    {
                        VerticalOptions = LayoutOptions.End,
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 0
                    };

                    messageDetail.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(async () =>
                        {
                            await makeMessage(Convert.ToInt32(item["idMessage"]));
                        })
                    });

                    messageDetail.Children.Add(new CircleImage
                    {
                        Source = ImageRender.display("user", (string)item["imgsender"]),
                        WidthRequest = 50,
                        HeightRequest = 50,
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.Fill,
                        Aspect = Aspect.AspectFill
                    });

                    StackLayout stackMessage = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        Padding = new Thickness(5, 0, 0, 0)
                    };

                    stackMessage.Children.Add(new Label
                    {
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold",
                        Text = (string)item["senderName"],
                        FontAttributes = FontAttributes.Bold
                    });

                    stackMessage.Children.Add(new Label
                    {
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-LightItalic" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-LightItalic.ttf#OpenSans-LightItalic" : "Assets/Fonts/OpenSans-LightItalic.ttf#OpenSans-LightItalic",
                        Text = (string)item["message"]
                    });

                    messageDetail.Children.Add(stackMessage);

                    messageDataTop.Children.Add(messageDetail);

                    messageDataBlock.Children.Add(messageDataTop);

                    StackLayout messageDataBottom = new StackLayout
                    {
                        VerticalOptions = LayoutOptions.End,
                        HorizontalOptions = LayoutOptions.End,
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 0
                    };

                    Button btAnswer = new Button
                    {
                        Text = "Responder",
                        TextColor = Color.White,
                        HorizontalOptions = LayoutOptions.End,
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                        FontSize = 12,
                        HeightRequest = 40,
                        WidthRequest = 100,
                        BackgroundColor = Color.FromHex("#667653")
                    };

                    btAnswer.Clicked += delegate
                    {
                        cleanClickMenu();
                        menuMessage.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                                Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold";
                        makeMessage(Convert.ToInt32(item["idMessage"]));

                    };

                    messageDataBottom.Children.Add(btAnswer);

                    Button btDelete = new Button
                    {
                        Text = "Deletar",
                        TextColor = Color.White,
                        HorizontalOptions = LayoutOptions.End,
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                        FontSize = 12,
                        HeightRequest = 40,
                        WidthRequest = 100,
                        BackgroundColor = Color.Black
                    };

                    messageDataBottom.Children.Add(btDelete);

                    Button btArquive = new Button
                    {
                        Text = "Arquivar",
                        TextColor = Color.White,
                        HorizontalOptions = LayoutOptions.End,
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                        FontSize = 12,
                        HeightRequest = 40,
                        WidthRequest = 100,
                        BackgroundColor = Color.Black
                    };

                    //messageDataBottom.Children.Add(btArquive);

                    messageDataBlock.Children.Add(messageDataBottom);

                    pageContent.Children.Add(messageDataBlock);

                    StackLayout ratingDivisor = new StackLayout
                    {
                        Margin = new Thickness(0, 5)
                    };

                    ratingDivisor.Children.Add(new BoxView
                    {
                        HeightRequest = 1,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.EndAndExpand,
                        Color = Color.FromHex("#e1e1e1")
                    });

                    pageContent.Children.Add(ratingDivisor);

                }

            }

            contentIndicator.IsVisible = false;

            return true;

        }

        public async Task<bool> makeProject(int idProject)
        {

            cleanClickMenu();
            menuProject.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold";

            contentIndicator.IsVisible = true;

            pageContent.Children.Clear();

            string endpoint = "portalib-dev-project";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idProject", idProject },
                { "idUser", Convert.ToInt32(Settings.config_user) }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "data" },
                { "mod", "project" }
            };

            projectId = idProject;

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            StackLayout dataProject = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Margin = new Thickness(10, 0)
            };

            dataProject.Children.Add(new Label
            {
                Text = (string)res["project"]["title"],
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Bold" :
                    Device.RuntimePlatform == Device.Android ? "Jura-Bold.ttf#Jura-Bold" : "Assets/Fonts/Jura-Bold.ttf#Jura-Bold"
            });

            dataProject.Children.Add(new Label
            {
                Text = (string)res["project"]["description"],
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
            });

            dataProject.Children.Add(new Label
            {
                Text = "Veja mais [+]",
                Margin = new Thickness(0, 10),
                HorizontalOptions = LayoutOptions.Start,
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
            });

            pageContent.Children.Add(dataProject);

            if (Convert.ToInt32(res["project"]["idUser"]) == Convert.ToInt32(Settings.config_user))
            {
                StackLayout stackNewPost = new StackLayout { };

                Button btNewPost = new Button
                {
                    Text = "+ Nova Foto",
                    HeightRequest = 50,
                    TextColor = Color.White,
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Regular" :
                                Device.RuntimePlatform == Device.Android ? "Jura-Regular.ttf#Jura-Regular" : "Assets/Fonts/Jura-Regular.ttf#Jura-Regular",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.End,
                    BackgroundColor = Color.FromHex("#667653")
                };

                btNewPost.Clicked += goToPostNew;

                stackNewPost.Children.Add(btNewPost);
                pageContent.Children.Add(stackNewPost);
            }

            int tt = 0;

            foreach (var item in res["posts"])
                tt++;

            if (tt == 0)
            {

                pageContent.Children.Add(new Label
                {
                    Text = "Nenhuma foto inserida no projeto.",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.StartAndExpand

                });

            }
            else
            {

                foreach (var item in res["posts"])
                {

                    StackLayout block = new StackLayout
                    {
                        HeightRequest = 350,
                        Margin = new Thickness(0, -10),
                        Spacing = 0
                    };

                    pageContent.Children.Add(block);

                    block.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(item["idPost"])));
                        })
                    });

                    RelativeLayout block_rel = new RelativeLayout();

                    block.Children.Add(block_rel);

                    Image projectImage = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 350,
                        Margin = new Thickness(15),
                        Source = ImageRender.display("post", (string)item["image"])
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
                        Margin = new Thickness(15),
                        BackgroundColor = Color.Black
                    };


                    block_rel.Children.Add(projectBox,
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
                            return parent.Height - 90;
                        }),

                        heightConstraint: Constraint.RelativeToParent((parent) =>
                        {
                            return parent.Height - (parent.Height - 90);
                        })
                    );

                    StackLayout projectData = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Margin = new Thickness(15, 0, 15, 0)
                    };

                    projectData.Children.Add(new Label
                    {
                        Text = (string)item["title"],
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        HorizontalTextAlignment = TextAlignment.Start,
                        FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Medium" :
                        Device.RuntimePlatform == Device.Android ? "Jura-Medium.ttf#Jura-Medium" : "Assets/Fonts/Jura-Medium.ttf#Jura-Medium",
                        FontSize = 16,
                        TextColor = Color.White,
                        Margin = new Thickness(10, 13, 0, 0)

                    });


                    Image bt_save = new Image
                    {
                        Source = "bt_save_int",
                        HeightRequest = 43,
                        HorizontalOptions = LayoutOptions.End,
                        Margin = new Thickness(2, 2, 2, 0)
                    };

                    bt_save.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            Navigation.PushModalAsync(new PostSave(Convert.ToInt32(item["idPost"]), "project", Convert.ToInt32(res["project"]["idUser"]), Convert.ToInt32(res["project"]["idProject"])));
                        })
                    });

                    projectData.Children.Add(bt_save);

                    Image bt_like = new Image
                    {
                        Source = "bt_like_int",
                        HeightRequest = 43,
                        HorizontalOptions = LayoutOptions.End,
                        Margin = new Thickness(2, 2, 2, 0),
                        IsVisible = (Convert.ToInt32(item["liked"]) == 0)
                    };

                    Image bt_like_active = new Image
                    {
                        Source = "bt_like_int_active",
                        HeightRequest = 43,
                        HorizontalOptions = LayoutOptions.End,
                        Margin = new Thickness(2, 2, 2, 0),
                        IsVisible = (Convert.ToInt32(item["liked"]) > 0)
                    };

                    bt_like.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            likePost(Convert.ToInt32(item["idPost"]), true, bt_like, bt_like_active);

                        })
                    });

                    projectData.Children.Add(bt_like);

                    bt_like_active.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            likePost(Convert.ToInt32(item["idPost"]), false, bt_like, bt_like_active);

                        })
                    });

                    projectData.Children.Add(bt_like_active);

                    Image bt_share = new Image
                    {
                        Source = "bt_share_post",
                        HeightRequest = 43,
                        HorizontalOptions = LayoutOptions.End,
                        Margin = new Thickness(0, 2, 15, 0)
                    };

                    bt_share.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(async () =>
                        {

                            await CrossShare.Current.Share(new Plugin.Share.Abstractions.ShareMessage()
                            {
                                Text = string.Format("Baixe o app Portal Decoração, e veja o projeto {0}, de {1}.", (string)res["project"]["title"], (string)res["project"]["userName"]),
                                Title = "Portal Decoração"
                            });
                        })
                    });

                    projectData.Children.Add(bt_share);

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
                            return parent.Height - 70;
                        })
                    );

                }
            }

            contentIndicator.IsVisible = false;

            return true;

        }

        public async Task<bool> makeBook(int idBook)
        {

            contentIndicator.IsVisible = true;

            pageContent.Children.Clear();

            string endpoint = "portalib-dev-book";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idBook", idBook }
            };
            
            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "data" },
                { "mod", "book" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);


            int tt = 0;

            foreach (var item in res["posts"])
                tt++;


            if (tt > 0)
            {

                pageContent.Children.Add(new Label
                {
                    Text = string.Format("{0} >", (string)res["book"]["title"]),
                    Margin = new Thickness(0, 5),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                    Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.Start
                });
            }
            else
            {

                pageContent.Children.Add(new Label
                {
                    Text = "Nenhuma foto salva neste livro de idéias.",
                    Margin = new Thickness(0, 5),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                    FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                    Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                    HorizontalOptions = LayoutOptions.Start
                });
            }

            //grid

            if (tt > 0)
            {

                var gridBook = new Grid();

                int col = 0;
                int row = 0;

                gridBook.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                gridBook.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                for (int i = 0; i < ((tt / 2) + ((tt % 2) > 0 ? 1 : 0)); i++)
                    gridBook.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                int count = 0;

                foreach (var item in res["posts"])
                {

                    StackLayout gridBookBlock = new StackLayout
                    {
                        HeightRequest = 150,
                        Spacing = 0
                    };

                    RelativeLayout block_rel = new RelativeLayout();

                    gridBookBlock.Children.Add(block_rel);

                    Image bookImage = new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 150,
                        Source = ImageRender.display("post", (string)item["image"])
                    };

                    block_rel.Children.Add(bookImage,
                        widthConstraint: Constraint.RelativeToParent((parent) => {
                            return parent.Width;
                        }),

                        heightConstraint: Constraint.RelativeToParent((parent) => {
                            return parent.Height;
                        })
                    );

                    gridBookBlock.GestureRecognizers.Add(new TapGestureRecognizer()
                    {
                        Command = new Command(() => {
                            App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(item["idPost"])));
                        })
                    });

                    gridBook.Children.Add(gridBookBlock, col, row);

                    if (((count + 1) % 2) == 0)
                    {
                        col = 0;
                        row++;
                    }
                    else
                    {
                        col = 1;
                    }

                    count++;

                }

                pageContent.Children.Add(gridBook);
            }

            contentIndicator.IsVisible = false;

            return true;

        }

        public async Task<bool> makeRating(int idRating)
        {

            contentIndicator.IsVisible = true;

            pageContent.Children.Clear();

            string endpoint = "portalib-dev-rating";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idUserRating", idRating }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "detail" },
                { "mod", "rating" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);


            pageContent.Children.Add(new Label
            {
                Text = string.Format("Avaliação para {0} >", (string)res["userName"]),
                Margin = new Thickness(0, 5),
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                    Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                HorizontalOptions = LayoutOptions.Start
            });

            StackLayout ratingDataBlock = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            CircleImage userRating = new CircleImage
            {
                WidthRequest = 50,
                HeightRequest = 50,
                BorderColor = Color.LightGray,
                BorderThickness = 1,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Start,
                Aspect = Aspect.AspectFill,
                Source = ImageRender.display("user", (string)res["imgsrater"])
            };

            userRating.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() => {
                    IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", (string)res["idRater"] },
                       { "module", "general" },
                       { "idModule", null },
                    };

                    App.Current.MainPage = new NavigationPage(new Profile(args));
                })
            });

            ratingDataBlock.Children.Add(userRating);

            StackLayout ratingDetailBlock = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(5, 0, 0, 0)
            };

            ratingDetailBlock.Children.Add(
                new Label
                {
                    Text = (string)res["raterName"],
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold"
                }
            );

            StackLayout ratingStartsBlock = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Padding = new Thickness(0, -3),
                Spacing = 0
            };

            string star;

            for (int i = 0; i < 5; i++)
            {
                star = "star";

                if ((float)res["rating"] >= (i + 1))
                    star = "star_dark";

                ratingStartsBlock.Children.Add(new Image
                {
                    Source = star,
                    HeightRequest = 24,
                    WidthRequest = 20,
                    Margin = new Thickness(3, 0)
                });

            }

            ratingDetailBlock.Children.Add(ratingStartsBlock);

            ratingDetailBlock.Children.Add(
                new Label
                {
                    Text = Date.sinceAgo((string)res["dtRegister"], Convert.ToInt32(res["since"])),
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Italic" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Italic.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Italic.ttf#OpenSans-Italic"
                }
            );

            ratingDetailBlock.Children.Add(new Label
            {
                Text = (string)res["comment"],
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Italic" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Italic.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Italic.ttf#OpenSans-Italic"
            });

            if (!string.IsNullOrEmpty((string)res["image"]))
            {

                ratingDetailBlock.Children.Add(new Image
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    WidthRequest = 200,
                    Aspect = Aspect.AspectFill,
                    Source = ImageRender.display("post", (string)res["image"])
                });

            }

            ratingDataBlock.Children.Add(ratingDetailBlock);

            pageContent.Children.Add(ratingDataBlock);

            contentIndicator.IsVisible = false;

            return true;

        }

        public async Task<bool> makeMessage(int idMessage)
        {

            contentIndicator.IsVisible = true;

            pageContent.Children.Clear();

            string endpoint = "portalib-dev-message";

            IDictionary<string, int> parameters = new Dictionary<string, int>()
            {
                { "idMessage", idMessage }
            };
            
            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "message" },
                { "mod", "message" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            StackLayout dataProject = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Margin = new Thickness(10, 0)
            };

            dataProject.Children.Add(new Label
            {
                Text = string.Format("Mensagem >", 6),
                Margin = new Thickness(0, 5),
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                FontSize = Device.RuntimePlatform == Device.iOS ? 18 :
                    Device.RuntimePlatform == Device.Android ? Device.GetNamedSize(NamedSize.Medium, this) : Device.GetNamedSize(NamedSize.Large, this),
                HorizontalOptions = LayoutOptions.Start
            });

            StackLayout messageDataTop = new StackLayout
            {
                Orientation = StackOrientation.Vertical
            };

            /*messageDataTop.Children.Add(new Label
            {
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-SemiboldItalic" :
                Device.RuntimePlatform == Device.Android ? "OpenSans-SemiboldItalic.ttf#OpenSans-SemiboldItalic" : "Assets/Fonts/OpenSans-SemiboldItalic.ttf#OpenSans-SemiboldItalic",
                Text = "...",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromHex("#4e5d3d")
            });*/

            messageDataTop.Children.Add(new Label
            {
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-SemiboldItalic" :
                Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                Text = Date.sinceAgo((string)res["dtRegister"], Convert.ToInt32(res["since"])),
                FontSize = 10,
                FontAttributes = FontAttributes.Italic
            });

            messageDataTop.Children.Add(new Label
            {
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold",
                Text = "Enviado por",
                TextColor = Color.DarkGray
            });

            StackLayout messageDetail = new StackLayout
            {
                VerticalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                Spacing = 0
            };

            messageDetail.Children.Add(new CircleImage
            {
                Source = ImageRender.display("user", (string)res["imgsender"]),
                WidthRequest = 50,
                HeightRequest = 50,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Fill,
                Aspect = Aspect.AspectFill
            });

            StackLayout stackMessage = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(5, 0, 0, 0)
            };

            stackMessage.Children.Add(new Label
            {
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold",
                Text = (string)res["senderName"],
                FontAttributes = FontAttributes.Bold
            });

            stackMessage.Children.Add(new Label
            {
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-LightItalic" :
                Device.RuntimePlatform == Device.Android ? "OpenSans-LightItalic.ttf#OpenSans-LightItalic" : "Assets/Fonts/OpenSans-LightItalic.ttf#OpenSans-LightItalic",
                Text = (string)res["message"]
            });

            messageDetail.Children.Add(stackMessage);

            messageDataTop.Children.Add(messageDetail);

            dataProject.Children.Add(messageDataTop);

            StackLayout messageDataBottom = new StackLayout
            {
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.End,
                Orientation = StackOrientation.Horizontal,
                Spacing = 0
            };

            Button btAnswer = new Button
            {
                Text = "Responder",
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.End,
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                FontSize = 12,
                HeightRequest = 40,
                WidthRequest = 100,
                BackgroundColor = Color.FromHex("#667653")
            };

            btAnswer.Clicked += delegate
            {
                goToMessage(Convert.ToInt32(res["idMessage"]));
            };

            messageDataBottom.Children.Add(btAnswer);

            Button btDelete = new Button
            {
                Text = "Deletar",
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.End,
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                FontSize = 12,
                HeightRequest = 40,
                WidthRequest = 100,
                BackgroundColor = Color.Black
            };

            messageDataBottom.Children.Add(btDelete);

            Button btArquive = new Button
            {
                Text = "Arquivar",
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.End,
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light",
                FontSize = 12,
                HeightRequest = 40,
                WidthRequest = 100,
                BackgroundColor = Color.Black
            };

            //messageDataBottom.Children.Add(btArquive);

            dataProject.Children.Add(messageDataBottom);

            pageContent.Children.Add(dataProject);

            contentIndicator.IsVisible = false;

            return true;

        }

        public void cleanClickMenu()
        {
            menuHome.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light";
            menuProject.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light";
            menuBook.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light";
            menuRating.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light";
            menuActivity.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light";
            menuMessage.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light";
        }

        public void goToExplorer()
        {
            App.Current.MainPage = new NavigationPage(new Explorer());
        }

        public async void goToHome(object idUser)
        {
            cleanClickMenu();
            menuHome.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold";

            await makeGeneralVision(Convert.ToInt32(idUser));
        }

        public async void goToBookList(object idUser)
        {
            cleanClickMenu();
            menuBook.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold";
            await makeBookList(Convert.ToInt32(idUser));
        }

        public async void goToRating(object idUser)
        {
            cleanClickMenu();
            menuRating.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold";
            await makeRatingList(Convert.ToInt32(idUser));
        }

        public async void goToProjectList(object idUser)
        {
            cleanClickMenu();
            menuProject.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold";
            await makeProjectList(Convert.ToInt32(idUser));
        }

        public async void goToProfile()
        {
            cleanClickMenu();
            menuHome.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold";

            await makeGeneralVision(userId);
        }

        public async void goToMessageList(object idUser)
        {
            cleanClickMenu();
            menuMessage.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold";
            await makeMessageList(Convert.ToInt32(idUser));
        }

        public async void goToMessage(int idMessage)
        {
            cleanClickMenu();
            menuMessage.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold";
            await makeMessage(idMessage);
        }

        public async void goToActivity(object idUser)
        {
            cleanClickMenu();
            menuActivity.FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Semibold" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Semibold.ttf#OpenSans-Semibold" : "Assets/Fonts/OpenSans-Semibold.ttf#OpenSans-Semibold";
            await makeActivity(Convert.ToInt32(idUser));
        }

        public void goToRatingNew(object i)
        {

            Navigation.PushModalAsync(new RatingNew(Convert.ToInt32(i)));
        }

        public void goToProjectNew(object sender, EventArgs e)
        {

            Navigation.PushModalAsync(new ProjectNewModal(userId));
        }

        public void goToBookNew(object sender, EventArgs e)
        {

            Navigation.PushModalAsync(new BookNew(userId));
        }

        public void goToPostNew(object sender, EventArgs e)
        {

            Navigation.PushModalAsync(new PostNew(projectId));
        }



    }
}