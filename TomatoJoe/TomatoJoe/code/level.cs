using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;

namespace TomatoJoe
{
	public class Level
	{
		private Game game;
		private Camera camera;
		private Player player;
		
		private Color backgroundColor;
		private string author;
		private int[] land;
		
		public Color BackgroundColor { get { return backgroundColor; } }

		public static int BlockSize { get { return 24; } }
		public static int ScreenWidth { get { return BlockSize * 12; } }
		public static int ScreenHeight { get { return BlockSize * 9; } }
		public Rectangle Bounds { get { return new Rectangle(0, 0, land.Length * BlockSize, ScreenHeight); } }
		
		private static bool randomLevels = false;
		private static Random rand = new Random();
		
		List<Item> itemList;
		List<Enemy> enemyList;
		
		public void ExtraLife() { game.ExtraLife(); }
		
		public enum CompleteCode
		{
			NotLoaded = -1,
			Incomplete = 0,
			PlayerDied = 1,
			Victory = 2,
		}
		private CompleteCode completeCode;
		
		public CompleteCode CompletionCode
		{ get { return completeCode; } 
		set {
			if (completeCode != CompleteCode.Incomplete)
				throw new Exception();

			completeCode = value;
		} }

		public Level(Game game, SpriteBatch batch)
		{
			completeCode = CompleteCode.NotLoaded;
			this.game = game;
			player = new Player(this, game.joeTexture, game.tomatoTexture, 0, ScreenHeight - BlockSize - Player.Height);
			camera = new Camera(batch, this);
		}
		
