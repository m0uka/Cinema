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
			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			base.Respawn();
		}
		

		public override async void Simulate( Client client )
		{
			base.Simulate( client );
			

			if ( IsServer && Input.Pressed( InputButton.Drop ) )
			{
				
			}

			if ( IsServer && Input.Pressed( InputButton.Attack2 ) )
			{
				var trace = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 1000 )
					.Radius( 5 )
					.Ignore( this )
					.Run();

				using ( Prediction.Off() )
				{
					if ( trace.Entity != null && trace.Entity is TVEntity tvEntity )
					{
						if ( tvEntity.Player.IsPlaying )
						{
							tvEntity.Player.Stop();
						}
						else
						{
							tvEntity.Player.Play();
						}
						
						return;
					}
					
					var model = new TVEntity();
					model.SetModel( "models/tv.vmdl" );
					model.Position = trace.EndPos;
					model.Spawn();
					model.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

					// Wait for receiver socket
					await Task.Delay( 1000 );
					model.RequestVideo( "https://www.youtube.com/watch?v=P_nj6wW6Gsc" );
				}
			}
		}
	}
}
