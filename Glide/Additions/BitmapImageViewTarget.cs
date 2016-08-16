using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide.Request.Animation;
using Object = Java.Lang.Object;

namespace Com.Bumptech.Glide.Request.Target
{
    public partial class BitmapImageViewTarget
    {
        protected override void SetResource(Object p0)
        {
            this.SetResource((Bitmap)p0);
        }

        public override View View
        {
            get { return this.; }
        }


        public override void OnResourceReady(Object p0, IGlideAnimation p1)
        {
            throw new NotImplementedException();
        }
    }
}