using Android.Graphics.Drawables;
using Com.Bumptech.Glide.Request.Animation;
using Java.Lang;

namespace Com.Bumptech.Glide.Request.Target
{
    public partial class DrawableImageViewTarget
    {
        protected override void SetResource(Object p0)
        {
            SetResource((Drawable)p0);    
        }

        public override void OnResourceReady(Object p0, IGlideAnimation p1)
        {
            this.OnResourceReady();
        }
    }
}