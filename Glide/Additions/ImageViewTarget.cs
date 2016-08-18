using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Com.Bumptech.Glide.Request.Target
{
    public abstract partial class ImageViewTarget
    {
        public new View View => (View)base.View;
    }
}