		public void LoadRandomLevel(int difficulty)
		{
			difficulty *= 5;
			difficulty += 0;
			// land constants
			const int maxLandHeight = 8;			// the maximum height from the bottom of the screen that land can appear
			const int maxJumpHeight = 3;			// the maximum vertical distance that Joe can jump upwards
			const float maxFlatJumpWidth = 6.6f;	// the maximum horizontal jumping distance if both ledges are the same height
			const float jumpDifferential = 0.8f;	// the amount the maximum horizontal distance changes per vertical square
			
			// item constants
			const int avgRegTomatoDist = 20;	// the average number of squares between a regular tomato and the next tomato
			const int avgGoldTomatoDist = 60;	// the average number of squares between a golden tomato and the next tomato
			const int varRegTomatoDist = 5;		// the variance in number of squares between a regular tomato and the next tomato
			const int varGoldTomatoDist = 20;	// the variance in number of squares between a golden tomato and the next tomato
			const float avgOddsOfGolden = 0.15f;// the average chance that a spawned tomato will be golden
			const float avgOddsofExLife = 0.002f;// the average chance each square that an extra life will be spawned
			
			const int minItemHeight = 3;		// the minimum height above the ground that an item can be spawned
			const int maxItemHeight = 5;		// the maximum height above the ground that an item can be spawned
			
			// calculate the length of the level and initialize
			int avgLength = 150 + difficulty;
			int levelLength = rand.Next(avgLength * 20 / 100, avgLength * 120 / 100); // length can vary 20% from the average
			
			land = new int[levelLength];
			itemList = new List<Item>();
			enemyList = new List<Enemy>();
			List<Item.Temp> tempItemList = new List<Item.Temp>();
			completeCode = CompleteCode.Incomplete;
			
			// generate random background color
			int red = 255 - difficulty - rand.Next(100 + difficulty);
			int green = 255 - difficulty - rand.Next(100 + difficulty);
			int blue = 255 - difficulty - rand.Next(100 + difficulty);
			if (red < 0) red = rand.Next(100);
			if (green < 0) green = rand.Next(100);
			if (blue < 0) blue = rand.Next(100);
			backgroundColor = new Color(red, green, blue);
			
			// begin generating land
			int curLandHeight = 4;
			for (int i = 0; i < 3; ++i)
				curLandHeight += rand.Next(-1, 1);
			int prevHeight = curLandHeight;
			int curLength = 0; // tell it to generate a fresh length
			bool isHole = true; // ...as if a hole just ended
			int curTomatoDist = curTomatoDist = avgRegTomatoDist * 2 + rand.Next(-varRegTomatoDist * 2, varRegTomatoDist * 2);
			
			for (int square = 0; square < levelLength;)
			{
				// is the last section finished?
				if (curLength <= 0)
				{
					curLength = 0;
					// generate a new length for the next section
					// did we just finish a hole?
					if (isHole)
					{
						// add a new section of land (land height is already specified, lets get the length)
						isHole = false;
						int odds = 200;
						curLength = 0;
						while (rand.Next(200) < odds)
						{
							odds -= 10 + rand.Next(difficulty);
							++curLength;
						}
					}
					// did we just finish a section of land?
					else
					{
						// determine the height difference with the next section
						int heightDifference = 0;
						for (int i = 0; i < 8; ++i)
						{
							if (rand.Next(200) < 50 + difficulty)
								heightDifference += rand.Next(-1, 1);
						}
						// if land is already at max or min height, reverse the height change if necessary
						if (curLandHeight == maxLandHeight && heightDifference > 0 || curLandHeight == 1 && heightDifference < 0)
							heightDifference *= -1;
						// if the height change is higher than Joe can jump, reduce it to his maximum jump height
						if (heightDifference > maxJumpHeight)
							heightDifference = maxJumpHeight;
						// if the change will take the land higher than maximum, set it to the maximum difference
						if (curLandHeight + heightDifference > maxLandHeight)
							heightDifference = maxLandHeight - curLandHeight;
						// if the change will take the land lower than minimum, set it to the minimum difference
						if (curLandHeight + heightDifference < 1)
							heightDifference = 1 - curLandHeight;
						// make sure we haven't ended up with an invalid land height somehow!
						if (curLandHeight + heightDifference > maxLandHeight + heightDifference || curLandHeight < 1)
							throw new Exception("Level Randomizer Error #1: INVALID Land Height!");
						
						// determine whether we should next add a hole, or just a conjoined section of land
						if (rand.Next(50) < 10 + rand.Next(difficulty))
						{
							// calculate the maximum length of the hole
							int maxHoleLength = (int)(0.5f + maxFlatJumpWidth - jumpDifferential * heightDifference);
							// early in the level, shorten the max hole length
							if (square < 20) maxHoleLength -= 2;
							
							// determine the length that this hole should be, based on difficulty and randomness
							curLength = 0;
							for (int i = 0; i < maxHoleLength; ++i)
							{
								if (rand.Next(50) < 20 + rand.Next(difficulty))
									++curLength;
							}
						}
						
						// regardless of whether we're adding a new hole or a conjoined section, we need to set these
						isHole = true;
						prevHeight = curLandHeight;
						curLandHeight += heightDifference;
						// is the length still zero?
						if (curLength <= 0)
							// lets go back and add on a new conjoined section of land
							continue;
					}
				}
				
				// continue adding the current section of land
				if (isHole)
					land[square] = 0;
				else
					land[square] = curLandHeight;
				--curLength;
				
				// continue adding tomatoes where necessary
				if (curTomatoDist <= 0)
				{
					// add a new tomato
					Item.Temp tomato = new Item.Temp(square - 1, 
						prevHeight + rand.Next(minItemHeight, maxItemHeight), Item.Type.Food);
					// should we add a golden tomato?
					if (rand.NextDouble() < avgOddsOfGolden)
					{
						// make it a golden tomato
						tomato.type = Item.Type.Gold;
						// determine the distance to the next tomato
						curTomatoDist = avgGoldTomatoDist + rand.Next(-varGoldTomatoDist, varGoldTomatoDist);
						// increase the distance haphazardly based on difficulty
						curTomatoDist += rand.Next(difficulty) * varGoldTomatoDist / 50;
						// make sure the distance to next tomato isn't too far
						if (curTomatoDist > avgGoldTomatoDist + varGoldTomatoDist * 4)
						{
							curTomatoDist = avgGoldTomatoDist;
							for (int i = 0; i < 4; ++i)
								curTomatoDist += rand.Next(varGoldTomatoDist);
						}
					}
					// should we add a regular tomato?
					else
					{
						// make it a regular tomato
						tomato.type = Item.Type.Food;
						// determine the distance to the next tomato
						curTomatoDist = avgRegTomatoDist + rand.Next(-varRegTomatoDist, varRegTomatoDist);
						// increase the distance haphazardly based on difficulty
						curTomatoDist += rand.Next(difficulty) * varRegTomatoDist / 50;
						// make sure the distance to next tomato isn't too far
						if (curTomatoDist > avgRegTomatoDist + varRegTomatoDist * 4)
						{
							curTomatoDist = avgRegTomatoDist;
							for (int i = 0; i < 4; ++i)
								curTomatoDist += rand.Next(varRegTomatoDist);
						}
					}
					// add the tomato item to the temp list
					tempItemList.Add(tomato);
				}
				--curTomatoDist;
				
				// should we add an extra life?
				if (rand.NextDouble() < avgOddsofExLife * (50 + difficulty) / 50)
				{
					Item.Temp life = new Item.Temp(square - 1, 
						prevHeight + rand.Next(minItemHeight, maxItemHeight), Item.Type.Life);
					tempItemList.Add(life);
				}
				
				// only go to the next square if we've finished all the tasks for this square
				++square;
			}
			// add the items to the main list
			foreach (Item.Temp temp in tempItemList)
				itemList.Add(new Item(game.itemTexture, temp.column, temp.row, temp.type));
		}
		
