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
	public class Gobbler : Enemy
	{
		public override int Width { get { return 32; } }
		public override int Height { get { return 32; } }

		private int HitBorder { get { return 6; } }
		public override HitBox HitBox { get { return new HitBox(X + HitBorder, Y + HitBorder, Width - HitBorder * 2, Height - HitBorder); } }
		
		private int[] FrameOrder = { 0, 1, 2, 1 };
		private int FrameNum = 5;
		
		public enum SubType
		{
			Blue = 0,
			Green = 1,
			Yellow = 2,
			Purple = 3,
		}
		private SubType subType; 
		
		public Gobbler(Level level, Texture2D enemyTexture, int column, int row, int subType)
			: base(level, enemyTexture, column, row)
		{
			this.subType = (SubType)subType;
			frameX = 0;
			frameY = subType;

			// keep the Gobblers from imbedding in the ground
			pos.Y -= HitBorder / 2;
		}
		
		public override void Update(Player player)
		{
			base.Update(player);
			
			frameReversed = X + Width / 2 < player.X + Player.Width / 2;
			frameX = FrameOrder[(timeInState / FrameNum) % FrameOrder.Length];
		}
	}
}

