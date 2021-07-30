using System.Collections.Generic;
using Cinema.Entities;
using Cinema.Player;
using Cinema.UI;
using Sandbox;

namespace Cinema
{
	public partial class CinemaGame : Sandbox.Game
	{
		public List<ScreenPlayer> ScreenPlayers { get; set; } = new ();
		public List<WebSocket> WebSockets { get; set; } = new ();
		
		public CinemaGame()
		{
			if ( IsServer )
			{
				_ = new CinemaHud();
			}
		}
		
		[ServerCmd("request_video")]
		public static void RequestVideo( int playable, string url )
		{
			// var ent = FindByIndex( playable );
			// if ( ent != null && ent is TVEntity tvEntity )
			// {
			// 	tvEntity.RequestVideo(url);
			// }

			var game = Current as CinemaGame;
			game?.ScreenPlayers[0].RequestVideo(url);
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();
			
			CreateScreenPlayers();
		}

		private void CreateScreenPlayers()
		{
			foreach ( var entity in Entity.All )
			{
				if ( entity is ScreenPlayer screen )
				{
					ScreenPlayers.Add( screen );
				}
			}

			if ( ScreenPlayers.Count < 1 )
			{
				Log.Error( "This map is not supported by Cinema! (NO SCREEN PLAYERS FOUND)" );
			}
		}

		public override void Shutdown()
		{
			foreach ( var websocket in WebSockets )
			{
				websocket.Dispose();
			}
			
			base.Shutdown();
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );
			
			var player = new CinemaPlayer();
			client.Pawn = player;

			player.Respawn();
		}
	}
}
