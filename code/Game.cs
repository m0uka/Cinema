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
		
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );
			
			var player = new CinemaPlayer();
			client.Pawn = player;

			player.Respawn();
		}
	}
}
