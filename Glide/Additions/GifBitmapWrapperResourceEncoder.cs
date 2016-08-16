using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide.Load.Engine;
using Object = Java.Lang.Object;

namespace Com.Bumptech.Glide.Load.Resource.Gifbitmap
{
    public partial class GifBitmapWrapperResourceEncoder : IResourceEncoder
    {
        bool IEncoder.Encode(Object p0, Stream p1)
        {
            return Encode((IResource)p0, p1);
        }

        public bool Encode()
        {
            throw new NotImplementedException();
        }
    }
}