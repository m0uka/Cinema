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
		public Entity PlayerEntity { get; set; }
		
		private List<byte[]> Frames { get; set; } = new();
		public int LoadedFrameCount => Frames.Count;

		private Dictionary<int, Texture> TextureFrames { get; set; } = new();
		
		public int CurrentFrame { get; set; }

		public bool IsPlaying { get; set; } = false;
		public bool IsStreaming { get; set; } = false;
		public bool IsBuffering { get; set; } = false;
		public bool ReadyToStart { get; set; } = false;
		
		private double LastFrame { get; set; }
		
		public Stopwatch PlaybackStopwatch { get; set; }

		public VideoData VideoData { get; set; }
		public VideoProgress VideoProgress { get; set; }
		
		public SoundStream SoundStream { get; set; }

		public double FrameLateDiff { get; set; }
		public double FrameDesyncCorrectValue { get; set; }
		
		private float BufferStart { get; set; }
		private float TimeBuffering { get; set; }

		public List<short[]> SoundSamplesPrecache { get; set; } = new();

		public VideoPlayer()
		{
			
		}

		public VideoPlayer( Entity playerEntity )
		{
			PlayerEntity = playerEntity;
		}

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

		private async void Buffer(int time = 1)
		{
			if ( IsBuffering ) return;
			
			PlaybackStopwatch?.Stop();
			BufferStart = Time.Now;
			IsBuffering = true;

			await Task.Delay( time * 1000 );

			EndBuffer();
		}

		private void EndBuffer()
		{
			IsBuffering = false;
			TimeBuffering += (Time.Now - BufferStart);
			
			PlaybackStopwatch.Start();

			// PlaybackStart -= (Time.Now - BufferStart);
		}

		public void UpdateFrames()
		{
			if ( !IsPlaying ) return;
			if ( Frames.Count == 0 ) return;
			if ( IsBuffering ) return;
			
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

				// Buffer(5);
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
			
			var elapsed = PlaybackStopwatch.Elapsed;

			double diff = Math.Abs(realProgress - elapsed.TotalSeconds);
			float multiplier = diff > 1 ? 10 : 1;

			// We are running too slow or fast!
			FrameDesyncCorrectValue = (realProgress - elapsed.TotalSeconds) * multiplier;
		}

		private void PlayFrame( int frame )
		{
			if (TextureFrames.TryGetValue(frame, out var textureFrame))
			{
				ActiveTexture = textureFrame;
			}
			
			var frameData = Frames[CurrentFrame];
			SetFrame( frameData );
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

			PlaybackStopwatch = new Stopwatch();
			PlaybackStopwatch.Start();
			CurrentFrame = 0;
		}

		public void Ready()
		{
			Frames.Clear();
			
			InitSoundStream();
			ReadyToStart = true;
		}

		private void InitSoundStream()
		{
			if ( SoundStream.IsValid )
			{
				SoundStream.Stop();
			}

			var sound = Sound.FromEntity( "audiostream.default", Local.Pawn );
			sound.SetVolume( 1 );
			sound.SetPitch( 1f );
				
			SoundStream = sound.CreateStream(VideoData.SoundSampleRate, 1);
		}

		public void Stop()
		{
			IsPlaying = false;
		}

		public bool IsReady()
		{
			return ReadyToStart && ( ((double) Frames.Count / VideoData.FrameCount) > 0.2f || Frames.Count > 200);
		}

		public void AddFrame( byte[] frame )
		{
			Frames.Add( frame );
			
			if ( IsReady() )
			{
				Play();
				ReadyToStart = false;
			}
		}

		public void AddSoundSample( short[] soundData )
		{
			if ( !IsPlaying )
			{
				SoundSamplesPrecache.Add( soundData );
			}
			else
			{
				if ( SoundSamplesPrecache.Count != 0 )
				{
					foreach ( var sample in SoundSamplesPrecache )
					{
						SoundStream.WriteData(sample);
					}
					
					SoundSamplesPrecache.Clear();
				}
				SoundStream.WriteData( soundData );
			}
		}
	}
}
