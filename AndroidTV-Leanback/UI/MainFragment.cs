using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V17.Leanback.App;
using Android.Support.V17.Leanback.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request.Target;
using Object = Java.Lang.Object;

namespace AndroidExample.TVLeanback.UI
{
    /// <summary>
    /// Main class to show BrowseFragment with header and rows of videos
    /// </summary>
    public class MainFragment : BrowseFragment, LoaderManager.ILoaderCallbacks
    {
        private const int BackgroundUpdateDelay = 300;
        private const int CategoryLoader = 123; // Unique ID for Category Loader.
        private readonly Handler _handler = new Handler();
        private ArrayObjectAdapter _categoryRowAdapter;
        private Drawable _defaultBackground;
        private DisplayMetrics _metrics;
        private Timer _backgroundTimer;
        private Uri _backgroundUri;
        private BackgroundManager _backgroundManager;

        private Dictionary<int, CursorObjectAdapter> _videoCursorAdapters;

        public override void OnAttach(Context context)
        {
            // Create a list to contain all the CursorObjectAdapters.
            // Each adapter is used to render a specific row of videos in the MainFragment.
            _videoCursorAdapters = new Dictionary<int, CursorObjectAdapter>();

            // Map category results from the database to ListRow objects.
            // This Adapter is used to render the MainFragment sidebar labels.
            _categoryRowAdapter = new ArrayObjectAdapter(new ListRowPresenter());

            // Start loading the categories from the database.
            LoaderManager.InitLoader(CategoryLoader, null, this);
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            // Final initialization, modifying UI elements.
            base.OnActivityCreated(savedInstanceState);

            // Prepare the manager that maintains the same background image between activities
            PrepareBackgroundManager();

            SetupUIElements();
            SetupEventListeners();
            PrepareEntranceTransition();

            Adapter = _categoryRowAdapter;

            UpdateRecommendations();
        }

        public override void OnDestroy()
        {
            _backgroundTimer?.Dispose();
            _backgroundManager = null;

            base.OnDestroy();
        }

        public override void OnStop()
        {
            _backgroundManager.Release();
            base.OnStop();
        }


        private void SetupUIElements()
        {
            BadgeDrawable = Activity.Resources.GetDrawable(Resource.Drawable.videos_by_google_banner, null);
            Title = GetString(Resource.String.browse_title);
            HeadersState = HeadersEnabled;
            HeadersTransitionOnBackEnabled = true;

            // Set fastLane (or headers) background color
            BrandColor = Resources.GetColor(Resource.Color.fastlane_background, null);
            // Set search icon color.
            SearchAffordanceColor = Resources.GetColor(Resource.Color.search_opaque, null);

            SetHeaderPresenterSelector(new HeaderPresenterSelector()); 
        }

        private class HeaderPresenterSelector : PresenterSelector
        {
            public override Presenter GetPresenter(Object item)
            {
                return new IconHeaderItemPresenter();
            }
        }

        private void UpdateRecommendations()
        {
            throw new NotImplementedException();
        }

        private void SetupEventListeners()
        {
            var listener = new InnerEventListener(this);
            SetOnSearchClickedListener(listener);
            OnItemViewSelectedListener = listener;
            OnItemViewClickedListener = listener;
        }

        private class InnerEventListener : View.IOnClickListener,
            IOnItemViewClickedListener,
            IOnItemViewSelectedListener
        {
            private readonly MainFragment _mainFragment;

            public InnerEventListener(MainFragment mainFragment)
            {
                _mainFragment = mainFragment;
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void OnItemSelected(Presenter.ViewHolder itemViewHolder, Object item, RowPresenter.ViewHolder rowViewHolder, Row row)
            {
                throw new NotImplementedException();
            }

            public void OnItemClicked(Presenter.ViewHolder itemViewHolder, Object item, RowPresenter.ViewHolder rowViewHolder, Row row)
            {
                throw new NotImplementedException();
            }

            public IntPtr Handle { get; }
            public void OnClick(View v)
            {
                var intent = new Intent(_mainFragment.Activity, typeof(SearchActivity));
                _mainFragment.StartActivity(intent);
            }
        }

        private void PrepareBackgroundManager()
        {
            _backgroundManager = BackgroundManager.GetInstance(Activity);
            _backgroundManager.Attach(Activity.Window);
            _defaultBackground = Resources.GetDrawable(Resource.Drawable.default_background, null);
            _metrics = new DisplayMetrics();
            Activity.WindowManager.DefaultDisplay.GetMetrics(_metrics);
        }

        private void UpdateBackground(string uri)
        {
            int width = _metrics.WidthPixels;
            int height = _metrics.HeightPixels;
            Glide.With(this)
                .Load(uri).AsBitmap().CenterCrop().Error(_defaultBackground)
                .Into(new SimpleTarget())
                
       }

        class MySimpleTarget : SimpleTarget
        {
            public MySimpleTarget(int width, int height) : base(width, height)
            {
            }
        }
        public Loader OnCreateLoader(int id, Bundle args)
        {
            throw new NotImplementedException();
        }

        public void OnLoaderReset(Loader loader)
        {
            throw new NotImplementedException();
        }

        public void OnLoadFinished(Loader loader, Object data)
        {
            throw new NotImplementedException();
        }
    }
}