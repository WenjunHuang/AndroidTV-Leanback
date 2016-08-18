using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V17.Leanback;
using Android.Support.V17.Leanback.App;
using Android.Support.V17.Leanback.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace AndroidExample.TVLeanback.UI
{
    public class PlaybackControlHelper : PlaybackControlGlue
    {
        private const string Tag = nameof(PlaybackControlHelper);
        const bool Debug = BuildConfig.Debug;

        private static readonly int[] SeekSpeeds = {2};
        const int DefaultUpdatePeroid = 500;
        const int UpdatePeriod = 16;
        public PlaybackControlHelper(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public PlaybackControlHelper(Context context, PlaybackOverlayFragment fragment, int[] fastForwardSpeeds, int[] rewindSpeeds) : base(context, fragment, fastForwardSpeeds, rewindSpeeds)
        {
        }

        public PlaybackControlHelper(Context context, PlaybackOverlayFragment fragment, int[] seekSpeeds) : base(context, fragment, seekSpeeds)
        {
        }

        public PlaybackControlHelper(Context context, int[] fastForwardSpeeds, int[] rewindSpeeds) : base(context, fastForwardSpeeds, rewindSpeeds)
        {
        }

        public PlaybackControlHelper(Context context, int[] seekSpeeds) : base(context, seekSpeeds)
        {
        }

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