		public void Load(int levelNum)
		{
			/* TEMP
			itemList = new List<Item>();
			completeCode = CompleteCode.Incomplete;
			backgroundColor = Color.CornflowerBlue;
			//this.levelNum = levelNum; TODO:CHECK?
			
			switch (levelNum)
			{
				case 1: // the first test level
					backgroundColor = Color.LightSkyBlue;
					land = new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 4, 4, 4, 4, 0, 0, 3, 3, 3, 3, 0, 0, 2, 2, 2, 2, 2, 2, 5, 5, 
						5, 5, 5, 6, 6, 6, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 4, 4, 4, 4, 4, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 2, 
						2, 2, 2, 5, 5, 5, 3, 3, 3, 3, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 4, 4, 4, 4, 4, 4, 4, 4, 4, };
					itemList.Add(new Item(game.itemTexture, 23, 8, Item.Type.Food));
					itemList.Add(new Item(game.itemTexture, 61, 5, Item.Type.Food));
					itemList.Add(new Item(game.itemTexture, 84, 6, Item.Type.Food));
					break;
				case 2: // the second test level
					backgroundColor = Color.DarkKhaki;
					land = new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 2, 2, 2, 2, 2, 4, 4, 4, 4, 4, 0, 0, 0, 
						0, 2, 2, 2, 2, 2, 0, 0, 4, 4, 4, 4, 4, 1, 1, 1, 1, 1, 0, 4, 4, 4, 4, 6, 6, 6, 6, 6, 0, 0, 0, 0, 0, 
						2, 2, 2, 2, 5, 5, 5, 0, 0, 0, 6, 6, 0, 0, 6, 6, 6, 0, 0, 0, 5, 5, 5, 5, 5, 5, 0, 0, 0, 0, 3, 3, 3, 
						3, 3, 3, };
					itemList.Add(new Item(game.itemTexture, 2, 6, Item.Type.Food));
					itemList.Add(new Item(game.itemTexture, 63, 8, Item.Type.Gold));
					break;
				case 3: // the hard test level
					backgroundColor = Color.Crimson;
					land = new int[] { 
					//  0  1  2  3  4  5  6  7  8  9
						2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 10
						0, 0, 0, 0, 2, 2, 0, 0, 0, 4, // 20
						4, 4, 0, 0, 0, 0, 4, 0, 0, 0, // 30
						0, 4, 0, 0, 0, 6, 6, 6, 0, 0, // 40
						0, 0, 0, 2, 2, 2, 2, 2, 4, 4, // 50
					//  0  1  2  3  4  5  6  7  8  9
						4, 0, 0, 0, 0, 0, 2, 0, 0, 0, // 60
						0, 2, 2, 0, 0, 0, 0, 3, 3, 3, // 70
						0, 0, 0, 0, 4, 4, 0, 0, 0, 0, // 80
						0, 3, 3, 0, 0, 0, 0, 0, 0, 1, // 90
						1, 1, 1, 0, 0, 0, 0, 3, 0, 0, // 100
					//  0  1  2  3  4  5  6  7  8  9
						0, 0, 4, 0, 0, 0, 0, 5, 0, 0, // 110
						0, 0, 0, 3, 0, 0, 0, 0, 0, 2, // 120
						2, 2, 2, };
					itemList.Add(new Item(game.itemTexture, 46, 5, Item.Type.Food));
					itemList.Add(new Item(game.itemTexture, 58, 4, Item.Type.Food));
					break;
				default: // an empty level
					land = new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, };
					break;
			}
			/*/ // TEMP
			
			if (levelNum < 0 || levelNum > 99)
				levelNum = 0;
			game.LevelNum = levelNum;
			
			// load a random level?
			if (randomLevels)
			{
				LoadRandomLevel(levelNum);
				return;
			}
			
			// load the level file
			string filename = "Level";
			if (levelNum < 10)
				filename += '0';
			filename += levelNum.ToString() + ".txt";

			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Levels/" + filename);
			string lineOfText;
			StreamReader sr;
			try {
				sr = new StreamReader(path);
			} catch (FileNotFoundException)
			{
				Load(levelNum + 1);
				return;
			}
			
			itemList = new List<Item>();
			enemyList = new List<Enemy>();
			completeCode = CompleteCode.Incomplete;
			backgroundColor = Color.CornflowerBlue;
			
			const int TabWidth = 8;
			const int MaxLevelLength = 1000;
			const int MaxVars = 10;
			
			int levelLength = 0;
			int levelHeight = 0;
			int[] tempLand = new int[MaxLevelLength];
			List<Item.Temp> tempItemList = new List<Item.Temp>();
			List<Enemy.Temp> tempEnemyList = new List<Enemy.Temp>();
			
			string[] tempVarNames = new string[MaxVars];
			string[] tempVarValues = new string[MaxVars];
			for (int i = 0; i < MaxVars; ++i)
			{
				tempVarNames[i] = "";
				tempVarValues[i] = "";
			}
			//List<string> tempVarName = new List<string>();
			//List<string> tempVarValue = new List<string>();
			string tempVarData = "";
			int curVars = 0;
			int varState = 0;
			
			for (int lineNum = 0; (lineOfText = sr.ReadLine()) != null; ++lineNum)
			{
				int length = lineOfText.Length;
				if (length > MaxLevelLength)
					length = MaxLevelLength;
				
				for (int cursor = 0, colNum = 0; cursor < length; ++cursor, ++colNum)
				{
					if (varState == 0)
					{
						bool modified = false;
						switch (lineOfText[cursor])
						{
							case '#':
								modified = true;
								if (tempLand[colNum] == 0)
									tempLand[colNum] = lineNum + 1;
								break;
							case 'c':
								modified = true;
								tempItemList.Add(new Item.Temp(colNum - 1, lineNum, Item.Type.Food));
								break;
							case 'g':
								modified = true;
								tempItemList.Add(new Item.Temp(colNum - 1, lineNum, Item.Type.Gold));
								break;
							case 'e':
								modified = true;
								tempItemList.Add(new Item.Temp(colNum - 1, lineNum, Item.Type.Life));
								break;
							case 'F':
								modified = true;
								tempEnemyList.Add(new Enemy.Temp(colNum - 1, lineNum, 
										Enemy.Type.Gobbler, (int)Gobbler.SubType.Blue));
								break;
							case 'G':
								modified = true;
								tempEnemyList.Add(new Enemy.Temp(colNum - 1, lineNum, 
										Enemy.Type.Gobbler, (int)Gobbler.SubType.Green));
								break;
							case 'H':
								modified = true;
								tempEnemyList.Add(new Enemy.Temp(colNum - 1, lineNum, 
										Enemy.Type.Gobbler, (int)Gobbler.SubType.Yellow));
								break;
							case 'I':
								modified = true;
								tempEnemyList.Add(new Enemy.Temp(colNum - 1, lineNum, 
										Enemy.Type.Gobbler, (int)Gobbler.SubType.Purple));
								break;
							case '\t':
								colNum += TabWidth - (colNum % TabWidth) - 1;
								break;
							case '!':
								varState = 1;
								break;
						}
						if (modified)
						{
							if (levelLength < colNum + 1)
								levelLength = colNum + 1;
							if (levelHeight < lineNum + 1)
								levelHeight = lineNum + 1;
						}
					}
					else
					{
						char c = lineOfText[cursor];
						if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
							tempVarData += c;
						else if (varState == 1 && c == '=')
						{
							tempVarNames[curVars] = tempVarData;
							tempVarData = "";
							varState = 2;
						}
						else if (varState == 2 && c == '!')
						{
							tempVarValues[curVars] = tempVarData;
							tempVarData = "";
							varState = 1;
							++curVars;
						}
					}
				}
			}
			if (varState == 2)
				tempVarValues[curVars] = tempVarData;
			
			// assign and correct land heights
			land = new int[levelLength];
			for (int i = 0; i < levelLength; ++i)
			{
				if (tempLand[i] == 0)
					land[i] = 0;
				else
					land[i] = levelHeight + 1 - tempLand[i];
			}
			
			// add items and enemies and correct initial heights
			foreach (Item.Temp temp in tempItemList)
				itemList.Add(new Item(game.itemTexture, temp.column, levelHeight - temp.row, temp.type));
			foreach (Enemy.Temp temp in tempEnemyList)
			{
				temp.row = levelHeight - temp.row;
				enemyList.Add(Enemy.Create(game, this, temp));
			}
			
			// load options
			for (int i = 0; i < MaxVars; ++i)
			{
				string varName = tempVarNames[i].ToLower();
				if (varName == "color")
				{
					PropertyInfo colorProperty = typeof(Color).GetProperty(tempVarValues[i]);
					try
					{
						backgroundColor = (Color)colorProperty.GetValue(null, null);
					}
					catch (Exception)
					{
						backgroundColor = Color.DarkGray;
					}
					continue;
				}
				if (varName == "author")
				{
					author = tempVarValues[i];
					if (author == "")
						author = "Tim";
					continue;
				}
			}
			
			// TEMP */
		}
		
