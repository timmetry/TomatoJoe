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
	public class HitBox
	{
		protected Vector2 pos;
		protected Vector2 size;

		public Vector2 Pos { get { return pos; } set { pos = value; } }
		public Vector2 Size { get { return size; } set { size = value; } }
		public float X { get { return pos.X; } set { pos.X = value; } }
		public float Y { get { return pos.Y; } set { pos.Y = value; } }
		public float Width { get { return size.X; } set { size.X = value; } }
		public float Height { get { return size.Y; } set { size.Y = value; } }
		public float Left { get { return X; } set { X = value; } }
		public float Right { get { return X + Width; } set { Width = value - X; } }
		public float Top { get { return Y; } set { Y = value; } }
		public float Bottom { get { return Y + Height; } set { Height = value - Y; } }

		public HitBox()
		{
			pos = Vector2.Zero;
			size = Vector2.Zero;
		}

		public HitBox(float x, float y, float Width, float Height)
		{
			pos = new Vector2(x, y);
			size = new Vector2(Width, Height);
		}

		public HitBox(Vector2 pos, Vector2 size)
		{
			this.pos = pos;
			this.size = size;
		}

		public HitBox(Rectangle rect)
		{
			pos = new Vector2(rect.X, rect.Y);
			size = new Vector2(rect.Width, rect.Height);
		}

		public bool IsNotNull()
		{
			return Left != 0 && Right != 0 && Width != 0 && Height != 0;
		}

		public HitBox CheckCollision(HitBox hitbox2)
		{
			if (Left < hitbox2.Right && Right > hitbox2.Left && Top < hitbox2.Bottom && Bottom > hitbox2.Top)
			{
				HitBox overlap = new HitBox();
				overlap.Left = Left - hitbox2.Right;
				overlap.Right = Right - hitbox2.Left;
				overlap.Top = Top - hitbox2.Bottom;
				overlap.Bottom = Bottom - hitbox2.Top;
				return overlap;
			}
			else
				return Zero;
		}

		public static HitBox Zero { get { return new HitBox(); } }
	}
}