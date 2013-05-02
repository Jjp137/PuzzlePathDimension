﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace PuzzlePathDimension {
  class CompletionScreen : GameScreen{

    DetailsTemplate detailsTemplate;
    LevelScoreData levelData;
    LevelGroup levelSet;

    MenuButton nextLevelMenuEntry;
    MenuButton levelSelectMenuEntry;
    MenuButton retryMenuEntry;

    ContentManager content;
    Simulation simulation;

    public CompletionScreen(TopLevelModel topLevel, LevelScoreData completedLevel, Simulation simulation)
    : base(topLevel) {

      levelData = completedLevel;
      this.simulation = simulation;

      base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
      base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
    }

    public override void LoadContent(ContentManager shared) {
      base.LoadContent(shared);

      content = shared;
      SpriteFont font = shared.Load<SpriteFont>("Font/menufont");
      int treasureScore = 500 * levelData.TreasuresCollected;
      int ballsLeftScore = 150 * levelData.BallsLeft;
      string fullPath = Configuration.UserDataPath + Path.DirectorySeparatorChar + "levellist.xml";

      detailsTemplate = new DetailsTemplate();
      Console.WriteLine(fullPath);
      levelSet = LevelGroup.Load(fullPath);


      detailsTemplate.Title = new TextLine("Congratulations, Level Complete.", font, Color.White);

      IList<IMenuLine> description = detailsTemplate.Lines;

      description.Add(new TextLine("Treasures obtained: " + levelData.TreasuresCollected + "/" + levelData.TreasuresInLevel +
      " (+" + treasureScore + ")", font, Color.White));
      description.Add(new TextLine("Balls remaining: " + levelData.BallsLeft + " (+" + ballsLeftScore + ")", font, Color.White));
      description.Add(new TextLine("Time spent (seconds): " + levelData.TimeSpent, font, Color.White));
      description.Add(new TextLine("Par time (seconds): " + levelData.ParTime, font, Color.White));
      if (levelData.TimeSpent <= levelData.ParTime) {
        description.Add(new TextLine("Par time met! (+100)", font, Color.White));
      }

      description.Add(new TextLine("Your score is: " + levelData.Score, font, Color.White));

      retryMenuEntry = new MenuButton("Retry Level", font);
      retryMenuEntry.Selected += RetryMenuEntrySelected;
      detailsTemplate.Buttons[DetailsTemplate.Selection.Left] = retryMenuEntry;

      levelSelectMenuEntry = new MenuButton("Level Select", font);
      levelSelectMenuEntry.Selected += LevelSelectMenuEntrySelected;
      detailsTemplate.Buttons[DetailsTemplate.Selection.Middle] = levelSelectMenuEntry;
      detailsTemplate.SelectedItem = DetailsTemplate.Selection.Middle;

      if (levelSet.FindNextLevel(levelData.LevelName) != null) {
        nextLevelMenuEntry = new MenuButton("Next Level", font);
        nextLevelMenuEntry.Selected += NextLevelMenuEntrySelected;
        detailsTemplate.Buttons[DetailsTemplate.Selection.Right] = nextLevelMenuEntry;
        detailsTemplate.SelectedItem = DetailsTemplate.Selection.Right;
      }

      SaveUserProgress();
    }

    protected override void OnButtonReleased(VirtualButtons button) {
      switch (button) {
      case VirtualButtons.Left:
        detailsTemplate.SelectPrev();
        break;
      case VirtualButtons.Right:
        detailsTemplate.SelectNext();
        break;
      case VirtualButtons.Select:
        detailsTemplate.Confirm();
        break;
      }
    }

    protected override void OnPointChanged(Point point) {
      detailsTemplate.SelectAtPoint(point);
    }

    /// <summary>
    /// Update the Screen.
    /// </summary>
    /// <param name="gameTime"></param>
    /// <param name="otherScreenHasFocus"></param>
    /// <param name="coveredByOtherScreen"></param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
      base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

      detailsTemplate.TransitionPosition = TransitionPosition;
      detailsTemplate.Update(gameTime);
    }

    /// <summary>
    /// Draw the game's description and the Buttons to the Screen.
    /// </summary>
    /// <param name="gameTime"></param>
    /// <param name="spriteBatch"></param>
    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
      base.Draw(gameTime, spriteBatch);

      detailsTemplate.Draw(spriteBatch, gameTime);
    }

    /// <summary>
    /// Event handler for when the user selects the Nect level menu entry.
    /// </summary>
    void NextLevelMenuEntrySelected() {
      LevelEntry nextLevel = levelSet.FindNextLevel(levelData.LevelName);
      Console.WriteLine(nextLevel.FullPath);

      LoadingScreen.Load(TopLevel, true, new GameEditorScreen(TopLevel, nextLevel.FullPath));
    }

    /// <summary>
    /// Event handler for when the user selects ok on the level select
    /// button on the message box. This uses the loading screen to
    /// transition from the game back to the level select screen.
    /// </summary>
    void LevelSelectMenuEntrySelected() {
      LoadingScreen.Load(TopLevel, false, null, new BackgroundScreen(TopLevel), new MainMenuScreen(TopLevel), new LevelSelectScreen(TopLevel, content));
    }

    /// <summary>
    /// Event handler for when the user selects the Retry menu entry.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void RetryMenuEntrySelected() {
      simulation.Restart();
    }

    /// <summary>
    /// Saves the user progress.
    /// </summary>
    private void SaveUserProgress() {
      string levelName = levelData.LevelName;
      UserProfile profile = base.Profile;
      SerializableDictionary<string, LevelStatus> progressInfo = base.Profile.Progress;

      LevelStatus status;

      // If the user has played the level before, get the info from the dictionary,
      // and replace the score if it's better.
      if (progressInfo.ContainsKey(levelName)) {
        status = progressInfo[levelName];

        status.Completed = true;

        if (status.Score < levelData.Score) {
          status.Score = levelData.Score;
          status.FastestTimeInSeconds = levelData.TimeSpent;
        }

      } else {
        // Otherwise, create a new LevelStatus object and insert it into
        // the dictionary.
        status = new LevelStatus();
        status.Completed = true;
        status.FastestTimeInSeconds = levelData.TimeSpent;
        status.Score = levelData.Score;

        progressInfo[levelName] = status;
      }

      // In either case, save the user's profile.
      profile.Save(Configuration.UserDataPath + Path.DirectorySeparatorChar + "profile.xml");
    }
  }
}
