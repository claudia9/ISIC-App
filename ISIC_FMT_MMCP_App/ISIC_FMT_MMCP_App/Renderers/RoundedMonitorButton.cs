using Xamarin.Forms;

namespace ISIC_FMT_MMCP_App
{
    public class RoundedMonitorButton : Button
    {
        public bool Checked { get; private set; }

        public RoundedMonitorButton()
        {
            Clicked += Button_Clicked;
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            Checked = true;
            BorderWidth = 5;
            BorderColor = Color.Green;
            TextColor = Color.Green;
            Clicked -= Button_Clicked;
            Clicked += Button_Unclicked;

        }

        private void Button_Unclicked(object sender, System.EventArgs e)
        {
            Checked = false;
            BorderWidth = 5;
            BorderColor = Color.White;
            TextColor = Color.White;
            Clicked -= Button_Unclicked;
            Clicked += Button_Clicked;
        }
    }
}
