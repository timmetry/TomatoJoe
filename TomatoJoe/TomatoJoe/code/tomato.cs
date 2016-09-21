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
	public class Tomato : Weapon
	{
		public override bool IsEnemyWeapon { get { return false; } }
		public override bool IsSolidWeapon { get { return true; } }

		public override int Width { get { return 16; } }
		public override int Height { get { return 16; } }

		protected override int FinalFrame { get { return 3; } }
		protected override float Gravity { get { return 0.1f; } }

		private int splatTime;
		private int burstFrame;
		private int MaxSplatTime { get { return 150; } }
		public bool Finished { get { return finished; } }
		public bool Active { get { return !finished && splatTime == 0 && burstFrame > FinalFrame; } }

		protected override int HitBorder { get { return 4; } }
		public override HitBox HitBox
		{
			get
			{
				return new HitBox(X + HitBorder, Y + HitBorder,
					Width - HitBorder * 2, Height - HitBorder * 2);
			}
		}
		
		public Tomato(Level level, Texture2D tomatoTexture, float posX, float posY, float movX, float movY)
			: base(level, tomatoTexture, posX, posY, movX, movY)
		{
			splatTime = 0;
			burstFrame = FinalFrame + 1;
		}
		
		public override void Update()
		{
			if (burstFrame <= FinalFrame)
			{
				++burstFrame;
				if (burstFrame > FinalFrame && splatTime < 2)
					finished = true;
			}
		
			if (splatTime > 0)
			{
				if (!finished)
				{
					--splatTime;
					if (splatTime < 2)
						finished = true;
				}
			}
			else
			{
				base.Update();

			}
		}
		
		public void Draw(Camera camera)
		{
			if (Active || splatTime > 1)
			{
				Rectangle source = new Rectangle(frame * Width, 0, Width, Height);
				Rectangle destination = new Rectangle(X, Y, Width, Height);
				
				if (splatTime > 0)
					source.Y = 2 * Height;
				
				camera.Draw(texture, source, destination);
			}
			
			if (burstFrame <= FinalFrame)
			{
				Rectangle source = new Rectangle(burstFrame * Width, Height, Width, Height);
				Rectangle destination = new Rectangle(X, Y, Width, Height);
				camera.Draw(texture, source, destination);
			}
		}
	}
}