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
	public class Item
	{
		private Vector2 pos;
		private Texture2D itemTexture;
		
		public int X { get { return (int)(0.5 + pos.X); } }
		public int Y { get { return (int)(0.5 + pos.Y); } }
		public static int Width { get { return Level.BlockSize; } }
		public static int Height { get { return Level.BlockSize; } }
		
		public enum Type
		{
			Food,
			Gold,
			Life,
		}
		private Type type;
		public Type ItemType { get { return type; } }
		
		private bool collected;
		public bool Collected { get { return collected; } }
		
		private int HitBorder { get { return 4; } }
		public HitBox HitBox { get { return new HitBox(X + HitBorder, Y + HitBorder, Width - HitBorder * 2, Height - HitBorder * 2); } }
		
		public class Temp
		{
			public int column;
			public int row;
			public Type type;
			
			public Temp(int column, int row, Type type)
			{
				this.column = column;
				this.row = row;
				this.type = type;
			}
		}
		
		public Item(Texture2D itemTexture, int column, int row, Type type)
		{
			pos = new Vector2((column + 1) * Level.BlockSize, Level.ScreenHeight - row * Level.BlockSize);
			this.itemTexture = itemTexture;
			this.type = type;
			collected = false;
		}
		
		public void Update(Player player)
		{
			if (!collected)
			{
				// see if the player collected the item
				HitBox overlap = this.HitBox.CheckCollision(player.HitBox);
				if (overlap.IsNotNull())
				{
					player.Collect(this);
					collected = true;
				}
			}
		}
		
		public void Draw(Camera camera)
		{
			if (!collected)
			{
				Rectangle source = new Rectangle((int)type * Width, 0, Width, Height);
				Rectangle destination = new Rectangle(X, Y, Width, Height);
				camera.Draw(itemTexture, source, destination);
			}
		}
	}
}