using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V17.Leanback.App;
using Android.Support.V17.Leanback.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request.Animation;
using Com.Bumptech.Glide.Request.Target;
using Object = Java.Lang.Object;
using Android.Database;
using Android.Provider;
using Android.Support.V4.App;
using LoaderManager = Android.App.LoaderManager;

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
            var recommendationIndent = new Intent(Activity, typeof(UpdateRecommendationsService));
            Activity.StartService(recommendationIndent);
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
            }

            public void OnItemSelected(Presenter.ViewHolder itemViewHolder, Object item, RowPresenter.ViewHolder rowViewHolder, Row row)
            {
                if (item is Video)
                {
                    _mainFragment._backgroundUri = new Uri(((Video)item).BackgroundImageUrl);
                    _mainFragment.StartBackgroundTimer();
                }
            }

            public void OnItemClicked(Presenter.ViewHolder itemViewHolder, Object item, RowPresenter.ViewHolder rowViewHolder, Row row)
            {
                if (item is Video)
                {
                    var video = (Video)item;
                    var intent = new Intent(Activity, typeof(VideoDetailsActivity));
                    intent.PutExtra(VideoDetailsActivity.Video, video);

                    var bundle = ActivityOptionsCompat.MakeSceneTransitionAnimation(
                        ActivityCompat,
                        ((ImageCardView)itemViewHolder.View).MainImageView,
                        VideoDetailsActivity.SharedElementName).ToBundle();
                    _mainFragment.Activity.StartActivity(intent, bundle);
                }
                else if (item is Java.Lang.String)
                {
                    var str = item.ToString();
                    if (str.Contains("Vertical Grid View"))
                    {
                        var intent =
                            new Intent(_mainFragment.Activity, typeof(VerticalGridActivity));
                        var bundle =
                            ActivityOptionsCompat.MakeSceneTransitionAnimation(
                                _mainFragment.Activity).ToBundle();
                        _mainFragment.StartActivity(intent, bundle);
                    }
                    else if (str.Contains("Guided Step First Page"))
                    {
                        var intent =
                            new Intent(_mainFragment.Activity, typeof(GuidedStepActivity));
                        var bundle =
                            ActivityOptionsCompat.MakeSceneTransitionAnimation(
                                _mainFragment.Activity).ToBundle();
                        _mainFragment.StartActivity(intent, bundle);
                    }
                    else if (str.Contains("Error Fragment"))
                    {
                        var errorFragment = new BrowseFragment();
                        _mainFragment.FragmentManager
                            .BeginTransaction()
                            .Replace(Resource.Id.main_frame, errorFragment)
                            .AddToBackStack(null)
                            .Commit();
                    }
                    else
                    {
                        Toast.MakeText(_mainFragment.Activity, item.ToString(), ToastLength.Long).Show();
                    }
                }
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
                .Load(uri)
                .AsBitmap()
                .CenterCrop()
                .Error(_defaultBackground)
                .Into(new MySimpleTarget(this, width, height));
            _backgroundTimer.Dispose();
        }

        private void StartBackgroundTimer()
        {
            _backgroundTimer?.Dispose();
            _backgroundTimer = new Timer(UpdateBackgroundTask, null, BackgroundUpdateDelay, Timeout.Infinite);
        }

        private void UpdateBackgroundTask(object state)
        {
            _handler.Post(() =>
            {
                if (_backgroundUri != null)
                    UpdateBackground(_backgroundUri.AbsoluteUri);
            });
        }

        class MySimpleTarget : SimpleTarget
        {
            private MainFragment _fragment;
            public MySimpleTarget(MainFragment fragment, int width, int height) : base(width, height)
            {
                _fragment = fragment;
            }

            public override void OnResourceReady(Object p0, IGlideAnimation p1)
            {
                _fragment._backgroundManager.SetBitmap((Bitmap)p0);
            }
        }
        public Loader OnCreateLoader(int id, Bundle args)
        {
            if (id == CategoryLoader)
                return new CursorLoader(
                    Activity, // Parent activity context
                    VideoContract.VideoEntry.ContentUri, // Table to query
                    new[] { "DISTINCT " + VideoContract.VideoEntry.ColumnCategory },
                    null, // No selection clause
                    null, // No selection arguments
                    null // Default sort order
                    );
            else
            {
                // Assume it is for a video.
                var category = args.GetString(VideoContract.VideoEntry.ColumnCategory);

                // This just creates a CursorLoader that gets all videos.
                return new CursorLoader(
                    Activity, // Parent activity context
                    VideoContract.VideoEntry.ContentUri, // Table to query
                    null, // Projection to return - null means return all fields
                    VideoContract.VideoEntry.ColumnCategory + " =?", // Selection clause
                    new[] { category }, // Select based on the category id.
                    null // Default sort order
                    );
            }
        }

        public void OnLoaderReset(Loader loader)
        {
            int loaderId = loader.Id;
            if (loaderId != CategoryLoader)
            {
                _videoCursorAdapters[loaderId].ChangeCursor(null);
            }
            else
            {
                _categoryRowAdapter.Clear();
            }
        }

        public void OnLoadFinished(Loader loader, Object data)
        {
            ICursor cursor = (ICursor)data;
            if (cursor != null && cursor.MoveToFirst())
            {
                int loaderId = loader.Id;
                if (loaderId == CategoryLoader)
                {
                    // Every time we have to re-get the category loader,
                    // we must re-create the sidebard.
                    _categoryRowAdapter.Clear();

                    // Iterate through each category entry and add it to the ArrayAdapter.
                    while (!cursor.IsAfterLast)
                    {
                        int categoryIndex =
                            cursor.GetColumnIndex(VideoContract.VideoEntry.ColumnCategory);
                        string category = cursor.GetString(categoryIndex);

                        // Create header for this category.
                        var header = new HeaderItem(category);

                        int videoLoaderId = category.GetHashCode(); // Create unique int from category
                        CursorObjectAdapter existingAdapter;
                        if (_videoCursorAdapters.TryGetValue(videoLoaderId, out existingAdapter))
                        {
                            // Map video results from the database to Video objects.
                            var videoCursorAdapter =
                                new CursorObjectAdapter(new CardPresenter());
                            videoCursorAdapter.Mapper = new VideoCursorMapper();
                            _videoCursorAdapters[videoLoaderId] = videoCursorAdapter;

                            var row = new ListRow(header, videoCursorAdapter);
                            _categoryRowAdapter.Add(row);

                            // Start loading the videos from the database from a particular category.
                            var args = new Bundle();
                            args.PutString(VideoContract.VideoEntry.ColumnCategory, category);
                            LoaderManager.InitLoader(videoLoaderId, args, this);
                        }
                        else
                        {
                            var row = new ListRow(header, existingAdapter);
                            _categoryRowAdapter.Add(row);
                        }

                        cursor.MoveToNext();
                    }

                    // Create a row for this special case with more samples.
                    var gridHeader = new HeaderItem("More Samples");
                    var gridPresenter = new GridItemPresenter(this);
                    var gridRowAdapter = new ArrayObjectAdapter(gridPresenter);
                    gridRowAdapter.Add("Vertical Grid View");
                    gridRowAdapter.Add("Guided Step First Page");
                    gridRowAdapter.Add("Error Fragment");
                    gridRowAdapter.Add("Personal Settings");
                    var row = new ListRow(gridHeader, gridRowAdapter);
                    _categoryRowAdapter.Add(row);

                    StartEntranceTransition();
                    // cursors have loaded.
                }
                else
                {
                    // The CursorAdapter contains a Cursor pointing to all videos.
                    _videoCursorAdapters[loaderId].ChangeCursor(cursor);
                }
            }
            else
            {
                // Start an Intent to fetch the videos.
                var serviceIntent = new Intent(Activity, typeof(FetchVideoService));
                Activity.StartService(serviceIntent);
            }
        }
    }
}