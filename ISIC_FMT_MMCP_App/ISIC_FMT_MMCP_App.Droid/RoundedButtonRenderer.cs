using Android.Graphics;
using Android.Graphics.Drawables;
using Isic.ViewModels;
using ISIC_FMT_MMCP_App;
using ISIC_FMT_MMCP_App.Droid;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(RoundedButton), typeof(RoundedButtonRenderer))]
namespace ISIC_FMT_MMCP_App.Droid
{
    public class RoundedButtonRenderer : ButtonRenderer
    {
        private GradientDrawable normal, clicked;

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.TextSize = 13;
                Control.SetTextColor(Android.Graphics.Color.White);
                Control.SetTypeface(Typeface.DefaultBold, TypefaceStyle.Bold);

                var button = e.NewElement;
                var mode = MeasureSpec.GetMode((int)button.BorderRadius);
                var borderRadius = 100;
                var borderWidth = 10;

                //New drawable for the button's normal state
                normal = new GradientDrawable();

                if(button.BackgroundColor.R == -1.0 && button.BackgroundColor.G == -1.0 && button.BackgroundColor.B == -1.0)
                {
                    normal.SetColor(Android.Graphics.Color.ParseColor("#444444"));
                } else
                {
                    normal.SetColor(button.BackgroundColor.ToAndroid());
                }

                normal.SetStroke((int)borderWidth, Android.Graphics.Color.ParseColor("#808080"));
                normal.SetCornerRadius(borderRadius);

                //New drawwable for the button's pressed state
                clicked = new GradientDrawable();

                clicked.SetColor(Android.Graphics.Color.ParseColor("#64B22E"));
                clicked.SetStroke(borderWidth, Android.Graphics.Color.ParseColor("#808080"));
                clicked.SetCornerRadius(borderRadius); 

                //Add the new drawables to a state list and assign it to the button
                var sld = new StateListDrawable();
                sld.AddState(new int[] { Android.Resource.Attribute.StatePressed }, clicked);
                sld.AddState(new int[] { }, normal);

                Control.SetBackground(sld);
            }

        }
    }
}