using Android.Database;
using Android.Support.V17.Leanback.Database;
using Java.Lang;

namespace AndroidExample.TVLeanback.Model
{
    public class VideoCursorMapper : CursorMapper
    {
        protected override Object Bind(ICursor cursor)
        {
            return new Video
            {
                Id = cursor.GetLong()
            };
        }

        protected override void BindColumns(ICursor cursor)
        {
            throw new System.NotImplementedException();
        }
    }
}