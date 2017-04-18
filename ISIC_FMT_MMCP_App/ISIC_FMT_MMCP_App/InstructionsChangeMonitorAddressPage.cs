
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public partial class InstructionsChangeMonitorAddressPage : Rg.Plugins.Popup.Pages.PopupPage
    {
    
        public InstructionsChangeMonitorAddressPage()
        {
            InitializeScreen();
        }

        private void InitializeScreen()
        {
            this.BackgroundColor = new Color(0, 0, 0, 0.4);

            Button btnClose = new Button() { Text = "Close this" };
            btnClose.Clicked += BtnClose_Clicked;
            Switch Not_Show = new Switch() { };
            Not_Show.Toggled += Not_Show_Toggled;
            if ((bool)Application.Current.Properties["Show_Instructions_MonAddr"] == false)
            {
                Not_Show.IsToggled = false;
            } else
            {
                Not_Show.IsToggled = true;
            }

            var list = new StackLayout()
            {

                Children =
                {
                    new Label
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        FontFamily = "Calibri",
                        FontAttributes = FontAttributes.Italic,
                        Text = "In order to change the address of the monitors and success on the process, read and follow carefully these instructions:"
                    },
                    new Label
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        FontFamily = "Calibri",
                        FontAttributes = FontAttributes.Italic,
                        Text = "1. Turn off all the monitors except the one requiring to have a different address."
                    },
                    new Label
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        FontFamily = "Calibri",
                        FontAttributes = FontAttributes.Italic,
                        Text = "2. Introduce the new address using the Address Box located in the page below."
                    },
                    new Label
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        FontFamily = "Calibri",
                        FontAttributes = FontAttributes.Italic,
                        Text = "3. Click the Save button located in the page below. (Make sure that only the monitor needing to have a new address is on)"
                    },
                    new Label
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        FontFamily = "Calibri",
                        FontAttributes = FontAttributes.Italic,
                        Text = "4. Check that the monitor turns off and on again. - If so, the address has been changed successfuly"
                    },
                    new Label
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        FontFamily = "Calibri",
                        FontAttributes = FontAttributes.Italic,
                        Text = "5. In order to change the address to another monitor, repeat the process starting from the instruction 1."
                    }
                }
            };

            Content = new StackLayout()
            {
                BackgroundColor = Color.FromHex("#F5F4F1"),
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(35, 30, 35, 30),
                Padding = new Thickness(10),

                Children = {
                    new Label()
                    {
                        Text = "Welcome to this tutorial!",
                        FontSize = 18,
                        FontFamily = "Calibri",
                        TextColor = Color.FromHex("#444444"),
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new Label()
                    {
                        Text = "Here you will learn how to change the address of the ISIC Monitors",
                        FontSize = 14,
                        FontFamily = "Calibri",
                        TextColor = Color.FromHex("#444444"),
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new ScrollView()
                    {
                        HeightRequest = 250,
                        Padding = new Thickness(10),
                        BackgroundColor = Color.FromHex("#D2D2D2"),
                        Content = list
                    },
                    new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            new Label()
                            {
                                LineBreakMode = LineBreakMode.WordWrap,
                                FontFamily = "Calibri",
                                FontAttributes = FontAttributes.Italic,
                                Text = "If you do not want this window to open again the next time, press the button on the right."
                            },
                            Not_Show
                        }
                    },
                    new Label()
                    {
                        HorizontalTextAlignment  = TextAlignment.Center,
                        FontFamily = "Calibri",
                        FontAttributes = FontAttributes.Italic,
                        Text = "(You can always read again the instructions using the button located on the right-top of the screen)"
                    },
                    btnClose,
                }
            };


        }

        public void Not_Show_Toggled(object sender, ToggledEventArgs e)
        {
            if ((bool)Application.Current.Properties["Show_Instructions_MonAddr"] == false)
            {
                Application.Current.Properties["Show_Instructions_MonAddr"] = true;
            }
            else
            {
                Application.Current.Properties["Show_Instructions_MonAddr"] = false;
            }
        }

        private void BtnClose_Clicked(object sender, System.EventArgs e)
        {
            PopupNavigation.PopAsync();
        }


    }
}
