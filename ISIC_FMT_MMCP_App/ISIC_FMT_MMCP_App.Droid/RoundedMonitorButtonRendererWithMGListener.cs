using Android.Views;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using ISIC_FMT_MMCP_App;
using ISIC_FMT_MMCP_App.Droid;
using Isic.Debugger;

//[assembly: ExportRenderer(typeof(RoundedMonitorButton), typeof(RoundedMonitorButtonRenderer))]
namespace ISIC_FMT_MMCP_App.Droid
{
    public class RoundedMonitorButtonRendererWithMGListener : ButtonRenderer
    {
        //private readonly MonitorGestureListener myListener;
        //private readonly GestureDetector myDetector;

        public RoundedMonitorButtonRendererWithMGListener()
        {
            /*myListener = new MonitorGestureListener();
            myDetector = new GestureDetector(this.myListener);
            IsicDebug.DebugGeneral(string.Format("Listener: {0}", myListener.ToString()));*/

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            /*if (e.NewElement == null)
            {
                this.GenericMotion -= HandleGenericMotion;
                this.Touch -= HandleTouch;
            }

            if(e.OldElement == null)
            {
                this.GenericMotion += HandleGenericMotion;
                this.Touch += HandleTouch;
            }*/


        }



        /*void HandleTouch(object sender, TouchEventArgs e)
        {
            myDetector.OnTouchEvent(e.Event);
        }

        void HandleGenericMotion(object sender, GenericMotionEventArgs e)
        {
            myDetector.OnTouchEvent(e.Event);
        }*/
    }

    public class MonitorGestureListener : GestureDetector.SimpleOnGestureListener
    {

        public override void OnLongPress(MotionEvent e)
        {
            IsicDebug.DebugGeneral("On Long Prees on Monitor Button");
            base.OnLongPress(e);
        }

        public override bool OnDoubleTap(MotionEvent e)
        {
            IsicDebug.DebugGeneral("On Long Press on Monitor Button");
            return base.OnDoubleTap(e);
        }

        public override bool OnDoubleTapEvent(MotionEvent e)
        {
            IsicDebug.DebugGeneral("On Double Tap on Monitor Button");
            return base.OnDoubleTapEvent(e);
        }

        public override bool OnSingleTapUp(MotionEvent e)
        {
            IsicDebug.DebugGeneral("On Single Tap Up on Monitor Button");
            return base.OnSingleTapUp(e);
        }

        public override bool OnDown(MotionEvent e)
        {
            IsicDebug.DebugGeneral("On Down on Monitor Button");
            //return base.OnDown(e);
            return true;
        }

        public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            IsicDebug.DebugGeneral("On Fling on Monitor Button");
            return base.OnFling(e1, e2, velocityX, velocityY);
        }

        public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            IsicDebug.DebugGeneral("On Scroll on Monitor Button");
            return base.OnScroll(e1, e2, distanceX, distanceY);
        }

        public override void OnShowPress(MotionEvent e)
        {
            IsicDebug.DebugGeneral("On Show Press on Monitor Button");
            base.OnShowPress(e);
        }

        public override bool OnSingleTapConfirmed(MotionEvent e)
        {
            IsicDebug.DebugGeneral("On Single Tap Confirmed on Monitor Button");
            return base.OnSingleTapConfirmed(e);
        }
    }
}