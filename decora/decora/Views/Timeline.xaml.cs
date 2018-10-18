using ImageCircle.Forms.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using decora.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;

namespace decora.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Timeline : ContentPage
    {
        private int counterPost { get; set; }
        private bool actionScroll { get; set; }
        private bool executing { get; set; }
        private int pageCounter { get; set; }

        public Timeline ()
        {

            App.lp.page = "timeline";
            App.lp.page_id = null;
            App.lp.module = null;
            App.lp.module_id = null;

            counterPost = 0;
            actionScroll = false;
            executing = false;
            pageCounter = 1;
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

                    await makeTimeline();

                    string screen = Settings.config_screen;

                    double s = Convert.ToDouble(screen);

                    MainRelative.HeightRequest = s;

                }
            }
            else
            {
                App.Current.MainPage = new NavigationPage(new FailConnection());
            }

            base.OnAppearing();

        }

        private void makeMenu()
        {


            StackLayout logo = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HeightRequest = 55,
                Spacing = 0
            };

            Image imgLogo = new Image
            {
                Source = "logo_branco",
                HeightRequest = 40,
                WidthRequest = 94,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Margin = new Thickness(-5)
            };

            logo.Children.Add(imgLogo);

            MainRelative.Children.Add(logo,

                    xConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.X;
                    }),

                    yConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Y;
                    }),

                    widthConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Width;
                    }));

            StackLayout menuBar = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HeightRequest = 55
            };

            StackLayout menu = new StackLayout
            {
                Padding = new Thickness(15, 10),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.StartAndExpand
            };

            Image imgMenu = new Image
            {
                Source = "menu_white",
                HeightRequest = 35
            };

            imgMenu.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(openMenu)
            });

            menu.Children.Add(imgMenu);

            menuBar.Children.Add(menu);

            StackLayout explorer = new StackLayout
            {
                Padding = new Thickness(5, 15),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            Image imgExplorer = new Image
            {
                Source = "lupa",
                HeightRequest = 30
            };

            imgExplorer.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(goToExplorer)
            });

            explorer.Children.Add(imgExplorer);

            menuBar.Children.Add(explorer);

            string id = Settings.config_user;
            string img = Settings.config_image;

            StackLayout avatar = new StackLayout
            {
                Padding = new Thickness(5, 10, 10, 10),
                Spacing = 0,
                HorizontalOptions = LayoutOptions.End
            };

            CircleImage imgAvatar = new CircleImage
            {
                Source = ImageRender.display("user", img),
                HeightRequest = 40,
                Aspect = Aspect.AspectFill,
                WidthRequest = 40,
                BorderColor = Color.White,
                BorderThickness = 1
            };

            imgAvatar.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() => {
                    IDictionary<string, string> args = new Dictionary<string, string>()
                    {
                       { "idUser", id },
                       { "module", "general" },
                       { "idModule", null },
                    };
                    App.Current.MainPage = new NavigationPage(new Profile(args));
                })
            });

            avatar.Children.Add(imgAvatar);

            menuBar.Children.Add(avatar);

            MainRelative.Children.Add(menuBar,

                    xConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.X;
                    }),

                    yConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Y;
                    }),

                    widthConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Width;
                    }));

        }

        public async Task makeTimeline(bool Load = false)
        {
            executing = true;
            string endpoint = "portalib-dev-general";

            int id = Convert.ToInt32(Settings.config_user);

            IDictionary<string, int> parameters;

            if (Load)
                pageCounter++;

            /*
            parameters = new Dictionary<string, int>()
                {
                   { "idUser", id },
                   { "getMoreData", 1 }
                };
        }
        else
        {

            parameters = new Dictionary<string, int>()
                {
                    { "idUser", id },
                    { "ignoreClear", 1 },
                    { "getMoreData", 1 }
                };
        }*/

            parameters = new Dictionary<string, int>()
            {
                { "idUser", id },
                { "page", pageCounter }
            };

            IDictionary<string, string> call = new Dictionary<string, string>
            {
                { "act", "timeline" },
                { "mod", "general" }
            };

            dynamic res = await decora.Service.Run(endpoint, call, "GET", parameters);

            if (!Load)
                makeMainPost(res);

            makePost(res, !Load);

        }

        private void makeMainPost(dynamic obj)
        {

            try
            {

                Image imgBg = new Image
                {
                    Aspect = Aspect.AspectFill,
                    Source = ImageRender.display("post", (string)obj["posts"][0]["imgPost"])
                };

                MainRelative.Children.Add(imgBg,
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
                    BackgroundColor = Color.Black
                };

                MainRelative.Children.Add(boxlight,
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

                Label title = new Label
                {
                    Text = "SUA MELHOR REFERÊNCIA EM ARQUITETURA, DECORAÇÃO, PAISAGISMO E DESIGN",
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Bold" :
                        Device.RuntimePlatform == Device.Android ? "Jura-Bold.ttf#Jura-Bold" : "Assets/Fonts/Jura-Medium.ttf#Jura-Bold",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.White,
                    FontSize = 37
                };

                MainRelative.Children.Add(title,

                    xConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.X + 15;
                    }),

                    yConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Y;
                    }),

                    widthConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Width - 30;
                    }),

                    heightConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Height - 20;
                    })
                );

                StackLayout projectBlock = new StackLayout
                {
                    Margin = new Thickness(15, 0),
                    Orientation = StackOrientation.Horizontal
                };

                Label titleProject = new Label
                {
                    Text = string.Format("Projeto de {0}", (string)obj["posts"][0]["userName"]),
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.EndAndExpand,
                    HorizontalTextAlignment = TextAlignment.End,
                    Margin = new Thickness(0, 13, 3, 0),
                    TextColor = Color.White,
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Italic" :
                        Device.RuntimePlatform == Device.Android ? "OpenSans-Italic.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Italic.ttf#OpenSans-Italic"
                };

                titleProject.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(obj["posts"][0]["idPost"])));
                    })
                });

                projectBlock.Children.Add(titleProject);

                CircleImage imgProject = new CircleImage
                {
                    Source = ImageRender.display("user", (string)obj["posts"][0]["imgUser"]),
                    BorderColor = Color.White,
                    HeightRequest = 50,
                    WidthRequest = 50,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center,
                    BorderThickness = 1,
                    Aspect = Aspect.AspectFill

                };

                imgProject.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(obj["posts"][0]["idPost"])));
                    })
                });

                projectBlock.Children.Add(imgProject);

                MainRelative.Children.Add(projectBlock,

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
            catch
            {

                Image imgBg = new Image
                {
                    Aspect = Aspect.AspectFill,
                    Source = ImageRender.display("post", "img7.png")
                };

                MainRelative.Children.Add(imgBg,
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
                    Opacity = 0.8,
                    BackgroundColor = Color.Black
                };

                MainRelative.Children.Add(boxlight,
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

                StackLayout boxText = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };

                Label title = new Label
                {
                    Text = "Bem-vindo! Explore e siga os profissionais para preencher a sua timeline.",
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Bold" :
                        Device.RuntimePlatform == Device.Android ? "Jura-Bold.ttf#Jura-Bold" : "Assets/Fonts/Jura-Medium.ttf#Jura-Bold",
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.White,
                    FontSize = 24
                };

                Button btExplorer = new Button
                {
                    Text = "EXPLORAR",
                    TextColor = Color.White,
                    BorderColor = Color.White,
                    FontAttributes = FontAttributes.Bold,
                    BorderWidth = 3,
                    BackgroundColor = Color.Transparent,
                    FontSize = 21,
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "Jura-Bold" :
                        Device.RuntimePlatform == Device.Android ? "Jura-Bold.ttf#Jura-Bold" : "Assets/Fonts/Jura-Medium.ttf#Jura-Bold",
                    Margin = new Thickness(20, 0, 0, 0)
                };

                btExplorer.Clicked += delegate
                {
                    goToExplorer();
                };

                boxText.Children.Add(title);
                boxText.Children.Add(btExplorer);

                MainRelative.Children.Add(boxText,

                    xConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.X + 15;
                    }),

                    yConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Y;
                    }),

                    widthConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Width - 30;
                    }),

                    heightConstraint: Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Height - 20;
                    })
                );

            }

            makeMenu();

        }

        private void makePost(dynamic obj, bool main)
        {
            bool validate;
            int c = 0;

            try
            {
                var post = obj["posts"][(main ? 1 : 0)];
                validate = true;
                if (!actionScroll)
                    scrollTimeline.Scrolled += checkScroll;
            }
            catch
            {
                validate = false;
            }

            if (validate)
            {
                foreach (var item in obj["posts"])
                {
                    if (c > (!main ? -1 : 0))
                    {
                        /*
                        if (counterPost % 10 == 0)
                            makeAds();

                        if (counterPost % 8 == 0)
                            makeCarrousel(obj);
                        */
                        StackLayout postBlock = new StackLayout
                        {
                            HeightRequest = 350,

                        };

                        RelativeLayout rel = new RelativeLayout();

                        postBlock.Children.Add(rel);

                        Image imgPost = new Image
                        {
                            Aspect = Aspect.AspectFill,
                            HeightRequest = 350,
                            Source = ImageRender.display("post", (string)item["imgPost"]),
                            Margin = new Thickness(15)
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
                            Margin = new Thickness(15),
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
                                return parent.Height - 90;
                            })
                        );

                        StackLayout dataPost = new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            Margin = new Thickness(15, 0)
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
                                return parent.Height - 70;
                            })
                        );

                        postBlock.GestureRecognizers.Add(new TapGestureRecognizer
                        {
                            Command = new Command(() =>
                            {
                                App.Current.MainPage = new NavigationPage(new Post(Convert.ToInt32(item["idPost"])));
                            })
                        });

                        MainContent.Children.Add(postBlock);

                        makeDivisor();

                        counterPost++;

                    }

                    c++;
                }
            }

            executing = false;

        }

        private void makeAds()
        {

            StackLayout adsBlock = new StackLayout
            {
                HeightRequest = 190,
                Spacing = 0,
                Margin = new Thickness(15, 10, 15, 10)
            };

            RelativeLayout rel = new RelativeLayout();

            adsBlock.Children.Add(rel);

            StackLayout adsContent = new StackLayout
            {
                Orientation = StackOrientation.Vertical
            };

            Image adsImage = new Image
            {
                Aspect = Aspect.AspectFill,
                Source = "publicidade2"
            };

            adsContent.Children.Add(adsImage);

            rel.Children.Add(adsContent,
                widthConstraint: Constraint.RelativeToParent((parent) =>
                {
                    return parent.Width;
                }),

                heightConstraint: Constraint.RelativeToParent((parent) =>
                {
                    return parent.Height;
                })
            );

            MainContent.Children.Add(adsBlock);

        }

        private void makeCarrousel(dynamic obj)
        {
            Random rnd = new Random();
            int rand = rnd.Next(10, 100);

            if (rand % 2 == 0)
            {
                makeProfessionals(obj);
            }
            else
            {
                makeCategories(obj);
            }

        }

        private void makeProfessionals(dynamic obj)
        {
            StackLayout professionals = new StackLayout();

            professionals.Children.Add(new Label
            {
                Text = "Busque por profissionais",
                FontSize = 21,
                Margin = new Thickness(10, 0, 0, 0),
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
            });

            StackLayout professionalBlock = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            foreach (var item in obj["users"])
            {
                StackLayout dataProfessional = new StackLayout
                {
                    HeightRequest = 250,
                };

                professionalBlock.Children.Add(dataProfessional);

                RelativeLayout block_rel = new RelativeLayout();

                block_rel.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => {
                        IDictionary<string, string> args = new Dictionary<string, string>()
                        {
                           { "idUser", Convert.ToString(item["idUser"]) },
                           { "module", "general" },
                           { "idModule", null },
                        };
                        App.Current.MainPage = new NavigationPage(new Profile(args));
                    })
                });

                dataProfessional.Children.Add(block_rel);

                Image professionalImage = new Image
                {
                    Aspect = Aspect.AspectFill,
                    HeightRequest = 250,
                    WidthRequest = 150,
                    Source = ImageRender.display("cover", (string)item["cover"])
                };

                block_rel.Children.Add(professionalImage,
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

                StackLayout professionalData = new StackLayout
                {
                    Orientation = StackOrientation.Vertical
                };

                professionalData.Children.Add(new CircleImage
                {
                    WidthRequest = 60,
                    HeightRequest = 60,
                    BorderThickness = 1,
                    VerticalOptions = LayoutOptions.End,
                    HorizontalOptions = LayoutOptions.Center,
                    BorderColor = Color.White,
                    Aspect = Aspect.AspectFill,
                    Source = ImageRender.display("user", (string)item["image"])
                });

                professionalData.Children.Add(new Label
                {
                    Text = (string)item["userName"],
                    WidthRequest = 150,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.End,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                    FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-CondBold" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-CondBold.ttf#OpenSans-CondBold" : "Assets/Fonts/OpenSans-CondBold.ttf#OpenSans-CondBold",
                    FontSize = 16,
                    TextColor = Color.White

                });

                block_rel.Children.Add(professionalData,
                    xConstraint: Constraint.RelativeToParent((parent) => {
                        return parent.X;
                    }),

                    yConstraint: Constraint.RelativeToParent((parent) => {
                        return parent.Height - 120;
                    })
                );

            }

            ScrollView baseProfessionals = new ScrollView
            {
                Margin = new Thickness(10),
                Orientation = ScrollOrientation.Horizontal,
                Content = professionalBlock
            };

            professionals.Children.Add(baseProfessionals);

            MainContent.Children.Add(professionals);

        }

        private void makeCategories(dynamic obj)
        {
            StackLayout Categories = new StackLayout();

            Categories.Children.Add(new Label
            {
                Text = "PROJETO POR CATEGORIA",
                FontSize = 21,
                Margin = new Thickness(10, 0, 0, 0),
                FontFamily = Device.RuntimePlatform == Device.iOS ? "OpenSans-Light" :
                    Device.RuntimePlatform == Device.Android ? "OpenSans-Light.ttf#OpenSans-Light" : "Assets/Fonts/OpenSans-Light.ttf#OpenSans-Light"
            });

            StackLayout categoryBlock = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            foreach (var item in obj["categories"])
            {
                StackLayout dataCategory = new StackLayout
                {
                    HeightRequest = 150,
                };

                categoryBlock.Children.Add(dataCategory);

                dataCategory.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => {
                        App.Current.MainPage = new NavigationPage(new Category(Convert.ToInt32((string)item["idCategory"])));
                    })
                });

                RelativeLayout block_rel = new RelativeLayout();

                dataCategory.Children.Add(block_rel);

                Image categoryImage = new Image
                {
                    Aspect = Aspect.AspectFill,
                    HeightRequest = 150,
                    WidthRequest = 150,
                    Source = ImageRender.display("category", (string)item["image"])
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

                StackLayout categoryData = new StackLayout
                {
                    Orientation = StackOrientation.Vertical
                };

                categoryData.Children.Add(new Label
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

                block_rel.Children.Add(categoryData,
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

            ScrollView baseCategories = new ScrollView
            {
                Margin = new Thickness(10),
                Orientation = ScrollOrientation.Horizontal,
                Content = categoryBlock
            };

            Categories.Children.Add(baseCategories);

            MainContent.Children.Add(Categories);

        }

        private void makeDivisor()
        {

            StackLayout divisorBlock = new StackLayout();
            divisorBlock.Children.Add(new BoxView
            {
                HeightRequest = 15,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand,
                Color = Color.LightGray,
                Margin = new Thickness(0, 0, 0, 10)
            });

            MainContent.Children.Add(divisorBlock);
        }

        public void openMenu()
        {
            Navigation.PushModalAsync(new Menu());
        }

        private void checkScroll(object sender, ScrolledEventArgs e)
        {
            actionScroll = true;

            double scrollingSpace = scrollTimeline.ContentSize.Height - scrollTimeline.ScrollY - 60;

            if ((scrollingSpace < scrollTimeline.Height) && !executing)
                makeTimeline(true);

        }

        public void goToExplorer()
        {
            App.Current.MainPage = new NavigationPage(new Explorer());
        }

    }
}