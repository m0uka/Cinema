using System;
using System.IO;
using System.Text.Json;
using Cinema.UI;
using Cinema.Video;
using Sandbox;

namespace Cinema.Entities
{
	[Library("cinema_screen")]
	[Hammer.Model]
	public partial class ScreenPlayer : ModelEntity, IPlayable
	{
		public VideoPlayer Player { get; set; }
		private VideoReceiver Receiver { get; set; }
		
		[Net] public bool IsVideoPlaying { get; set; }
		[Net] public string VideoId { get; set; }
		[Net] public TimeSince VideoStart { get; set; }

		public ScreenPlayer()
		{
			SetupPhysicsFromModel( PhysicsMotionType.Static );
			
			if ( IsClient )
			{
				InitializePlayer();
				InitializeReceiver();
				UpdateFrames();

				if ( IsVideoPlaying && !Player.IsPlaying )
				{
					Play( VideoId );
				}
			}

			if ( IsServer )
			{
				InitializeServerReceiver();
			}
		}

		private async void InitializePlayer()
		{
			Player = new VideoPlayer();

			await Task.Delay( 1000 );
			SceneObject.SetValue("tint", Color.White);
			
			VideoStreamPanel.Instance.Player = Player;
			VideoStreamPanel.Instance.Receiver = Receiver;
		}

		private async void InitializeServerReceiver()
		{
			Receiver = new VideoReceiver( (id) =>
			{
				VideoId = id;
				VideoStart = 0;
				IsVideoPlaying = true;
				Play( id );
			}, OnVideoProgress );
			
            await Receiver.CreateSocket();
		}

		private async void InitializeReceiver()
		{
			Receiver = new VideoReceiver( Player );
			await Receiver.CreateSocket();
		}

		[Event.Frame]
		private void Playback()
		{
			Player?.Playback();
			
			if ( Player?.ActiveTexture != null )
			{
				SceneObject?.SetValue( "screen", Player.ActiveTexture );
			}
		}

		[Event.Frame]
		private void UpdateFrames()
		{
			Player?.UpdateFrames();
		}

		[ClientRpc]
		public void Play( string id )
		{
			Log.Info( $"Playing the video! {id}" );
			Receiver.JoinVideoStream( id );
		}
		
		public void RequestVideo(string url)
		{
			Host.AssertServer();
			
			Log.Info( $"Requested a video, URL: {url}" );
			Receiver.RequestVideo(url);
		}

		[ClientRpc]
		private void OnVideoProgress(string jsonProgress)
		{
			Player.VideoProgress = JsonSerializer.Deserialize<VideoProgress>(jsonProgress);
		}
	}
}
