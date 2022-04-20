using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Cinema.Extensions;
using Cinema.Video;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Cinema.UI
{
	public class VideoStreamPanel : Panel
	{
		public static VideoStreamPanel Instance { get; set; }
		public Label DebugText { get; set; }
		
		public VideoPlayer Player { get; set; }
		public VideoReceiver Receiver { get; set; }
		
		public float FrameLoadTime { get; set; }

		public float Throughput { get; set; }

		public VideoStreamPanel()
		{
			DebugText = Add.Label( "DEBUG TEXT" );

			DebugText.Style.FontColor = Color.White;
			DebugText.Style.MarginLeft = 20;
			DebugText.Style.MarginTop = 20;

			Instance = this;
		}
		

		[Event.Frame]
		private void UpdateDebug()
		{
			if ( Player == null ) return;
			
			var builder = new StringBuilder();

			var progress = Player.VideoProgress;
			if ( progress != null )
			{
				builder.AppendLine( "VIDEO PROGRESS:" );
				builder.AppendLine( "------------------------------" );
				
				builder.AppendLine( $"Is probing: {progress.IsProbing}" );
				builder.AppendLine( $"Is downloading: {progress.IsDownloading}" );

				if ( !progress.IsProbing && progress.VideoInfo != null )
				{
					builder.AppendLine();
					
					builder.AppendLine( $"### VIDEO INFO ###" );
					builder.AppendLine( $"Video title: {progress.VideoInfo.Title}" );
					builder.AppendLine( $"Channel name: {progress.VideoInfo.ChannelTitle}" );
					builder.AppendLine( $"Published at: {progress.VideoInfo.PublishedAt}" );
					
					builder.AppendLine();
				}
				
				if ( progress.IsDownloading )
				{
					builder.AppendLine( $"Download status: {progress.DownloadText}" );
					builder.AppendLine( $"Download ETA: {progress.Eta}" );
					builder.AppendLine( $"Download rate: {progress.DownloadRate}" );
					builder.AppendLine( $"Video size: {progress.VideoSize}" );
				}

				builder.AppendLine( $"Is converting: {progress.IsConverting}" );
				builder.AppendLine();
			}

			var videoData = Player.VideoData;

			if ( videoData != null )
			{
				builder.AppendLine( "VIDEO DATA:" );
				builder.AppendLine( "---------------------------" );
				
				builder.AppendLine( $"Total frames: {videoData?.FrameCount}" );
				builder.AppendLine( $"Frame rate: {videoData?.FrameRate}" );
				builder.AppendLine();
				
				builder.AppendLine( "PLAYER INFO:" );
				builder.AppendLine( "---------------------------" );
				
				builder.AppendLine( $"Current frame: {Player.CurrentFrame}" );
				builder.AppendLine( $"Is playing: {Player.IsPlaying}" );
				builder.AppendLine( $"Is streaming: {Player.IsStreaming}" );
				builder.AppendLine( $"Is buffering: {Player.IsBuffering || (!Player.IsPlaying && !Player.IsReady())}" );
				builder.AppendLine();
				
				builder.AppendLine( "--------STATS--------" );
				
				builder.AppendLine( $"Frames lead: {Player.LoadedFrameCount - Player.CurrentFrame}" );
				builder.AppendLine( $"Frame load time: {FrameLoadTime}ms" );
				builder.AppendLine( $"Frame late diff: {TimeSpan.FromSeconds(Player.FrameLateDiff).TotalMilliseconds}ms" );
				builder.AppendLine( $"Playback time: {TimeSpan.FromSeconds(Player.PlaybackStopwatch?.Elapsed.TotalSeconds ?? 0):mm\\:ss}/{TimeSpan.FromSeconds(videoData.DurationDouble):mm\\:ss}" );

				//Throughput

				double playbackProgress = (double) Player.CurrentFrame / videoData.FrameCount;
				double frameRateFraction = 1 / videoData.FrameRate;
				
				double realDuration = videoData.FrameCount * frameRateFraction;
				double realProgress = playbackProgress * realDuration;

				builder.AppendLine( $"Real playback time: {TimeSpan.FromSeconds( realProgress ):mm\\:ss}/{TimeSpan.FromSeconds(realDuration):mm\\:ss}" );
				builder.AppendLine( $"Throughput: {Receiver.Throughput.ToSize(IntExtensions.SizeUnits.MB)}mb/s" );
			}

			DebugText.Text = builder.ToString();
		}

	}
}
