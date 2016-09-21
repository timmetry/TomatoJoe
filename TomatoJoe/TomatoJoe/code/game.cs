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
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game : Microsoft.Xna.Framework.Game
	{
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;

		public Texture2D joeTexture;
		public Texture2D landTexture;
		public Texture2D tomatoTexture;
		public Texture2D itemTexture;
		public Texture2D livesTexture;
		public Texture2D healthTexture;
		public Texture2D numbersTexture;
		public Texture2D levelTexture;
		public Texture2D gobblerTexture;
		public Texture2D boneTexture;

		private Level level;
		private int levelNum;
		private int lives;
		private int NewGameLives { get { return 10; } }
		private int FirstLevel { get { return 0; } }

		public int LevelNum { get { return levelNum; } set { levelNum = value; } }
		public int Lives { get { return lives; } }
		public void ExtraLife() { ++lives; }

		private bool pausePressedLastFrame;
		private bool isPaused;

		public Game()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			graphics.PreferredBackBufferWidth = Level.ScreenWidth * Camera.ZoomRate;
			graphics.PreferredBackBufferHeight = Level.ScreenHeight * Camera.ZoomRate;
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
			joeTexture = Content.Load<Texture2D>("joe");
			landTexture = Content.Load<Texture2D>("land");
			tomatoTexture = Content.Load<Texture2D>("tomato");
			itemTexture = Content.Load<Texture2D>("item");
			livesTexture = Content.Load<Texture2D>("lives");
			healthTexture = Content.Load<Texture2D>("health");
			numbersTexture = Content.Load<Texture2D>("numbers");
			levelTexture = Content.Load<Texture2D>("level");
			gobblerTexture = Content.Load<Texture2D>("gobbler");
			boneTexture = Content.Load<Texture2D>("bone");

			level = new Level(this, spriteBatch);
			levelNum = FirstLevel;
			level.Load(levelNum);
			lives = NewGameLives;
			pausePressedLastFrame = false;
			isPaused = false;
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
				|| Keyboard.GetState().IsKeyDown(Keys.Escape))
				this.Exit();

			// TODO: Add your update logic here
			Controls controls = new Controls(GamePad.GetState(PlayerIndex.One), Keyboard.GetState());

			if (!pausePressedLastFrame && controls.Pause)
				isPaused = !isPaused;

			if (!isPaused)
			{
				level.Update(controls);

				// see if the player died
				if (level.CompletionCode == Level.CompleteCode.PlayerDied)
				{
					// the player lost a life
					--lives;
					if (lives < 0)
					{
						// for now, just go back to the first level on Game Over
						levelNum = FirstLevel;
						lives = NewGameLives;
					}

					// reload the level
					level = new Level(this, spriteBatch);
					level.Load(levelNum);
				}
				// see if the player completed the level
				else if (level.CompletionCode == Level.CompleteCode.Victory)
				{
					// load the next level
					level = new Level(this, spriteBatch);
					level.Load(++levelNum);
				}
			}

			pausePressedLastFrame = controls.Pause;
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(level.BackgroundColor);

			// TODO: Add your drawing code here
			spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);
			level.Draw();
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
