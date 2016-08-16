using Android.App;
using Android.OS;

namespace AndroidExample.TVLeanback.UI
{
    [Activity(Label = "AndroidTV_Leanback", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
        }
    }
}

