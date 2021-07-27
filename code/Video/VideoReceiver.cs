using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cinema.UI;
using Sandbox;

namespace Cinema.Video
{
	public class VideoReceiver
	{
		public Action<string[]> MessageReceived { get; set; }
		public Action<string> StreamSuccess { get; set; }
		public Action<string> VideoProgress { get; set; }
		
		public VideoPlayer VideoPlayer { get; set; }
		
		private WebSocket WebSocket { get; set; }

		private Dictionary<DateTime, int> ThroughputData { get; set; } = new();

		public int Throughput
		{
			get
			{
				return ThroughputData.Sum( x => x.Value );
			}
		}

		public VideoReceiver( VideoPlayer player )
		{
			VideoPlayer = player;
		}
		
		public VideoReceiver(Action<string> onStreamSuccess, Action<string> onVideoProgress)
		{
			StreamSuccess = onStreamSuccess;
			VideoProgress = onVideoProgress;
		}

		private byte[] MergeBytes(List<byte[]> fragments)
		{
			var output = new byte[fragments.Sum(arr=>arr.Length)];
			int writeIdx=0;
			foreach(var byteArr in fragments) {
				byteArr.CopyTo(output, writeIdx);
				writeIdx += byteArr.Length;
			}

			return output;
		}

		private void CalculateThroughput(int length)
		{
			var now = DateTime.Now;
			if ( ThroughputData.ContainsKey( now ) )
			{
				ThroughputData[now] += length;
			}
				
			ThroughputData[now] = length;
			foreach ( var i in ThroughputData.Where( x => x.Key < DateTime.Now - TimeSpan.FromSeconds( 1 ) )
				.ToList() )
			{
				ThroughputData.Remove( i.Key );
			}
		}
		
		/// <summary>
		/// Inits a websocket connection
		/// </summary>
		public async Task CreateSocket()
		{
			WebSocket = new WebSocket();
			await WebSocket.Connect( "ws://m0uka.dev:8181" );

			if ( WebSocket.IsConnected )
			{
				Log.Info( "Connected to Cinema Backend!" );
			}
			else
			{
				Log.Error( "Couldn't connect to Cinema Backend!" );
				return;
			}

			bool activeFragment = false;
			List<byte[]> frameFragments = new List<byte[]>();
			WebSocket.OnDataReceived += (data) =>
			{
				if ( activeFragment )
				{
					if ( data.Length > 0 && data[0] == 0xFE )
					{
						activeFragment = false;

						byte[] fragment = MergeBytes( frameFragments );

						VideoPlayer.AddFrame(fragment);
						frameFragments.Clear();
						return;
					}
					
					frameFragments.Add( data.ToArray() );
				}
				else
				{
					// Check if data is JPEG
					if ( data.Length > 1 && data[0] == 0xFF && data[1] == 0xD8 )
					{
						VideoPlayer.AddFrame( data.ToArray() );
					}
					else if ( data.Length > 0 && data[0] == 0xAA )
					{
						// start of fragment
						activeFragment = true;
					}
				}
				
				CalculateThroughput( data.Length );
			};

			WebSocket.OnMessageReceived += ( msg ) =>
			{
				string[] messageSplit = msg.Split( " " );
				string msgId = messageSplit[0];

				MessageReceived?.Invoke( messageSplit );

				if ( msgId == "convert_success" )
				{
					string id = messageSplit[1];
					StreamSuccess( id );
				}

				if ( msgId == "error" )
				{
					string error = string.Join( " ", messageSplit.Skip( 1 ) );
					Log.Error( error );
				}
				
				if ( msgId == "video_progress" )
				{
					string rest = string.Join( " ", messageSplit.Skip( 1 ));
					VideoProgress( rest );
				}
				
				if ( msgId == "stream_end" )
				{
					VideoPlayer.IsStreaming = false;
				}

				if ( msgId == "join_success" )
				{
					string rest = string.Join( " ", messageSplit.Skip( 1 ));
					var video = JsonSerializer.Deserialize<VideoData>(rest);
					
					Log.Info( "Successfully joined, starting!" );
					VideoPlayer.VideoData = video;
					VideoPlayer.Play();
				}
				
				
			};
		}

		/// <summary>
		/// Request a video (serverside)
		/// </summary>
		public void RequestVideo(string url)
		{
			if ( !WebSocket.IsConnected ) return;
			WebSocket.Send( $"stream_request {url}" );
		}
		
		/// <summary>
		/// Join video stream (clientside)
		/// </summary>
		public void JoinVideoStream(string id)
		{
			if ( !WebSocket.IsConnected ) return;
			WebSocket.Send( $"stream_join {id}" );
		}
	}
}