		public HitBox CheckCollisionSolid(HitBox hitbox)
		{
			HitBox overlap = new HitBox();
			if (hitbox.Left < Bounds.Left)
			{
				overlap.Left = hitbox.Left - Bounds.Left;
				return overlap;
			}
			/*/ TEMP:
			if (hitbox.Top < Bounds.Top)
			{
				overlap.Top = hitbox.Top - Bounds.Top;
				return overlap;
			}
			// :TEMP */
			for (int i = 0; i < land.Length; ++i)
			{
				HitBox landHitBox = LandHitBox(i);
				overlap = hitbox.CheckCollision(landHitBox);
				if (overlap.IsNotNull())
					return overlap;
			}
			return overlap;
		}
		
		private HitBox LandHitBox(int segment)
		{
			if (segment < 0 && segment >= land.Length)
				return HitBox.Zero;
			else if (land[segment] < 1)
				return HitBox.Zero;
			else
				return new HitBox(segment * BlockSize, ScreenHeight - BlockSize * land[segment] + 1, 
					BlockSize, BlockSize * land[segment] - 1);
		}

		public void Update(Controls controls)
		{
			player.Update(controls);
			
			camera.Update(player);

			List<Item> collected = new List<Item>();
			foreach (Item item in itemList)
			{
				item.Update(player);
				if (item.Collected)
					collected.Add(item);
			}
			foreach (Item item in collected)
				itemList.Remove(item);

			List<Enemy> killed = new List<Enemy>();
			foreach (Enemy enemy in enemyList)
			{
				enemy.Update(player);
				if (enemy.IsDead)
					killed.Add(enemy);
			}
			foreach (Enemy enemy in killed)
				enemyList.Remove(enemy);
		}

