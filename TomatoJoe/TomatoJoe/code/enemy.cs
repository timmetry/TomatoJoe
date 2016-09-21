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
	public abstract class Enemy
	{
		protected Level level;
		protected Texture2D enemyTexture;
		protected Vector2 pos;
		protected Vector2 mov;
		protected int frameX;
		protected int frameY;
		protected bool frameReversed;
		protected bool isDying;
		protected bool isDead;
		protected int timeInState;
		
		public int X { get { return (int)(0.5 + pos.X); } }
		public int Y { get { return (int)(0.5 + pos.Y); } }
		public abstract int Width { get; }
		public abstract int Height { get; }
		
		public bool IsDying { get { return isDying; } }
		public bool IsDead { get { return isDead; } }
		public abstract HitBox HitBox { get; }
		
		public enum Type
		{
			Gobbler,
//			Frowney,
//			Boobley,
//			Copter,
		}
		
		public class Temp
		{
			public int column;
			public int row;
			public Type type;
			public int subType;
			
			public Temp(int column, int row, Type type, int subType)
			{
				this.column = column;
				this.row = row;
				this.type = type;
				this.subType = subType;
			}
		}
		
		public Enemy(Level level, Texture2D enemyTexture, int column, int row)
		{
			this.level = level;
			this.enemyTexture = enemyTexture;
			pos = new Vector2((column + 1) * Level.BlockSize + Level.BlockSize / 2 - Width / 2, 
				Level.ScreenHeight - row * Level.BlockSize + Level.BlockSize / 2 - Height / 2);
			
			isDying = false;
			timeInState = 0;
		}
		
		public static Enemy Create(Game game, Level level, Temp temp)
		{
			switch (temp.type)
			{
				case Type.Gobbler:
					return new Gobbler(level, game.gobblerTexture, temp.column, temp.row, temp.subType);
				default:
					throw new Exception("ERROR: Unknown Enemy Type");
			}
		}
		
		public virtual void Update(Player player)
		{
			++timeInState;

			if (IsDying)
			{
				if (Y > level.Bounds.Bottom)
					isDead = true;
			}
			else
			{
				// see if the enemy got hit by a tomato
				//TODO

				// see if the player got hit by the enemy
				HitBox overlap = this.HitBox.CheckCollision(player.HitBox);
				if (overlap.IsNotNull())
				{
					// the player dies
					player.KillSlowly();
				}
			}
		}
		
		public virtual void Draw(Camera camera)
		{
			Rectangle source = new Rectangle(frameX * Width, frameY * Height, Width, Height);
			Rectangle destination = new Rectangle(X, Y, Width, Height);

			SpriteEffects effects = SpriteEffects.None;
			if (frameReversed)
				effects |= SpriteEffects.FlipHorizontally;
			if (IsDying)
				effects |= SpriteEffects.FlipVertically;
			camera.Draw(enemyTexture, source, destination, effects);
		}
	}
}

