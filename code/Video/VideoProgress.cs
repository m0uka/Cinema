using Sandbox;

namespace Cinema.Video
{
	public class VideoProgress
	{
		public bool IsDownloading { get; set; }
		public string DownloadText { get; set; }
		public bool IsConverting { get; set; }
		
		public string VideoTitle { get; set; }
		public double DownloadProgress { get; set; }
		public string VideoSize { get; set; }
		public string Eta { get; set; }
		public string DownloadRate { get; set; }
	}
}
