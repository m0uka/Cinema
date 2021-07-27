using System;

namespace Cinema.Video
{
	public class VideoInfoResult
	{
		public bool Exists { get; set; }
		public bool TooLong { get; set; }
        
		public string Title { get; set; }
		public string ChannelTitle { get; set; }
		public string ThumbnailUrl { get; set; }
		public DateTime PublishedAt { get; set; }
	}
}
