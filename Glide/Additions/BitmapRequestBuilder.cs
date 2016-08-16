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

namespace Com.Bumptech.Glide
{
    public partial class BitmapRequestBuilder
    {
        GenericRequestBuilder IBitmapOptions.CenterCrop()
        {
            return this.CenterCrop();
        }

        GenericRequestBuilder IBitmapOptions.FitCenter()
        {
            return this.FitCenter();

        }
    }
}