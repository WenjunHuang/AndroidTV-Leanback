using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace AndroidExample.TVLeanback.Util
{
    public static class AndroidListenerHelper
    {
        class ViewOnClickWrapper : Java.Lang.Object, View.IOnClickListener
        {
            private readonly Action<View> _action;

            public ViewOnClickWrapper(Action<View> action)
            {
                _action = action;
            }

            public void OnClick(View v)
            {
                _action(v);
            }
        }

        class OnAudioFocusChangeWrapper : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
        {
            private readonly Action<AudioFocus> _action;

            public OnAudioFocusChangeWrapper(Action<AudioFocus> action)
            {
                _action = action;
            }

            public void OnAudioFocusChange(AudioFocus focusChange)
            {
                _action(focusChange);
            }
        }

        public static AudioManager.IOnAudioFocusChangeListener AudioFocusChange(Action<AudioFocus> action)
        {
            return new OnAudioFocusChangeWrapper(action);
        }

        public static View.IOnClickListener ViewOnClick(Action<View> action)
        {
            return new ViewOnClickWrapper(action);            
        }
    }
}