using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaskInitializer.Models.Cores
{
    public class GoogleReview
    {
        [JsonProperty("reviews")]
        public List<GoogleReviewReview> Reviews { get; set; }
    }

    public class GoogleReviewReview
    {
        [JsonProperty("comments")]
        public List<GoogleReviewReviewComment> Comments { get; set; }

        [JsonProperty("authorName")]
        public string AuthorName { get; set; }

        [JsonProperty("reviewId")]
        public string ReviewId { get; set; }
    }

    public class GoogleReviewReviewComment
    {
        [JsonProperty("userComment")]
        public GoogleReviewReviewCommentUserComment UserComment { get; set; }
    }

    public class GoogleReviewReviewCommentUserComment
    {
        [JsonProperty("deviceMetadata")]
        public GoogleReviewReviewCommentUserCommentDeviceMetadata DeviceMetadata { get; set; }

        [JsonProperty("lastModified")]
        public GoogleReviewReviewCommentUserCommentLastModified LastModified { get; set; }

        [JsonProperty("reviewerLanguage")]
        public string ReviewerLanguage { get; set; }

        [JsonProperty("appVersionName")]
        public string AppVersionName { get; set; }

        [JsonProperty("device")]
        public string Device { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("androidOsVersion")]
        public long AndroidOsVersion { get; set; }

        [JsonProperty("appVersionCode")]
        public long AppVersionCode { get; set; }

        [JsonProperty("starRating")]
        public long StarRating { get; set; }
    }

    public class GoogleReviewReviewCommentUserCommentDeviceMetadata
    {
        [JsonProperty("nativePlatform")]
        public string NativePlatform { get; set; }

        [JsonProperty("deviceClass")]
        public string DeviceClass { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("screenDensityDpi")]
        public long ScreenDensityDpi { get; set; }

        [JsonProperty("screenHeightPx")]
        public long ScreenHeightPx { get; set; }

        [JsonProperty("screenWidthPx")]
        public long ScreenWidthPx { get; set; }

        [JsonProperty("glEsVersion")]
        public long GlEsVersion { get; set; }

        [JsonProperty("ramMb")]
        public long RamMb { get; set; }
    }

    public class GoogleReviewReviewCommentUserCommentLastModified
    {
        [JsonProperty("seconds")]
        public string Seconds { get; set; }

        [JsonProperty("nanos")]
        public long Nanos { get; set; }
    }
}