using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Support.V17.Leanback.Widget;
using Android.Support.V4.App;
using Android.Views;
using AndroidExample.TVLeanback.Data;
using AndroidExample.TVLeanback.Model;
using AndroidExample.TVLeanback.Player;
using AndroidExample.TVLeanback.Util;
using Com.Google.Android.Exoplayer;
using Com.Google.Android.Exoplayer.Util;
using Java.Lang;
using LoaderManager = Android.App.LoaderManager;
using MediaController = Android.Media.Session.MediaController;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;

namespace AndroidExample.TVLeanback.UI
{
    public class PlaybackOverlayFragment : 
        Android.Support.V17.Leanback.App.PlaybackOverlayFragment, LoaderManager.ILoaderCallbacks
    {
        const string Tag = nameof(PlaybackOverlayFragment);
        const int DefaultBackgroundType = BgLight;
        const string AutoPlay = "auto_play";
        const int RecommendedVideosLoader = 1;
        const int QueueVideosLoader = 2;
        private static readonly Bundle AutoPlayExtras = new Bundle();

        static PlaybackOverlayFragment()
        {
            AutoPlayExtras.PutBoolean(AutoPlay, true);
        }

        private readonly VideoCursorMapper _videoCursorMapper = new VideoCursorMapper();
        private int _specificVideoLoaderId = 3;
        private int _queueIndex = -1;
        private Video _selectedVideo;
        private ArrayObjectAdapter _rowsAdapter;
        private List<MediaSession.QueueItem> _queue = new List<MediaSession.QueueItem>();
        private CursorObjectAdapter _videoCursorAdapter;
        private MediaSession _session;
        private LoaderManager.ILoaderCallbacks _callbacks;
        private MediaController _mediaController;
        private PlaybackControlHelper _glue;
        private MediaController.Callback _mediaControllerCallback;
        private VideoPlayer _player;
        private bool _isMetadataSet = false;
        private AudioManager _audioManager;
        private bool _hasAudioFocus;
        private bool _pauseTransient;
        private readonly Action<AudioFocus> _audioFocusChangeAction;

        public PlaybackOverlayFragment()
        {
            _audioFocusChangeAction = (focusChange) =>
            {
                switch (focusChange)
                {
                    case AudioFocus.Loss:
                        AbandonAudioFocus();
                        Pause();
                        break;
                    case AudioFocus.LossTransient:
                        if (_glue.IsMediaPlaying)
                        {
                            Pause();
                            _pauseTransient = true;
                        }
                        break;
                    case AudioFocus.LossTransientCanDuck:
                        _player.Mute = true;
                        break;
                    case AudioFocus.Gain:
                    case AudioFocus.GainTransient:
                    case AudioFocus.GainTransientMayDuck:
                        if (_pauseTransient)
                            Play();
                        _player.Mute = false;
                        break;
                }
            };
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            _callbacks = null;
            CreateMediaSession();
        }

        public override void OnStop()
        {
            base.OnStop();
            _session.Release();
            ReleasePlayer();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (_mediaController != null)
                _mediaController.UnregisterCallback(_mediaControllerCallback);
            _session.Release();
            ReleasePlayer();
        }

        public override void OnStart()
        {
            base.OnStart();

            // Set up UI
            var video = (Video)Activity.Intent.GetParcelableExtra(VideoDetailsActivity.Video);
            if (!UpdateSelectedVideo(video))
                return;

            _glue = new PlaybackControlHelper(this.Context, this, _selectedVideo);
            var controlsRowPresenter = _glue.CreateControlsRowAndPresenter();
            var controlsRow = _glue.ControlsRow;
            _mediaControllerCallback = _glue.CreateMediaControllerCallback();

            _mediaController = Activity.MediaController;
            _mediaController.RegisterCallback(_mediaControllerCallback);

            var ps = new ClassPresenterSelector();
            ps.AddClassPresenter(Java.Lang.Class.FromType(typeof(PlaybackControlsRow)), controlsRowPresenter);
            ps.AddClassPresenter(Java.Lang.Class.FromType(typeof(ListRow)), new ListRowPresenter());
            _rowsAdapter = new ArrayObjectAdapter();
            _rowsAdapter.Add(controlsRow);
            AddOtherRows();
            UpdatePlaybackRow();
            Adapter = _rowsAdapter;

            StartPlaying();
        }

        public override void OnResume()
        {
            base.OnResume();
            var video = (Video)Activity.Intent.GetParcelableExtra(VideoDetailsActivity.Video);
            if (!UpdateSelectedVideo(video))
                return;

            StartPlaying();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _audioManager = (AudioManager)Context.GetSystemService(Context.AudioService);

            // Initialize instance variables.
            var textureView = Activity.FindViewById<TextureView>(Resource.Id.texture_view);
            textureView.SurfaceTextureAvailable += (sender, args) =>
            {
                if (_player != null)
                    _player.Surface = new Surface(args.Surface);
            };
            textureView.SurfaceTextureDestroyed += (sender, args) =>
            {
                if (_player != null)
                    _player.BlockingClearSurface();
                args.Handled = true;
            };

            BackgroundType = DefaultBackgroundType;

            ItemViewClicked += (sender, args) =>
            {
                Video video = args.Item as Video;
                if (video != null)
                {
                    var intent = new Intent(Context, typeof(PlaybackOverlayActivity));
                    intent.PutExtra(VideoDetailsActivity.Video, video);

                    var bundle = ActivityOptionsCompat.MakeSceneTransitionAnimation(
                        Activity,
                        ((ImageCardView)args.ItemViewHolder.View).MainImageView,
                        VideoDetailsActivity.SharedElementName).ToBundle();
                    Activity.StartActivity(intent, bundle);
                }
            };
        }

        private bool UpdateSelectedVideo(Video video)
        {
            var intent = new Intent(Activity.Intent);
            intent.PutExtra(VideoDetailsActivity.Video, video);
            if (_selectedVideo != null && _selectedVideo.Equals(video))
                return false;

            _selectedVideo = video;

            var pi = PendingIntent.GetActivity(Activity, 0, intent, PendingIntentFlags.UpdateCurrent);
            _session.SetSessionActivity(pi);
            return true;
        }

        public override void OnPause()
        {
            base.OnPause();
            if (_glue.IsMediaPlaying)
            {
                var isVisibleBehind = Activity.RequestVisibleBehind(true);
                if (!isVisibleBehind)
                    Pause();
            }
            else
            {
                Activity.RequestVisibleBehind(false);
            }
        }

        private void SetPosition(long position)
        {
            if (position > _player.Duration)
                _player.SeekTo(_player.Duration);
            else if (position < 0)
                _player.SeekTo(0L);
            else
                _player.SeekTo(position);
        }

        private void CreateMediaSession()
        {
            if (_session == null)
            {
                _session = new MediaSession(Activity, "LeanbackSampleApp");
                _session.SetCallback(new MediaSessionCallback());
                _session.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
                _session.Active = true;

                // Set the Activity's MediaController used to invoke transport controls / adjust volumn.
                Activity.MediaController =
                    new MediaController(Context, _session.SessionToken);
                CurrentPlaybackState = PlaybackStateCode.None;
            }
        }

        private MediaSession.QueueItem GetQueueItem(Video video)
        {
            var desc = new MediaDescription.Builder()
                .SetDescription(video.Description)
                .SetMediaId($"{video.Id}")
                .SetMediaUri(Uri.Parse(video.VideoUrl))
                .SetIconUri(Uri.Parse(video.CardImageUrl))
                .SetSubtitle(video.Studio)
                .SetTitle(video.Title)
                .Build();

            return new MediaSession.QueueItem(desc, video.Id);
        }

        public long BufferedPosition => _player?.BufferedPosition ?? 0L;

        public long CurrentPosition => _player?.CurrentPositon ?? 0L;

        public long Duration => _player?.Duration ?? ExoPlayer.UnknownTime;

        private long GetAvailableActions(PlaybackStateCode nextState)
        {
            long actions = PlaybackState.ActionPlay
                           | PlaybackState.ActionPlayFromMediaId
                           | PlaybackState.ActionPlayFromSearch
                           | PlaybackState.ActionSkipToNext
                           | PlaybackState.ActionSkipToPrevious
                           | PlaybackState.ActionPause;

            if (nextState == PlaybackStateCode.Playing)
                actions |= PlaybackState.ActionPause;

            return actions;
        }

        private void Play()
        {
            // Request audio focus whenever we resume playback
            // because the app migh have abandoned audio focus due to the AUDIOFOCUS_LOSS
            RequestAudioFocus();

            if (_player == null)
            {
                CurrentPlaybackState = PlaybackStateCode.None;
                return;
            }
            if (!_glue.IsMediaPlaying)
            {
                _player.PlayControl.Start();
                CurrentPlaybackState = PlaybackStateCode.Playing;
            }
        }

        private void Pause()
        {
            _pauseTransient = false;

            if (_player == null)
            {
                CurrentPlaybackState = PlaybackStateCode.None;
                return;
            }
            if (_glue.IsMediaPlaying)
            {
                _player.PlayerControl.Pause();
                CurrentPlaybackState = PlaybackStateCode.Paused;
            }
        }

        private void RequestAudioFocus()
        {
            if (_hasAudioFocus)
                return;
            var result = _audioManager.RequestAudioFocus(
                AndroidListenerHelper.AudioFocusChange(_audioFocusChangeAction), Stream.Music, AudioFocus.Gain);
            if (result == AudioFocusRequest.Granted)
                _hasAudioFocus = true;
            else
                Pause();
        }

        private void AbandonAudioFocus()
        {
            _hasAudioFocus = false;
            _audioManager.AbandonAudioFocus(
                AndroidListenerHelper.AudioFocusChange(_audioFocusChangeAction));
        }

        internal void UpdatePlaybackRow()
        {
            _rowsAdapter.NotifyArrayItemRangeChanged(0, 1);
        }

        /// <summary>
        /// Creates a ListRow for releated videos.
        /// </summary>
        private void AddOtherRows()
        {
            _videoCursorAdapter = new CursorObjectAdapter(new CardPresenter());
            _videoCursorAdapter.Mapper = new VideoCursorMapper();

            var args = new Bundle();
            args.PutString(VideoContract.VideoEntry.ColumnCategory, _selectedVideo.Category);
            LoaderManager.InitLoader(RecommendedVideosLoader, args, this);

        }

        private PlaybackStateCode CurrentPlaybackState
        {
            get
            {
                if (Activity != null)
                {
                    var state = Activity.MediaController.PlaybackState;
                    if (state != null)
                        return state.State;
                    else
                        return PlaybackStateCode.None;
                }
                return PlaybackStateCode.None;
            },
            set
            {
                var currPosition = CurrentPosition;
                var stateBuilder =
                    new Android.Media.Session.PlaybackState.Builder()
                    .SetActions(GetAvailableActions(value));
                stateBuilder.SetState(value, currPosition, 1.0f);
                _session.SetPlaybackState(stateBuilder.Build());
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        Loader LoaderManager.ILoaderCallbacks.OnCreateLoader(int id, Bundle args)
        {
            switch (id)
            {
                case RecommendedVideosLoader:
                case QueueVideosLoader:
                    var category = args.GetString(VideoContract.VideoEntry.ColumnCategory);
                    return new CursorLoader(
                        Activity,
                        VideoContract.VideoEntry.ContentUri,
                        null, // Projection to return - null means return all fields.
                        VideoContract.VideoEntry.ColumnCategory + " = ?",
                        // Selection clause is category.
                        new []{category}, // Select based on the category.
                        null // Default sort order
                        );
            }
        }

        void LoaderManager.ILoaderCallbacks.OnLoaderReset(Loader loader)
        {
            _videoCursorAdapter.ChangeCursor(null);
        }

        void LoaderManager.ILoaderCallbacks.OnLoadFinished(Loader loader, Object data)
        {
            ICursor cursor = (ICursor) data;
            if (cursor != null && cursor.MoveToFirst())
            {
                switch (loader.Id)
                {
                    case QueueVideosLoader:
                        _queue.Clear();
                        while (!cursor.IsAfterLast)
                        {
                            Video v = (Video) _videoCursorMapper.Convert(cursor);

                            // Set the queue index to the selected video.
                            if (v.Id == _selectedVideo.Id)
                                _queueIndex = _queue.Count();

                            // Add the video to the queue.
                            var item = GetQueueItem(v);
                            _queue.Add(item);

                            cursor.MoveToNext();
                        }
                        break;
                    case RecommendedVideosLoader:
                        _videoCursorAdapter.ChangeCursor(cursor);
                        break;
                    default:
                        // Playing a specific video.
                        Video video = (Video) _videoCursorMapper.Convert(cursor);
                        PlayVideo(video, AutoPlayExtras);
                        break;
                }
            }
        }

        private void SetPosition(long position)
        {
            if (position > _player.Duration)
            {
                _player.SeekTo(_player.Duration);
            }
            else if (position < 0)
            {
                _player.SeekTo(0L);
            }
            else
            {
                _player.SeekTo(position);
            }
        }

        private void CreateMediaSession()
        {
            if (_session == null)
            {
                _session = new MediaSession(Activity, "LeanbackSampleApp");
                _session.SetCallback(new MediaSessionCallback());
                _session.SetFlags(MediaSessionFlags.HandlesMediaButtons 
                    | MediaSessionFlags.HandlesTransportControls);
                _session.Active = true;


                // Set the Activity's MediaController used to invoke transport controls / adjust volumn.
                Activity.MediaController =
                    new MediaController(Context, _session.SessionToken);
                CurrentPlaybackState = PlaybackStateCode.None;
            }
        }

        private VideoPlayer.RenderBuilder RendererBuilder
        {
            get
            {
                var userAgent = ExoPlayerUtil.GetUserAgent(Context, "ExoVideoPlayer");
                var contentUri = new System.Uri(_selectedVideo.VideoUrl);
                var contentType = ExoPlayerUtil.InferContentType(
                    contentUri.Segments.Last());

                switch (contentType)
                {
                    case ExoPlayerUtil.TypeOther:
                        return new ExtractorRendererBuilder(Context,
                            userAgent,
                            contentUri);
                    default:
                        throw new IllegalStateException("Unsupported type: " + contentType);
                }
            }
        }

        private void PreparePlayer()
        {
            if (_player == null)
            {
                _player = new VideoPlayer(RendererBuilder);
                _player
            }
        }
    }
}