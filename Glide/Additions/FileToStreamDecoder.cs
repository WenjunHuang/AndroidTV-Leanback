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
using Com.Bumptech.Glide.Load.Engine;
using Object = Java.Lang.Object;

namespace Com.Bumptech.Glide.Load.Resource.File
{
    public partial class FileToStreamDecoder : IResourceDecoder
    {
        IResource IResourceDecoder.Decode(Object p0, int p1, int p2)
        {
            return Decode((Java.IO.File)p0, p1, p2);
        }
    }
}