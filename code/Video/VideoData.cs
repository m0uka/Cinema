using System;

namespace Cinema.Video
{
	public class VideoData
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public double FrameRate { get; set; }
		public int FrameCount { get; set; }
		public int SoundSampleRate { get; set; }
		public int SoundChannels { get; set; }
		public double DurationDouble { get; set; }
	}
}
