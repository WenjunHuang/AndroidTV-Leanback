using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Support.V17.Leanback;
using Android.Support.V17.Leanback.App;
using Android.Support.V17.Leanback.Widget;
using Android.Views;
using AndroidExample.TVLeanback.Model;
using Java.Lang;
using Math = System.Math;

namespace AndroidExample.TVLeanback.UI
{
    public class PlaybackControlHelper : PlaybackControlGlue
    {
        private const string Tag = nameof(PlaybackControlHelper);
        const bool Debug = BuildConfig.Debug;

        private static readonly int[] SeekSpeeds = { 2 };
        const int DefaultUpdatePeroid = 500;
        const int NormalUpdatePeriod = 16;

        private Drawable _mediaArt;
        private PlaybackOverlayFragment _fragment;
        private readonly MediaController _mediaController;
        private readonly MediaController.TransportControls _transportControls;
        private readonly PlaybackControlsRow.RepeatAction _repeatAction;
        private readonly PlaybackControlsRow.ThumbsUpAction _thumbsUpAction;
        private readonly PlaybackControlsRow.ThumbsDownAction _thumbsDownAction;
        private readonly PlaybackControlsRow.FastForwardAction _fastFordwardAction;
        private readonly PlaybackControlsRow.RewindAction _rewindAction;
        private Video _video;
        private Handler _handler = new Handler();
        private System.Action _updateProgressAction;


        public PlaybackControlHelper(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public PlaybackControlHelper(Context context, PlaybackOverlayFragment fragment, Video video) : base(context, fragment, SeekSpeeds)
        {
            _fragment = fragment;
            _video = video;
            _mediaController = _fragment.Activity.MediaController;
            _transportControls = _mediaController.GetTransportControls();

            _thumbsUpAction = new PlaybackControlsRow.ThumbsUpAction(context);
            _thumbsUpAction.Index = PlaybackControlsRow.ThumbsUpAction.Outline;
            _thumbsDownAction = new PlaybackControlsRow.ThumbsDownAction(context);
            _thumbsDownAction.Index = PlaybackControlsRow.ThumbsDownAction.Outline;
            _repeatAction = new PlaybackControlsRow.RepeatAction(context);
        }

        public MediaController.Callback CreateMediaControllerCallback()
        {
            return new MediaControllerCallack();
        }

        public override PlaybackControlsRowPresenter CreateControlsRowAndPresenter()
        {
            var presenter = base.CreateControlsRowAndPresenter();

            var adapter = new ArrayObjectAdapter(new ControlButtonPresenterSelector());
            ControlsRow.SecondaryActionsAdapter = adapter;

            _fastFordwardAction =
                (PlaybackControlsRow.FastForwardAction) PrimaryActionsAdapter.Lookup(ActionFastForward);
            _rewindAction =
                (PlaybackControlsRow.RewindAction) PrimaryActionsAdapter.Lookup(ActionRewind);

            adapter.Add(_thumbsDownAction);
            adapter.Add(_repeatAction);
            adapter.Add(_thumbsUpAction);

            presenter.ActionClicked += (action) =>
            {
                DispatchAction(action);
            };

            return presenter;
        }

        public override void EnableProgressUpdating(bool enable)
        {
            _handler.RemoveCallbacks(_updateProgressAction);
            if (enable)
            {
                _handler.Post(_updateProgressAction);
            }
        }

        public override int UpdatePeriod
        {
            get
            {
                var view = _fragment.View;
                var totalTime = ControlsRow.TotalTime;
                if (view == null || totalTime <= 0 || view.Width == 0)
                {
                    return DefaultUpdatePeroid;
                }
                return Math.Max(NormalUpdatePeriod, (int)(totalTime/view.Width));
            }
        }

        public override void UpdateProgress()
        {
            if (_updateProgressAction == null)
            {
                _updateProgressAction = () =>
                {
                    var totalTime = ControlsRow.TotalTime;
                    var currentTime = CurrentPosition;
                    ControlsRow.CurrentTime = currentTime;

                    var progress = (int) _fragment;
                    ControlsRow.BufferedProgress = progress;
                }
            }
        }

        public object PrimaryActionsAdapter { get; private set; }

        protected override void OnRowChanged(PlaybackControlsRow row)
        {
            throw new NotImplementedException();
        }

        protected override void PausePlayback()
        {
            throw new NotImplementedException();
        }

        protected override void SkipToNext()
        {
            throw new NotImplementedException();
        }

        protected override void SkipToPrevious()
        {
            throw new NotImplementedException();
        }

        protected override void StartPlayback(int speed)
        {
            throw new NotImplementedException();
        }

        public override int CurrentPosition { get; }
        public override int CurrentSpeedId { get; }
        public override bool HasValidMedia { get; }
        public override bool IsMediaPlaying { get; }
        public override Drawable MediaArt { get; }
        public override int MediaDuration { get; }
        public override ICharSequence MediaSubtitleFormatted { get; }
        public override ICharSequence MediaTitleFormatted { get; }
        public override long SupportedActions { get; }
    }
}