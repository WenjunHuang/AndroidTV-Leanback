using Android.Graphics;
using Com.Bumptech.Glide.Request.Animation;
using Java.Lang;

namespace Com.Bumptech.Glide.Request.Target
{
    public partial class NotificationTarget
    {
        public override void OnResourceReady(Object p0, IGlideAnimation p1)
        {
           OnResourceReady((Bitmap)p0, p1); 
        }
    }
}