		public void Draw()
		{
			Rectangle source;
			Rectangle destination;
			// draw the land
			for (int i = 0; i < land.Length; ++i)
			{
				// draw the first half of the top block
				source = new Rectangle(BlockSize, 0, BlockSize / 2, BlockSize);
				destination = new Rectangle(i * BlockSize, ScreenHeight - BlockSize * land[i], 
					BlockSize / 2, BlockSize);
				if (i == 0 || land[i - 1] >= land[i])
					source.X -= BlockSize;
				camera.Draw(game.landTexture, source, destination);
				// draw the second half of the top block
				source.X = BlockSize + BlockSize / 2;
				destination.X += BlockSize / 2;
				if (i >= land.Length - 1 || land[i + 1] >= land[i])
					source.X -= BlockSize;
				camera.Draw(game.landTexture, source, destination);

				// draw the lower blocks
				source.Y = BlockSize;
				int currentHeight = land[i];
				for (int j = 0; j < land[i] - 1; j += 1)
				{
					currentHeight -= 1;
					destination.Y += BlockSize;
					// draw the first half of the block
					source.X = BlockSize;
					destination.X -= BlockSize / 2;
					if (i == 0 || land[i - 1] >= currentHeight)
						source.X -= BlockSize;
					camera.Draw(game.landTexture, source, destination);
					// draw the second half of the block
					source.X = BlockSize + BlockSize / 2;
					destination.X += BlockSize / 2;
					if (i >= land.Length - 1 || land[i + 1] >= currentHeight)
						source.X -= BlockSize;
					camera.Draw(game.landTexture, source, destination);
				}
			}
			
			// draw the items
			foreach (Item item in itemList)
				item.Draw(camera);

			// draw the enemies
			foreach (Enemy enemy in enemyList)
				enemy.Draw(camera);
			
			// draw the player
			player.Draw(camera);
			
			// draw the lives icon
			source = new Rectangle(0, 0, game.livesTexture.Width, game.livesTexture.Height);
			destination = new Rectangle(0, 0, game.livesTexture.Width, game.livesTexture.Height);
			camera.DrawRelative(game.livesTexture, source, destination);

			// draw the number of lives
			int numberWidth = game.numbersTexture.Width / 10;
			int number = game.Lives;
			int digits;
			for (digits = 1; number > 9; ++digits)
				number /= 10;
			int cursorPosition = game.livesTexture.Width + (digits - 1) * numberWidth;
			for (number = game.Lives; digits > 0; --digits, cursorPosition -= numberWidth)
			{
				source = new Rectangle(numberWidth * (number % 10), 0, numberWidth, game.numbersTexture.Height);
				destination = new Rectangle(cursorPosition, 0, numberWidth, game.numbersTexture.Height);
				camera.DrawRelative(game.numbersTexture, source, destination);
				number /= 10;
			}
			// draw the level number
			number = game.LevelNum;
			for (digits = 1; number > 9; ++digits)
				number /= 10;
			cursorPosition = camera.Width - numberWidth;
			for (number = game.LevelNum; digits > 0; --digits, cursorPosition -= numberWidth)
			{
				source = new Rectangle(numberWidth * (number % 10), 0, numberWidth, game.numbersTexture.Height);
				destination = new Rectangle(cursorPosition, 0, numberWidth, game.numbersTexture.Height);
				camera.DrawRelative(game.numbersTexture, source, destination);
				number /= 10;
			}
			cursorPosition += numberWidth;
			// draw the level text
			source = new Rectangle(0, 0, game.levelTexture.Width, game.levelTexture.Height);
			destination = new Rectangle(cursorPosition - game.levelTexture.Width, 0, game.levelTexture.Width, game.levelTexture.Height);
			camera.DrawRelative(game.levelTexture, source, destination);
			// draw the health bar
			source = new Rectangle(0, 0, game.healthTexture.Width, game.healthTexture.Height / 2);
			destination = new Rectangle(camera.Width / 2 - game.healthTexture.Width / 2, 0, game.healthTexture.Width, game.healthTexture.Height / 2);
			camera.DrawRelative(game.healthTexture, source, destination);
			source.Y = game.healthTexture.Height / 2;
			source.Width = game.healthTexture.Width * player.Health / player.MaxHealth;
			destination.Width = source.Width;
			camera.DrawRelative(game.healthTexture, source, destination);
		}
	}
}