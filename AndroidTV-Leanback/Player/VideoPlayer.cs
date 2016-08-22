using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Com.Google.Android.Exoplayer;
using Com.Google.Android.Exoplayer.Chunk;
using Com.Google.Android.Exoplayer.Text;
using Com.Google.Android.Exoplayer.Upstream;
using Com.Google.Android.Exoplayer.Util;

namespace AndroidExample.TVLeanback.Player
{
    public class VideoPlayer
    {
        public enum State
        {
            Idle = ExoPlayer.StateIdle,
            Preparing = ExoPlayer.StatePreparing,
            Buffering = ExoPlayer.StateBuffering,
            Ready = ExoPlayer.StateReady,
            Ended = ExoPlayer.StateEnded
        }

        public enum TrackState
        {
            TrackDisabled = ExoPlayer.TrackDisabled,
            TrackDefault = ExoPlayer.TrackDefault
        }

        public const int RendererCount = 4;

        public enum TrackType
        {
            Video = 0,
            Audio = 1,
            Text = 2,
            Metadata = 3
        }

        public enum RendererBuildingState
        {
            Idle = 0,
            Building = 1,
            Built = 2
        }

        public class StateChangedArgs : EventArgs
        {
            public bool PlayWhenReady { get; set; }
            public int PlaybackState { get; set; }
        }

        public class ErrorArgs : EventArgs
        {
            public Exception Error { get; set; }
        }

        public class VideoSizeChangedArgs : EventArgs
        {
            public int Width { get; set; }
            public int Height { get; set; }

            public int UnappliedRotationDegress { get; set; }
            public float PixelWidthHeightRatio { get; set; }
        }

        private readonly IExoPlayer _player;
        private readonly PlayerControl _playerControl;
        private readonly Handler _mainHandler;

        public event EventHandler<StateChangedArgs> StateChangedEvent;
        public event EventHandler<ErrorArgs> ErrorEvent;
        public event EventHandler<VideoSizeChangedArgs> VideoSizeChangedEvent;

        private IRendererBuilder _rendererBuilder;

        private RendererBuildingState _rendererBuildingState;
        private State _lastReportedPlaybackState;
        private bool _lastReportPlayWhenReady;

        private Surface _surface;
        private TrackRenderer _videoRenderer;
        private CodecCounters _codecCounters;
        private Format _videoFormat;
        private int _videoTrackToRestore;

        private IBandwidthMeter _bandwidthMeter;
        private bool _background;

        private Action<List<Cue>> _captionAction;
        private Action<Dictionary<string, object>> _id3MetadataAction;

        public VideoPlayer(IRendererBuilder rendererBuilder)
        {
            _rendererBuilder = rendererBuilder;
            _player = ExoPlayerFactory.NewInstance(RendererCount, 1000, 5000);
            _player.AddListener(this);
            _playerControl = new PlayerControl(_player);
            _mainHandler = new Handler();
            _lastReportedPlaybackState = State.Idle;
            _rendererBuildingState = RendererBuildingState.Idle;

            // Disabl text initially.
            _player.SetSelectedTrack((int)TrackType.Text, (int)TrackState.TrackDisabled);
        }

        public PlayerControl PlayerControl => _playerControl;

        public Surface Surface
        {
            get { return _surface; },
            set
            {
                _surface = value;
                PushSurface(false);
            }
        }

        public void BlockingClearSurface()
        {
            _surface = null;
            PushSurface(true);
        }

        public int GetTrackCount(TrackType type)
        {
            return _player.GetTrackCount((int)type);
        }

        public int GetSelectedTrack(TrackType type)
        {
            return _player.GetSelectedTrack((int) type);
        }

        public void SetSelectedTrack(TrackType type, int index)
        {
            _player.SetSelectedTrack((int)type, index);
            if (type == TrackType.Text && index < 0 && _captionAction != null)
                _captionAction(new List<Cue>());
        }

        public bool Backgrounded
        {
            get
            {
                return _background;
            },
            set
            {
                if (_background != value)
                {
                    _background = value;
                    if (_background)
                    {
                        _videoTrackToRestore = GetSelectedTrack(TrackType.Video);
                        SetSelectedTrack(TrackType.Video, (int) TrackState.TrackDisabled);
                        BlockingClearSurface();
                    }
                    else
                    {
                        SetSelectedTrack(TrackType.Video, _videoTrackToRestore);
                    }
                }
            }
        }

        public void Prepare()
        {
            if (_rendererBuildingState == RendererBuildingState.Built)
                _player.Stop();

            _rendererBuilder.Cancel();
            _videoFormat = null;
            _videoRenderer = null;
            _rendererBuildingState = RendererBuildingState.Building;
            MaybeReportPlayerState();
            _rendererBuilder.BuildRenderers(this);
        }

        internal void OnRenderers(TrackRenderer[] renderers, IBandwidthMeter bandwidthMeter)
        {
            for (int i = 0; i < renderers.Length; ++i)
            {
                if (renderers[i] == null)
                {
                    // Convert a null renderer to a dummy renderer.
                    renderers[i] = new DummyTrackRenderer();
                }
            }

            // Complete preparation.
            _videoRenderer = renderers[(int) TrackType.Video];
            _codecCounters = _videoRenderer is MediaCodecTrackRenderer
                ? ((MediaCodecTrackRenderer) _videoRenderer).CodecCounters
                : renderers[(int) TrackType.Audio] is MediaCodecTrackRenderer
                    ? ((MediaCodecTrackRenderer) renderers[(int) TrackType.Audio]).CodecCounters
                    : null;
            _bandwidthMeter = bandwidthMeter;
            PushSurface(false);
            _player.Prepare(renderers);
            _rendererBuildingState = RendererBuildingState.Built;
        }

        public bool PlayWhenReady
        {
            set { _player.PlayWhenReady = value; }
        }

        public void SeekTo(long positionMs)
        {
            _player.SeekTo(positionMs);
        }

        public void Release()
        {
            _rendererBuilder.Cancel();
            _rendererBuildingState = RendererBuildingState.Idle;
            _surface = null;
            _player.Release();
        }

        public State PlaybackState
        {
            get
            {
                if (_rendererBuildingState == RendererBuildingState.Building)
                    return State.Preparing;

                State playerState = (State)_player.PlaybackState;
                if (_rendererBuildingState == RendererBuildingState.Built
                    && playerState == State.Idle)
                    return State.Preparing;

                return playerState;
            }
        }

        public void Mute(bool mute)
        {
            _player.SetSelectedTrack((int)TrackType.Audio, mute ? -1 : 0);
        }
    }

    public interface IRendererBuilder
    {
        void BuildRenderers(VideoPlayer player);
        void Cancel();
    }
}