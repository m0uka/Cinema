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
		
		public TVEntity SpawnedEntity { get; set; }

		public override async void Simulate( Client client )
		{
			base.Simulate( client );
			

			if ( IsClient && Input.Pressed( InputButton.Use ) )
			{
				using ( Prediction.Off() )
				{
					var trace = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 100000 )
						.Radius( 5 )
						.Ignore( this )
						.UseHitboxes()
						.Run();
					
					Log.Info( trace.Entity );
					
					if ( trace.Entity != null && trace.Entity is TVEntity tvEntity )
					{
						VideoRequestPanel.Instance.SetPlayer(tvEntity);
						
						return;
					}

				}
			}

			if ( IsServer && Input.Pressed( InputButton.Attack2 ) )
			{
				using ( Prediction.Off() )
				{
					var trace = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 100000 )
						.Radius( 5 )
						.Ignore( this )
						.UseHitboxes()
						.Run();

					if ( SpawnedEntity != null )
					{
						SpawnedEntity.Position = trace.EndPos;
						return;
					}
					
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
					model.SetupPhysicsFromModel( PhysicsMotionType.Invalid, false );

					if ( SpawnedEntity == null )
					{
						SpawnedEntity = model;
					}
				}
			}
		}
	}
}
