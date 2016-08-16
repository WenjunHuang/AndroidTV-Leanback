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
using Com.Bumptech.Glide.Request.Animation;
using Object = Java.Lang.Object;

namespace Com.Bumptech.Glide.Request.Target
{
    public abstract partial class BaseTarget
    {
        public abstract void OnResourceReady(Object p0, IGlideAnimation p1);
    }
}