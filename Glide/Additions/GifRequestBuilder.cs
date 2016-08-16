using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace Com.Bumptech.Glide
{
    public partial class GifRequestBuilder
    {
        GenericRequestBuilder IBitmapOptions.CenterCrop()
        {
            return this.CenterCrop();
        }

        GenericRequestBuilder IBitmapOptions.FitCenter()
        {
            return this.CenterCrop();
        }

        GenericRequestBuilder IDrawableOptions.CrossFade()
        {
            return this.CrossFade();
        }

        GenericRequestBuilder IDrawableOptions.CrossFade(Animation p0, int p1)
        {
            return this.CrossFade(p0, p1);
        }

        GenericRequestBuilder IDrawableOptions.CrossFade(int p0)
        {
            return this.CrossFade(p0);
        }

        GenericRequestBuilder IDrawableOptions.CrossFade(int p0, int p1)
        {
            return this.CrossFade(p0, p1);
        }
    }
}