using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cinema.UI;
using Cinema.Video;
using Sandbox;
using Sandbox.UI;

namespace Cinema.Entities
{
	[Library("cinema_screenplayer")]
	[Display( Name= "Cinema Screen Player" )]
	[SandboxEditor.Model]
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
			Player = new VideoPlayer( this );

			await Task.Delay( 1000 );
			
			SceneObject.Attributes.Set("tint", Color.White);

			VideoStreamPanel.Instance.Player = Player;
			VideoStreamPanel.Instance.Receiver = Receiver;
		}

		private async void InitializeServerReceiver()
		{
			Receiver = new VideoReceiver( async (id) =>
			{
				VideoId = id;
				VideoStart = 0;
				IsVideoPlaying = true;
				
				foreach ( var player in Client.All )
				{
					try
					{
						await GameServices.SubmitScore( player.PlayerId, 1 );
					}
					catch ( Exception e )
					{
						Log.Warning($"Failed to submit score to backend, error: {e.Message}");
					}
				}

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
				SceneObject?.Attributes.Set( "screen", Player.ActiveTexture );
			}
		}

		[Event.Frame]
		private void UpdateFrames()
		{
			Player?.UpdateFrames();
			Receiver?.Update();
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
