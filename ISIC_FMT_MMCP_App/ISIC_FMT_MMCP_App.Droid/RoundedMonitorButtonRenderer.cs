using Android.Views;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using ISIC_FMT_MMCP_App;
using ISIC_FMT_MMCP_App.Droid;
using Isic.Debugger;
using Android.Graphics.Drawables;
using Android.Graphics;
using System.Collections.Generic;
using System;

[assembly: ExportRenderer(typeof(RoundedMonitorButton), typeof(RoundedMonitorButtonRenderer))]
namespace ISIC_FMT_MMCP_App.Droid
{
    public class RoundedMonitorButtonRenderer : ButtonRenderer
    {
        private GradientDrawable normal, clicked, setted;
        private StateListDrawable sld1, sld2;

        private RoundedMonitorButton currentButton;

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            currentButton = (RoundedMonitorButton)e.NewElement;

            if (Control != null)
            {
                Control.TextSize = 13;
                Control.SetTypeface(Typeface.DefaultBold, TypefaceStyle.Bold);

                //CNC@ISIC 29/03/17 -> Added this condition in order to fix error with Disposed object ButtonGroup.Instance when poping activity and pushing again.
                if (ButtonGroup.Instance.Count == 4) {
                    ButtonGroup.Instance.Clear();
                }
                ButtonGroup.Instance.Add(Control);

                var button = e.NewElement;
                var borderRadius = 100;
                var borderWidth = 6;

                normal = new GradientDrawable();

                normal.SetColor(Android.Graphics.Color.ParseColor("#444444"));
                normal.SetStroke((int)borderWidth, Android.Graphics.Color.ParseColor("#808080"));
                normal.SetCornerRadius(borderRadius);

                clicked = new GradientDrawable();

                clicked.SetColor(Android.Graphics.Color.ParseColor("#444444"));
                clicked.SetStroke((int)borderWidth, Android.Graphics.Color.ParseColor("#808080"));
                clicked.SetCornerRadius(borderRadius);

                setted = new GradientDrawable();

                setted.SetColor(Android.Graphics.Color.ParseColor("#444444"));
                setted.SetStroke((int)borderWidth, Android.Graphics.Color.ParseColor("#64B22E"));
                setted.SetCornerRadius(borderRadius);


                //Add the new drawables to a state list and assign it to the button
                sld1 = new StateListDrawable();
                sld1.AddState(new int[] { Android.Resource.Attribute.StatePressed }, clicked);
                sld1.AddState(new int[] { }, normal);

                sld2 = new StateListDrawable();
                sld2.AddState(new int[] { Android.Resource.Attribute.StatePressed }, clicked);
                sld2.AddState(new int[] { }, setted);

                Control.SetTextColor(Android.Graphics.Color.White);
                Control.SetBackground(sld1);

                currentButton.Clicked += Control_Click;

                Control.LongClick += Control_LongClick;

                //Control.TextChanged += Control_Click;
            }

        }


        private void Control_Click(object sender, System.EventArgs e)
        {
            bool isChecked = currentButton.IsClicked;
            IsicDebug.DebugGeneral("IsChecked in Button renderer: " + isChecked);

            if (isChecked == true)
            {
                ButtonGroup.Instance.updateButtonGraphic(Control, sld1, sld2);
            }

        }

        private void Control_LongClick(object sender, LongClickEventArgs e)
        {
            IsicDebug.DebugGeneral("HELLO CLAUDIA, you are on Long Click");
        }


    }

    public class ButtonGroup : List<Android.Widget.Button>
    {
        private static ButtonGroup _instance;
        private ButtonGroup()
        {
        }
        public static ButtonGroup Instance
        {
            //NEED TO FIX THE DISPOSE PROBLEM!!!! ITEM IS DISPOSED WHEN POPING THE REMOTE CONTROL PAGE AND WHEN OPENNING THE PAGE AGAIN AND CLICK A MONITOR BUTTON, IT THROWS AN EXCEPTION!
            get
            {
                if (_instance == null)
                {
                    _instance = new ButtonGroup();
                }
                return _instance;
            }

            set
            {
               _instance = value;
            }
        }

        public void updateButtonGraphic(Android.Widget.Button button, StateListDrawable sld1, StateListDrawable sld2)
        {
            try
            {
                foreach (var item in this)
                {

                    if (button == item)
                    {
                        item.SetTextColor(Android.Graphics.Color.ParseColor("#64B22E"));
                        item.SetBackground(sld2);
                    }
                    else
                    {
                        item.SetTextColor(Android.Graphics.Color.White);
                        item.SetBackground(sld1);
                    }
                }
            }
            catch (Exception e)
            {
                IsicDebug.DebugException(String.Format("Item is null, it cannot be called - {0}", e));
            }


        }
    }

}