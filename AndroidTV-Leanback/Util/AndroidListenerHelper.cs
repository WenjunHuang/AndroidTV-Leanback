using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
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
            private Action<View> _action;

            public ViewOnClickWrapper(Action<View> action)
            {
                _action = action;
            }
            public new void Dispose()
            {
                base.Dispose();
            }

            public new IntPtr Handle => base.Handle;

            public void OnClick(View v)
            {
                _action(v);
            }
        }
        public static View.IOnClickListener ViewOnClick(Action<View> action)
        {
            return new ViewOnClickWrapper(action);            
        }
    }
}