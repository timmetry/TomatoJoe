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
	public class Player
	{
		private Level level;
		private Texture2D joeTexture;
		private Texture2D tomatoTexture;
		private Vector2 pos;
		private Vector2 mov;
		private int frameX;
		private int frameY;
		private bool frameReversed;
		private bool onIce;
		private bool jumpPressedLastFrame;
		
		private int[] WalkingFrameOrder = { 0, 1, 0, 3 };
		private int[] RunningFrameOrder = { 0, 1, 2, 1, 0, 3, 4, 3 };
		private int[] SprintingFrameOrder = { 0, 1, 2, 1, 0, 3, 4, 3 };
		private int WalkingFrameNum { get { return 5; } }
		private int RunningFrameNum { get { return 5; } }
		private int SprintingFrameNum { get { return 8; } }
		private int JumpingFrame { get { return 2; } }
		private int FallingFrame { get { return 2; } }
		private int AttackFrameFinal { get { return 3; } }
		private int AttackFrameToStartThrow { get { return 3; } }

		public int X { get { return (int)(0.5 + pos.X); } }
		public int Y { get { return (int)(0.5 + pos.Y); } }
		public float Speed { get { return mov.X; } }
		public static int Width { get { return 24; } }
		public static int Height { get { return 32; } }

		/////////////////////////////////////
		//* New Horizontal Movement Method
		// ^ (remove one slash to change to old horizontal movement method)
		public float MaxWalkSpeed { get { return 1.5f; } } // speed for using stepping (shorter) animation
		public float MaxJogSpeed { get { return 2.0f; } } // maximum speed if not holding Run button
		public float MaxRunSpeed { get { return 2.5f; } } // maximum speed if holding Run button and not in Sprint mode
		public float MaxSprintSpeed { get { return 3.0f; } } // maximum speed while in Sprint mode
		public float Accel { get { if (onIce) return IceAccel; else return 0.2f; } }
		public float Decel { get { if (onIce) return IceDecel; else return 0.1f; } }
		public float IceAccel { get { return 0.05f; } }
		public float IceDecel { get { return 0.01f; } }
		
		private bool IsSprinting { get { return mov.X < -MaxRunSpeed || mov.X > MaxRunSpeed; } }
		private bool IsRunning { get { return mov.X < -MaxJogSpeed || mov.X > MaxJogSpeed; } }
		private bool IsJogging { get { return mov.X < -MaxWalkSpeed || mov.X > MaxWalkSpeed; } }
		private bool IsWalking { get { return mov.X < 0 || mov.X > 0; } }
		
		private int StandJumpHeight { get { return 14; } }
		private int WalkJumpHeight { get { return 14; } }
		private int JogJumpHeight { get { return 14; } }
		private int RunJumpHeight { get { return 16; } }
		private int SprintJumpHeight { get { return 20; } }

		private int JumpHeight
		{
			get
			{
				if (IsSprinting) return SprintJumpHeight;
				if (IsRunning) return RunJumpHeight;
				if (IsJogging) return JogJumpHeight;
				if (IsWalking) return WalkJumpHeight;
				return StandJumpHeight;
			}
		}
		
		public void MoveHorz(Controls controls, int lean) { MoveHorzNew(controls, lean); }
		
		private int sprintTimer = 0;
		private int SprintChargeTime { get { return 120; } }
		private int SprintChargeMax { get { return 240; } }
		private int SprintTimeAccel { get { return 2; } }
		private int SprintTimeDecel { get { return 1; } }
		public bool SprintModeOn { get { return sprintTimer >= SprintChargeTime; } }
		
		public void UpdateSprintTimer(Controls controls, int lean)
		{
			if (lean != 0 && controls.Run)
			{
				sprintTimer += SprintTimeAccel;
				if (sprintTimer >= SprintChargeTime) sprintTimer = SprintChargeMax;
			}
			else
			{
				sprintTimer -= SprintTimeDecel;
				if (sprintTimer < SprintChargeTime) sprintTimer = 0;
			}
		}
		/////////////////////////////////////
		/*/ // Old Horizontal Movement Method
		public float MaxWalkSpeed { get { return 1.2f; } }
		public float MaxRunSpeed { get { return 2.4f; } }
		public float MaxSprintSpeed { get { return 3.6f; } }
		private float IceAccel { get { return 0.05f; } }
		private float IceDecel { get { return 0.01f; } }
		private float WalkAccel { get { if (onIce) return IceAccel; else return 0.2f; } }
		private float WalkDecel { get { if (onIce) return IceDecel; else return 0.1f; } }
		private float RunAccel { get { return 0.04f; } }
		private float RunDecel { get { return 0.02f; } }
		private float SprintAccel { get { return 0.01f; } }
		private float SprintDecel { get { return 0.005f; } }
		
		private bool IsStandSpeed { get { return mov.X > -MaxWalkSpeed && mov.X < MaxWalkSpeed; } }
		private bool IsWalkSpeed { get { return mov.X > -MaxRunSpeed && mov.X < MaxRunSpeed && !IsStandSpeed; } }
		private bool IsRunSpeed { get { return mov.X > -MaxSprintSpeed && mov.X < MaxSprintSpeed && !IsWalkSpeed; } }
		private bool IsSprintSpeed { get { return mov.X <= -MaxSprintSpeed && mov.X >= MaxSprintSpeed; } }
		
		private int StandJumpHeight { get { return 14; } }
		private int WalkJumpHeight { get { return 14; } }
		private int RunJumpHeight { get { return 16; } }
		private int SprintJumpHeight { get { return 20; } }

		private int JumpHeight
		{
			get
			{
				if (IsStandSpeed) return StandJumpHeight;
				if (IsWalkSpeed) return WalkJumpHeight;
				if (IsRunSpeed) return RunJumpHeight;
				return SprintJumpHeight;
			}
		}

		private float Decel
		{
			get
			{
				if (IsRunSpeed) return RunDecel;
				if (IsSprintSpeed) return SprintDecel;
				return WalkDecel;
			}
		}
		
		public void MoveHorz(Controls controls, int lean) { MoveHorzOld(controls, lean); }
		//*/
		/////////////////////////////////////
		
		private float Gravity { get { return 0.5f; } }
		private float JumpSpeed { get { return 4.0f; } }
		private float FallSpeed { get { return 4.0f; } }

		private int EarlyJumpFrames { get { return 6; } }
		private int LateJumpFrames { get { return 6; } }
		private int earlyJumpTimer;
		private int lateJumpTimer;
		
		public HitBox HitBox { get { return new HitBox(pos.X + 3, pos.Y + 4, Width - 6, Height - 4); } }

		public enum State
		{
			Standing,
			Walking,
			Running,
			Jumping,
			Falling,
			Dying,
			Dead,
			Sprinting,
		}
		private State state;
		private int timeInState;
		
		private int health;
		public int Health { get { return health; } }
		public int StartingHealth { get { return 1200; } }
		public int MaxHealth { get { return 1500; } }
		public int HealAmount { get { return 300; } }
		private int DeathTime { get { return 80; } }
		
		public void Collect(Item item)
		{
			switch (item.ItemType)
			{
				case Item.Type.Food:
					health += HealAmount;
					if (health > MaxHealth)
						health = MaxHealth;
					break;
				case Item.Type.Gold:
					health = MaxHealth;
					break;
				case Item.Type.Life:
					level.ExtraLife();
					break;
			}
		}

		private bool attacking;
		private bool attackPressedLastFrame;
		private bool CanAttack { get { return !attacking && numTomatoes < MaxTomatoes; } }
		
		private List<Tomato> tomatoes;
		private int numTomatoes;
		private int MaxTomatoes { get { return 2; } }
		private float TomatoSpeed { get { if (frameReversed) return mov.X - MaxRunSpeed; else return mov.X + MaxRunSpeed; } }
		private int TomatoOffsetX { get { return 12; } }
		private int TomatoOffsetY { get { return -2; } }
		
		public void KillSlowly()
		{
			// only kill the player if he's not already dead
			if (!IsDying())
			{
				state = State.Dying;
				timeInState = 0;
			}
		}
		public void KillQuickly()
		{
			// only kill the player if he's not already dead
			if (!IsDying())
			{
				state = State.Dead;
				timeInState = 0;
			}
		}
		public bool IsDying()
		{
			return state == State.Dead || state == State.Dying;
		}

		public Player(Level level, Texture2D joeTexture, Texture2D tomatoTexture, float x, float y)
		{
			this.level = level;
			this.joeTexture = joeTexture;
			this.tomatoTexture = tomatoTexture;
			this.pos = new Vector2(x, y);
			this.mov = new Vector2(0, 0);
			frameX = 0;
			frameY = 0;
			frameReversed = false;
			state = State.Standing;
			timeInState = 0;
			attacking = false;
			onIce = false;
			jumpPressedLastFrame = true;
			attackPressedLastFrame = true;
			health = StartingHealth;
			tomatoes = new List<Tomato>();
			numTomatoes = 0;
			earlyJumpTimer = 0;
			lateJumpTimer = 0;
		}
		
		public void MoveHorzNew(Controls controls, int lean)
		{
			///////////////////////////////////////////////////////////////////////////////////////////
			// this is the new method for moving the player horizontally
			// it features a very quick acceleration to normal running speed,
			// with a timer that immediately activates Sprinting speed after a few seconds
			///////////////////////////////////////////////////////////////////////////////////////////
			UpdateSprintTimer(controls, lean);
			
			float maxSpeed = 0;
			if (lean != 0)
			{
				maxSpeed = MaxJogSpeed;
				if (controls.Run)
				{
					maxSpeed = MaxRunSpeed;
					if (SprintModeOn)
						maxSpeed = MaxSprintSpeed;
				}
			}
			
			// is the player speeding up?
			if (lean != 0 && mov.X * lean < maxSpeed)
			{
				// speed the player up
				mov.X += lean * Accel;
				if (mov.X > maxSpeed * lean)
					mov.X = maxSpeed * lean;
			}
			// is the player slowing down?
			else
			{
				// slow the player down
				int curDir = mov.X < 0 ? -1 : 1;
				mov.X -= Decel * curDir;
				if (mov.X < maxSpeed * curDir)
					mov.X = maxSpeed * curDir;
			}
			pos.X += mov.X;
			HitBox overlap = level.CheckCollisionSolid(this.HitBox);
			if (mov.X < 0 && overlap.Left != 0)
			{
				pos.X -= overlap.Left;
				mov.X = 0;
			}
			else if (mov.X > 0 && overlap.Right != 0)
			{
				pos.X -= overlap.Right;
				mov.X = 0;
			}
		}

		/*
		public void MoveHorzOld(Controls controls, int lean)
		{
			///////////////////////////////////////////////////////////////////////////////////////////
			// this is the old method for moving the player horizontally
			// it features a very gradual acceleration method that provides much more limited control
			///////////////////////////////////////////////////////////////////////////////////////////
			if (lean == 0 || (!controls.Run && ((lean < 0 && mov.X < -MaxRunSpeed) || (lean > 0 && mov.X > MaxRunSpeed))))
			{
				// slow down
				if (mov.X < 0)
				{
					mov.X += Decel;
					if (mov.X > 0)
						mov.X = 0;
				}
				else if (mov.X > 0)
				{
					mov.X -= Decel;
					if (mov.X < 0)
						mov.X = 0;
				}
			}
			else
			{
				// speed up
				float maxSpeed = MaxWalkSpeed;
				float accel = WalkAccel;
				if (controls.Run && ((lean < 0 && mov.X < 0) || (lean > 0 && mov.X > 0)))
				{
					if (mov.X <= -MaxRunSpeed || mov.X >= MaxRunSpeed)
					{
						maxSpeed = MaxSprintSpeed;
						accel = SprintAccel;
					}
					else if (mov.X <= -MaxWalkSpeed || mov.X >= MaxWalkSpeed)
					{
						maxSpeed = MaxRunSpeed;
						accel = RunAccel;
					}
				}
				
				mov.X += lean * accel;
				if (mov.X < -maxSpeed && lean < 0)
					mov.X = -maxSpeed;
				if (mov.X > maxSpeed && lean > 0)
					mov.X = maxSpeed;
			}
			pos.X += mov.X;
			HitBox overlap = level.CheckCollisionSolid(this.HitBox);
			if (mov.X < 0 && overlap.Left != 0)
			{
				pos.X -= overlap.Left;
				mov.X = 0;
			}
			else if (mov.X > 0 && overlap.Right != 0)
			{
				pos.X -= overlap.Right;
				mov.X = 0;
			}
		}
		*/

		public void Update(Controls controls)
		{
			// track the state of the previous frame
			State prevState = state;

			// update tomatoes already on screen
			numTomatoes = 0;
			List<Tomato> completed = new List<Tomato>();
			foreach (Tomato tomato in tomatoes)
			{
				tomato.Update();
				if (tomato.Active)
					++numTomatoes;
				if (tomato.Finished)
					completed.Add(tomato);
			}
			foreach (Tomato tomato in completed)
				tomatoes.Remove(tomato);

			if (state == State.Dead)
			{
				++timeInState;
				if (timeInState >= DeathTime)
					level.CompletionCode = Level.CompleteCode.PlayerDied;
				return;
			}
			if (state == State.Dying)
			{
				++timeInState;
				if (timeInState < 2)
				{
					if (mov.Y > 0)
						mov.Y = -mov.Y;
				}
				mov.Y += Gravity / 2;
				if (mov.Y > FallSpeed)
					mov.Y = FallSpeed;
				pos += mov;
				if (pos.Y > level.Bounds.Bottom)
				{
					state = State.Dead;
					timeInState = 0;
				}
			
				// if the player is dead, don't do any other updates
				return;
			}

			// move the player horizontally
			int lean = 0;
			if (controls.Left)
				lean -= 1;
			if (controls.Right)
				lean += 1;
			MoveHorz(controls, lean);

			// move the player vertically
			if (state == State.Standing || state == State.Walking || state == State.Running || state == State.Sprinting)
			{
				if (!jumpPressedLastFrame && controls.Jump)
					state = State.Jumping;
				else
				{
					// simulate the player falling
					pos.Y += FallSpeed;
					HitBox overlap = level.CheckCollisionSolid(this.HitBox);
					if (overlap.Bottom != 0)
						// correct based on the overlap
						// this will correct for slopes and moving platforms too
						pos.Y -= overlap.Bottom;
					else
					{
						pos.Y -= FallSpeed;
						// if the falling simulation did not hit a solid object, 
						// then the player should fall for real
						state = State.Falling;
						lateJumpTimer = LateJumpFrames;
					}
				}
			}
			else if (state == State.Jumping)
			{
				mov.Y = -JumpSpeed;
				pos.Y += mov.Y;
				HitBox overlap = level.CheckCollisionSolid(this.HitBox);
				if (overlap.Top != 0)
				{
					pos.Y -= overlap.Top;
					mov.Y = 0;
					state = State.Falling;
				}
			}
			else if (state == State.Falling)
			{
				if (!jumpPressedLastFrame && controls.Jump && lateJumpTimer > 0)
				{
					mov.Y = 0;
					state = State.Jumping;
					lateJumpTimer = 0;
				}
				else
				{
					if (!jumpPressedLastFrame && controls.Jump)
						earlyJumpTimer = EarlyJumpFrames;

					mov.Y += Gravity;
					if (mov.Y > FallSpeed)
						mov.Y = FallSpeed;
					pos.Y += mov.Y;
					HitBox overlap = level.CheckCollisionSolid(this.HitBox);
					if (overlap.Bottom != 0)
					{
						pos.Y -= overlap.Bottom;
						mov.Y = 0;
						if (earlyJumpTimer > 0)
						{
							state = State.Jumping;
							earlyJumpTimer = 0;
						}
						else
							state = State.Standing;
					}
				}
			}
			if (earlyJumpTimer > 0)
				--earlyJumpTimer;
			if (lateJumpTimer > 0)
				--lateJumpTimer;
			
			// see if the player completed the level
			if (HitBox.Left >= level.Bounds.Right)
			{
				level.CompletionCode = Level.CompleteCode.Victory;
				return;
			}
			// see if the player fell in the hole
			if (pos.Y > level.Bounds.Bottom)
			{
				KillQuickly();
				return;
			}
			// see if the player died of hunger
			--health;
			if (health < 0)
			{
				KillSlowly();
				return;
			}

			// make the player attack if needed
			if (controls.Attack && !attackPressedLastFrame && CanAttack)
				attacking = true;

			// update the Y (attacking) frame
			if (attacking)
			{
				if (frameY < AttackFrameFinal)
					++frameY;
				else
				{
					attacking = false;
					frameY = 0;
				}
			}
			
			// throw a tomato if needed
			if (attacking && frameY == AttackFrameToStartThrow && numTomatoes < MaxTomatoes)
			{
				float startPosX = pos.X + Width / 2 - 8;
				float startPosY = pos.Y + Height / 2 - 8 - TomatoOffsetY;
				if (TomatoSpeed < 0)
					startPosX -= TomatoOffsetX;
				else
					startPosX += TomatoOffsetX;
					
				Tomato tomato = new Tomato(level, tomatoTexture, startPosX, startPosY, TomatoSpeed, 0);
				tomatoes.Add(tomato);
			}

			// update the state
			if (state == State.Standing || state == State.Walking || state == State.Running || state == State.Sprinting)
			{
				if (mov.X == 0)
					state = State.Standing;
				else if (mov.X <= MaxWalkSpeed && mov.X >= -MaxWalkSpeed)
					state = State.Walking;
				else if (mov.X <= MaxRunSpeed && mov.X >= -MaxRunSpeed)
					state = State.Running;
				else
					state = State.Sprinting;
			}
			if (state == prevState)
				++timeInState;
			else
				timeInState = 0;
			if (state == State.Jumping && (!controls.Jump || timeInState > JumpHeight))
				state = State.Falling;

			// update the X frame
			if (lean < 0)
				frameReversed = true;
			if (lean > 0)
				frameReversed = false;
			switch (state)
			{
				case State.Standing:
					frameX = 0;
					break;
				case State.Walking:
					frameX = WalkingFrameOrder[(timeInState / WalkingFrameNum) % WalkingFrameOrder.Length];
					break;
				case State.Running:
					frameX = RunningFrameOrder[(timeInState / RunningFrameNum) % RunningFrameOrder.Length];
					break;
				case State.Sprinting:
					frameX = SprintingFrameOrder[(timeInState / SprintingFrameNum) % SprintingFrameOrder.Length];
					break;
				case State.Jumping:
					if (timeInState > 1)
						frameX = JumpingFrame;
					else if (timeInState == 1)
						frameX = JumpingFrame - 1;
					else if (timeInState == 0)
						frameX = JumpingFrame - 2;
					break;
				case State.Falling:
					frameX = FallingFrame;
					break;
			}

			// update button flags
			jumpPressedLastFrame = controls.Jump;
			attackPressedLastFrame = controls.Attack;
		}

		public void Draw(Camera camera)
		{
			Rectangle source = new Rectangle(frameX * Width, frameY * Height, Width, Height);
			Rectangle destination = new Rectangle(X, Y, Width, Height);

			SpriteEffects effects = SpriteEffects.None;
			if (frameReversed)
				effects |= SpriteEffects.FlipHorizontally;
			if (state == State.Dying)
				effects |= SpriteEffects.FlipVertically;
			camera.Draw(joeTexture, source, destination, effects);

			foreach (Tomato tomato in tomatoes)
				tomato.Draw(camera);
		}
	}
}