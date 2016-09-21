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
	public class Controls
	{
		private bool left = false;
		private bool right = false;
		private bool up = false;
		private bool down = false;
		private bool run = false;
		private bool jump = false;
		private bool attack = false;
		private bool pause = false;

		public bool Left { get { return left; } }
		public bool Right { get { return right; } }
		public bool Up { get { return up; } }
		public bool Down { get { return down; } }
		public bool Run { get { return run; } }
		public bool Jump { get { return jump; } }
		public bool Attack { get { return attack; } }
		public bool Pause { get { return pause; } }

		public Controls(GamePadState gamePad, KeyboardState keyboard)
		{
			if (gamePad.ThumbSticks.Left.X < -0.5
				|| gamePad.DPad.Left == ButtonState.Pressed
				|| keyboard.IsKeyDown(Keys.Left))
				left = true;
			if (gamePad.ThumbSticks.Left.X > 0.5
				|| gamePad.DPad.Right == ButtonState.Pressed
				|| keyboard.IsKeyDown(Keys.Right))
				right = true;
			if (gamePad.ThumbSticks.Left.Y < -0.5
				|| gamePad.DPad.Up == ButtonState.Pressed
				|| keyboard.IsKeyDown(Keys.Up))
				up = true;
			if (gamePad.ThumbSticks.Left.Y > 0.5
				|| gamePad.DPad.Down == ButtonState.Pressed
				|| keyboard.IsKeyDown(Keys.Down))
				down = true;
			if (gamePad.Triggers.Left > 0.5
				|| gamePad.Triggers.Right > 0.5
				|| keyboard.IsKeyDown(Keys.LeftShift)
				|| keyboard.IsKeyDown(Keys.RightShift))
				run = true;
			if (gamePad.Buttons.A == ButtonState.Pressed
				|| keyboard.IsKeyDown(Keys.X))
				jump = true;
			if (gamePad.Buttons.B == ButtonState.Pressed
				|| gamePad.Buttons.X == ButtonState.Pressed
				|| keyboard.IsKeyDown(Keys.Z))
				attack = true;
			if (gamePad.Buttons.Start == ButtonState.Pressed
				|| keyboard.IsKeyDown(Keys.Space))
				pause = true;
		}
	}
}