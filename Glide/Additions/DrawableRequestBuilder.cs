using Android.Views.Animations;

namespace Com.Bumptech.Glide
{
    public  partial class DrawableRequestBuilder
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