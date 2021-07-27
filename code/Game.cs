using Cinema.Entities;
using Cinema.Player;
using Cinema.UI;
using Sandbox;

namespace Cinema
{
	public partial class CinemaGame : Sandbox.Game
	{
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
			var ent = FindByIndex( playable );
			if ( ent != null && ent is TVEntity tvEntity )
			{
				tvEntity.RequestVideo(url);
			}
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
