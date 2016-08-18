using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Object = Java.Lang.Object;

namespace AndroidExample.TVLeanback.Module
{
    public sealed class Video : Java.Lang.Object, IParcelable
    {
        public long Id { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string CardImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public string Studio { get; set; }

        private Video() { }
        private Video(Parcel parcel)
        {
            Id = parcel.ReadLong();
            Category = parcel.ReadString();
            Title = parcel.ReadString();
            Description = parcel.ReadString();
            BackgroundImageUrl = parcel.ReadString();
            CardImageUrl = parcel.ReadString();
            VideoUrl = parcel.ReadString();
            Studio = parcel.ReadString();
        }

        public static readonly IParcelableCreator CREATOR = new VideoCreator();

        class VideoCreator : Java.Lang.Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new Video(source);
            }

            public Object[] NewArray(int size)
            {
                return new Video[size];
            }

            public new void Dispose()
            {
                base.Dispose();
            }

            public new IntPtr Handle => base.Handle;
        }

        public int DescribeContents() => 0;

        public void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteLong(Id);
            dest.WriteString(Category);
            dest.WriteString(Title);
            dest.WriteString(Description);
            dest.WriteString(BackgroundImageUrl);
            dest.WriteString(CardImageUrl);
            dest.WriteString(VideoUrl);
            dest.WriteString(Studio);
        }

        public class VideoBuilder
        {
            private long _id;
            private string _category;
            private string _title;
            private string _description;
            private string _backgroundImageUrl;
            private string _cardImageUrl;
            private string _videoUrl;
            private string _studio;

            public VideoBuilder Id(long id)
            {
                _id = id;
                return this;
            }

            public VideoBuilder Category(string category)
            {
                _category = category;
                return this;
            }

            public VideoBuilder Title(string title)
            {
                _title = title;
                return this;
            }

            public VideoBuilder Description(string description)
            {
                _description = description;
                return this;
            }

            public VideoBuilder VideoUrl(string videoUrl)
            {
                _videoUrl = videoUrl;
                return this;
            }

            public VideoBuilder BackgroundImageUrl(string backgroundUrl)
            {
                _backgroundImageUrl = backgroundUrl;
                return this;
            }

            public VideoBuilder CardImageUrl(string cardImageUrl)
            {
                _cardImageUrl = cardImageUrl;
                return this;
            }

            public VideoBuilder Studio(string studio)
            {
                _studio = studio;
                return this;
            }

            public Video BuildFromMediaDesc(MediaDescription desc)
            {
                return new Video
                {
                    Id = long.Parse(desc.MediaId),
                    Category = "",
                    Title = desc.Title,
                    Description = desc.Description,
                    VideoUrl = "",
                    BackgroundImageUrl = "",
                    CardImageUrl = desc.IconUri.ToString(),
                    Studio = desc.Subtitle
                };
            }

            public Video Build()
            {
                return new Video
                {
                    Id = _id,
                    Category = _category,
                    Title = _title,
                    Description = _description,
                    VideoUrl = _videoUrl,
                    BackgroundImageUrl = _backgroundImageUrl,
                    CardImageUrl = _cardImageUrl,
                    Studio = _studio
                };
            }

        }
    }
}