using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V17.Leanback.App;
using Android.Util;
using Android.Views;
using Android.Widget;

using static AndroidExample.TVLeanback.Util.AndroidListenerHelper;

namespace AndroidExample.TVLeanback.UI
{
    /// <summary>
    /// This class demonstrates how to extend ErrorFragment to create an error dialog
    /// </summary>
    public class BrowseErrorFragment : ErrorFragment
    {
        const bool Translucent = true;
        const int TimerDelay = 1000;
        private readonly Handler _handler = new Handler();
        private SpinnerFragment _spinnerFragment;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Title = "Videos by Google";

            _spinnerFragment = new SpinnerFragment();
            FragmentManager.BeginTransaction()
                .Add(Resource.Id.main_frame, _spinnerFragment)
                .Commit();

            // Create your fragment here
        }

        public override void OnStart()
        {
            base.OnStart();
            _handler.PostDelayed(() =>
            {
                FragmentManager.BeginTransaction()
                    .Remove(_spinnerFragment)
                    .Commit();
                SetErrorContent();
            }, TimerDelay);
        }

        public override void OnStop()
        {
            base.OnStop();
            _handler.RemoveCallbacksAndMessages(null);
            FragmentManager.BeginTransaction()
                .Remove(_spinnerFragment)
                .Commit();
        }

        private void SetErrorContent()
        {
            ImageDrawable = Resources.GetDrawable(Resource.Drawable.lb_ic_sad_cloud, null);
            Message = "An error occurred";
            SetDefaultBackground(Translucent);

            ButtonText = "Dismiss";
            ButtonClickListener = ViewOnClick((view) =>
            {
                FragmentManager.BeginTransaction()
                    .Remove(this)
                    .Commit();
                FragmentManager.PopBackStack();
            });
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }

    internal class SpinnerFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var progressBar = new ProgressBar(container.Context);
            if (container is FrameLayout)
            {
                int width = Resources.GetDimensionPixelSize(Resource.Dimension.spinner_width);
                int height = Resources.GetDimensionPixelSize(Resource.Dimension.spinner_height);
                var layoutParams = new FrameLayout.LayoutParams(width, height);
                progressBar.LayoutParameters = layoutParams;
            }
            return progressBar;
        }
    }
}