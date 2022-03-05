using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Cinema.Entities;
using Cinema.UI;
using Cinema.Video;
using Sandbox;
using Sandbox.UI;

namespace Cinema.Player
{
	public partial class CinemaPlayer : Sandbox.Player
	{
		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );
			
			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			CameraMode = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			base.Respawn();
		}
		

		public override void Simulate( Client client )
		{
			base.Simulate( client );
			

			if ( IsClient && Input.Pressed( InputButton.Use ) )
			{
				using ( Prediction.Off() )
				{
					VideoRequestPanel.Instance.SetPlayer();
					
					// var trace = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 100000 )
					// 	.Radius( 5 )
					// 	.Ignore( this )
					// 	.UseHitboxes()
					// 	.Run();
					//
					// Log.Info( trace.Entity );
					//
					// if ( trace.Entity != null && trace.Entity is TVEntity tvEntity )
					// {
					// 	VideoRequestPanel.Instance.SetPlayer(tvEntity);
					// 	
					// 	return;
					// }

				}
			}
			//
			// if ( IsClient && Input.Pressed( InputButton.Attack2 ) )
			// {
			// 	using ( Prediction.Off() )
			// 	{
			// 		var panel = new ScreenWorldPanel();
			// 		panel.Position = client.Pawn.Position;
			// 	}
			// }
		}
	}
}
