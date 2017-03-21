using Android.Views;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using ISIC_FMT_MMCP_App;
using ISIC_FMT_MMCP_App.Droid;
using Isic.Debugger;

[assembly: ExportRenderer(typeof(RoundedMonitorButton), typeof(RoundedMonitorButtonRenderer))]
namespace ISIC_FMT_MMCP_App.Droid
{
    public class RoundedMonitorButtonRenderer : ButtonRenderer
    {

        public RoundedMonitorButtonRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

             
            if (Control != null)
            {
                Control.LongClick += Control_LongClick;
            }

        }

        private void Control_LongClick(object sender, LongClickEventArgs e)
        {
            IsicDebug.DebugGeneral("HELLO CLAUDIA, you are on Long Click");

        }


    }

}