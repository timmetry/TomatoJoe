using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TomatoJoe
{
	public class Camera
	{
		private Rectangle bounds;

		private Level level;
		private SpriteBatch batch;
		
		public int Width { get { return bounds.Width; } }
		public int Height { get { return bounds.Height; } }

		public static int ZoomRate { get { return 3; } }

		private float pan;
		private float panMov;
		private float PanSpeedSlow { get { return 0.25f; } }
		private float PanSpeedNormal { get { return 0.5f; } }
		private float PanSpeedFast { get { return 2f; } }
		private float PanAccelSlow { get { return 0.05f; } }
		private float PanAccelNormal { get { return 0.1f; } }
		private float PanAccelFast { get { return 0.4f; } }
		private float PanDistanceForSlowReturn { get { return 60f; } }
		private float MaxPan { get { return bounds.Width / 4; } }

		public Camera(SpriteBatch batch, Level level)
		{
			this.batch = batch;
			this.level = level;
			this.bounds = new Rectangle(0, 0, Level.ScreenWidth, Level.ScreenHeight);
			pan = 0;
		}

		public void Draw(Texture2D texture, Rectangle source, Rectangle destination)
		{
			Draw(texture, source, destination, SpriteEffects.None);
		}

		public void Draw(Texture2D texture, Rectangle source, Rectangle destination, SpriteEffects effects)
		{
			if (destination.Right > bounds.Left && destination.Left < bounds.Right
				&& destination.Bottom > bounds.Top && destination.Top < bounds.Bottom)
			{
				destination.X -= bounds.X;
				destination.Y -= bounds.Y;
				destination.X *= ZoomRate;
				destination.Y *= ZoomRate;
				destination.Width *= ZoomRate;
				destination.Height *= ZoomRate;
				batch.Draw(texture, destination, source, Color.White, 0, Vector2.Zero, effects, 0);
			}
		}
		
		public void DrawRelative(Texture2D texture, Rectangle source, Rectangle destination)
		{
			destination.X += bounds.X;
			destination.Y += bounds.Y;
			Draw(texture, source, destination);
		}
		
		public void Update(Player player)
		{
			// is the player moving right at a steady pace, with the camera not pointed behind him?
			if (player.Speed > player.MaxWalkSpeed && pan >= 0)
			{
				// slowly pan the camera more to the right
				panMov += PanAccelNormal;
				if (panMov > PanSpeedNormal)
					panMov = PanSpeedNormal;
			}
			// is the player moving left at a steady pace, with the camera not pointed behind him?
			else if (player.Speed < -player.MaxWalkSpeed && pan <= 0)
			{
				// slowly pan the camera more to the left
				panMov -= PanAccelNormal;
				if (panMov < -PanSpeedNormal)
					panMov = -PanSpeedNormal;
			}
			// is the player slowing down, stopped, or is the camera pointed behind him?
			// if so, we want to quickly pan the camera back to the center
			else
			{
				if (pan > 0)
				{
					// is the camera panned far to the right, or is joe running the opposite way?
					if (pan > PanDistanceForSlowReturn || player.Speed < -player.MaxWalkSpeed)
					{
						// we want to quickly return the pan to the center
						panMov -= PanAccelFast;
						if (panMov < -PanSpeedFast)
							panMov = -PanSpeedFast;
					}
					// is the camera close to centering on Joe, while he's staying still?
					else if (panMov < -PanSpeedSlow)
					{
						// we want to slow down the panning
						panMov += PanAccelSlow;
						if (panMov > -PanSpeedSlow)
							panMov = -PanSpeedSlow;
					}
					// has the camera finished centering on Joe, while he's staying still?
					else if (pan + panMov <= 0)
					{
						// stop panning the camera
						panMov = 0;
						pan = 0;
					}
				}
				else if (pan < 0)
				{
					// is the camera panned far to the left, or is joe running the opposite way?
					if (pan < -PanDistanceForSlowReturn || player.Speed > player.MaxWalkSpeed)
					{
						// we want to quickly return the pan to the center
						panMov += PanAccelFast;
						if (panMov > PanSpeedFast)
							panMov = PanSpeedFast;
					}
					// is the camera close to centering on Joe, while he's staying still?
					else if (panMov > PanSpeedSlow)
					{
						// we want to slow down the panning
						panMov -= PanAccelSlow;
						if (panMov < PanSpeedSlow)
							panMov = PanSpeedSlow;
					}
					// has the camera finished centering on Joe, while he's staying still?
					else if (pan + panMov >= 0)
					{
						// stop panning the camera
						panMov = 0;
						pan = 0;
					}
				}
			}
			// continue panning the camera (unless Joe is dead)
			if (!player.IsDying())
				pan += panMov;
			// make sure it doesn't pan too far
			if (pan > MaxPan)
				pan = MaxPan;
			if (pan < -MaxPan)
				pan = -MaxPan;

			// follow the player with the camera
			bounds.X = player.X - bounds.Width / 2 + Player.Width;
			// pan the camera to its current location based on Joe
			bounds.X += (int)(0.5 + pan);
			
			// dont let the camera pan beyond the boundaries of the level
			if (bounds.X < 0)
				bounds.X = 0;
			if (bounds.X > level.Bounds.Width - bounds.Width)
				bounds.X = level.Bounds.Width - bounds.Width;
		}
	}
}