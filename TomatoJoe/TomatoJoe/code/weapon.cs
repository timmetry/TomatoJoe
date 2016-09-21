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
	public abstract class Weapon
	{
		protected Level level;
		protected Texture2D texture;
		protected Vector2 pos;
		protected Vector2 mov;
		protected int frame;
		protected bool finished;

		public abstract bool IsEnemyWeapon { get; }
		public abstract bool IsSolidWeapon { get; }

		public int X { get { return (int)(0.5 + pos.X); } }
		public int Y { get { return (int)(0.5 + pos.Y); } }
		public abstract int Width { get; }
		public abstract int Height { get; }

		protected abstract int FinalFrame { get; }
		protected abstract float Gravity { get; }

		protected abstract int HitBorder { get; }
		public abstract HitBox HitBox { get; }

		public Weapon(Level level, Texture2D texture, float posX, float posY, float movX, float movY)
		{
			this.level = level;
			this.texture = texture;
			pos = new Vector2(posX, posY);
			mov = new Vector2(movX, movY);
			if (movX < 0)
				frame = 0;
			else
				frame = 1;
			finished = false;
		}

		public virtual void Update()
		{
			if (HitBox.Left > level.Bounds.Right || HitBox.Top > level.Bounds.Bottom)
				finished = true;

			pos.X += mov.X;
			HitBox overlap = level.CheckCollisionSolid(this.HitBox);
			if (mov.X < 0 && overlap.Left != 0)
			{
				frame = 0;
				pos.X -= overlap.Left + Width / 4;
				if (overlap.Bottom < Height - HitBorder)
					pos.Y += Height - HitBorder - overlap.Bottom;
				burstFrame = 0;
				splatTime = MaxSplatTime;
				mov = Vector2.Zero;
			}
			else if (mov.X > 0 && overlap.Right != 0)
			{
				frame = 3;
				pos.X -= overlap.Right - Width / 4;
				if (overlap.Bottom < Height - HitBorder)
					pos.Y += Height - HitBorder - overlap.Bottom;
				burstFrame = 0;
				splatTime = MaxSplatTime;
				mov = Vector2.Zero;
			}
			mov.Y += Gravity;
			pos.Y += mov.Y;
			overlap = level.CheckCollisionSolid(this.HitBox);
			if (overlap.Bottom != 0 && splatTime != MaxSplatTime)
			{
				if (mov.X < 0)
					frame = 1;
				else
					frame = 2;
				pos.Y -= overlap.Bottom - Height / 4;
				if (-overlap.Left < Width - HitBorder)
					pos.X -= Width - HitBorder + overlap.Left;
				if (overlap.Right < Width - HitBorder)
					pos.X += Width - HitBorder - overlap.Right;
				burstFrame = 0;
				splatTime = MaxSplatTime;
				mov = Vector2.Zero;
			}

			if (mov.X < 0)
			{
				--frame;
				if (frame < 0)
					frame = FinalFrame;
			}
			else
			{
				++frame;
				if (frame > FinalFrame)
					frame = 0;
			}
		}
	}
}