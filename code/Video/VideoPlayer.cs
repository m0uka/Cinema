using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cinema.Entities;
using Cinema.UI;
using Sandbox;

namespace Cinema.Video
{
	public partial class VideoPlayer
	{
		public Action<Texture> OnFrameChange { get; set; }
		
		public Texture ActiveTexture { get; set; }
		
		private List<byte[]> Frames { get; set; } = new();
		private Dictionary<int, Texture> TextureFrames { get; set; } = new();
		
		public int CurrentFrame { get; set; }

		public bool IsPlaying { get; set; } = false;
		public bool IsStreaming { get; set; } = false;
		
		private double LastFrame { get; set; }
		
		public TimeSince PlaybackStart { get; set; }
		public TimeSince? StartFrom { get; set; } = null;
		
		public VideoData VideoData { get; set; }
		public VideoProgress VideoProgress { get; set; }

		public double FrameLateDiff { get; set; }
		public double FrameDesyncCorrectValue { get; set; }

		public void Playback()
		{
			if ( !IsPlaying ) return;
			if ( Frames.Count == 0 ) return;

			var sw = new Stopwatch();
			sw.Start();
			
			PlayFrame( CurrentFrame );
			
			sw.Stop();

			VideoStreamPanel.Instance.FrameLoadTime = (float) sw.Elapsed.TotalMilliseconds;
		}

		public void UpdateFrames()
		{
			if ( !IsPlaying ) return;
			if ( Frames.Count == 0 ) return;
			
			CorrectDesync();
			
			if ( LastFrame > Time.Now )
			{
				return;
			}

			if ( CurrentFrame + 2 > Frames.Count )
			{
				if ( !IsStreaming )
				{
					IsPlaying = false;
				}

				return;
			}

			double frameRate = 1 / (VideoData?.FrameRate ?? 30);
			LastFrame = Time.Now + frameRate + FrameDesyncCorrectValue;
			CurrentFrame++;
		}
		
		private void CorrectDesync()
		{
			double playbackProgress = (double) CurrentFrame / VideoData.FrameCount;
			double frameRateFraction = 1 / VideoData.FrameRate;
				
			double realDuration = VideoData.FrameCount * frameRateFraction;
			double realProgress = playbackProgress * realDuration;

			double diff = Math.Abs(realProgress - PlaybackStart);
			float multiplier = diff > 1 ? 10 : 1;

			// We are running too slow or fast!
			FrameDesyncCorrectValue = (realProgress - PlaybackStart) * multiplier;
		}

		private void PlayFrame( int frame )
		{
			if (TextureFrames.TryGetValue(frame, out var textureFrame))
			{
				ActiveTexture = textureFrame;
			}
			else
			{
				var frameData = Frames[CurrentFrame];
				SetFrame( frameData );
			}
		}

		private void SetFrame(byte[] data)
		{
			if ( ActiveTexture != null )
			{
				ActiveTexture.Dispose();	
			}

			// This is pretty expensive
			var stream = new MemoryStream( data );
			var texture = Sandbox.TextureLoader.Image.Load( stream );

			ActiveTexture = texture;
		}

		public void Play()
		{
			IsPlaying = true;
			IsStreaming = true;
			PlaybackStart = StartFrom ?? 0;
			CurrentFrame = 0;
		}

		public void Stop()
		{
			IsPlaying = false;
		}

		public void AddFrame( byte[] frame )
		{
			Frames.Add( frame );
		}
	}
}
