using System;
using Android.App;
using Android.Content;
using Android.Provider;

namespace AndroidExample.TVLeanback.Data
{
    /// <summary>
    /// VideoContract represents the contract for storing videos in the SQLite database.
    /// </summary>
    public static class VideoContract
    {
        // The name for the entire content provider.
        public const string ContentAuthority = "tvleanback";
        public static readonly Uri BaseContentUri = new Uri("content://" + ContentAuthority);
        public const string PathVideo = "video";

        public class VideoEntry 
        {
            public static readonly Uri ContentUri = new Uri(BaseContentUri, PathVideo);

            public const string ContentType =
                ContentResolver.CursorDirBaseType + "/" + ContentAuthority + "." + PathVideo;

            // Name of the video table.
            public const string TableName = "video";

            // Column with the foreign key into the category table.
            public const string ColumnCategory = "category";

            // Name of the video.
            public const string ColumnName = SearchManager.SuggestColumnText1;

            // Description of the video.
            public const string ColumnDesc = SearchManager.SuggestColumnText2;

            // The url to the video content.
            public const string ColumnVideoUrl = "video_url";

            // The url to the background image.
            public const string ColumnBgImageUrl = "bg_image_url";

            // The studio name.
            public const string ColumnStudio = "studio";

            // The card image for the video.
            public const string ColumnCardImg = SearchManager.SuggestColumnResultCardImage;

            // The content type of the video.
            public const string ColumnContentType = SearchManager.SuggestColumnContentType;

            // Whether the video is live or not.
            public const string ColumnIsLive = SearchManager.SuggestColumnIsLive;

            // The width of the video.
            public const string ColumnVideoWidth = SearchManager.SuggestColumnVideoWidth;

            // The height of the video.
            public const string ColumnVideoHeight = SearchManager.SuggestColumnVideoHeight;

            // The audio channel configuration.
            public const string ColumnAudioChannelConfig = SearchManager.SuggestColumnAudioChannelConfig;

            // The purchase price of the video.
            public const string ColumnPurchasePrice = SearchManager.SuggestColumnPurchasePrice;

            // The rental price of the video.
            public const string ColumnRentalPrice = SearchManager.SuggestColumnRentalPrice;

            // The rating style of the video.
            public const string ColumnRatingStyle = SearchManager.SuggestColumnRatingStyle;

            // The score of the rating.
            public const string ColumnRatingScore = SearchManager.SuggestColumnRatingScore;

            // The year the video was produced.
            public const string ColumnProductionYear = SearchManager.SuggestColumnProductionYear;

            // The duration of the video.
            public const string ColumnDuration = SearchManager.SuggestColumnDuration;

            // The action intent for the result.
            public const string ColumnAction = SearchManager.SuggestColumnIntentAction;

            public static Uri BuildVideoUri(long id)
            {
                throw new NotImplementedException();
            }
        }
    }
}