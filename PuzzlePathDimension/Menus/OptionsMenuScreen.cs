//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace PuzzlePathDimension {
  /// <summary>
  /// The options screen is brought up over the top of the main menu
  /// screen, and gives the user a chance to configure the game
  /// in various hopefully useful ways.
  /// </summary>
  class OptionsMenuScreen : GameScreen {
    /// <summary>
    /// The strings that represent the possible input types.
    /// </summary>
    static string[] controllerType = { "Keyboard/Mouse", "Xbox 360 Gamepad" };

    /// <summary>
    /// Whether sound is on or not.
    /// </summary>
    bool sound;
    /// <summary>
    /// The current input device being used.
    /// </summary>
    int currentControllerType; // could be AdapterType
    /// <summary>
    /// Whether the currently selected controller is connected.
    /// </summary>
    bool currentControllerConnected;

    /// <summary>
    /// The menu template that forms the basis of the options screen.
    /// </summary>
    MenuTemplate menuTemplate = new MenuTemplate();

    /// <summary>
    /// The menu button that represents the sound option.
    /// </summary>
    MenuButton soundMenuEntry;
    /// <summary>
    /// The menu button that represents the controller type option.
    /// </summary>
    MenuButton controllerConfigurationMenuEntry;
    /// <summary>
    /// The menu button that, when selected, saves all changes made to the options.
    /// </summary>
    MenuButton apply;
    /// <summary>
    /// The menu button that, when selected, cancels all changes made to the options.
    /// </summary>
    MenuButton back;

    /// <summary>
    /// The first line of text that warns that a controller is not connected.
    /// </summary>
    TextLine firstWarningLine;
    /// <summary>
    /// The first line of text that warns that a controller is not connected.
    /// </summary>
    TextLine secondWarningLine;

    /// <summary>
    /// Constructor.
    /// </summary>
    public OptionsMenuScreen(TopLevelModel topLevel)
      : base(topLevel) {
      base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
      base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
    }

    public override void LoadContent(ContentManager shared) {
      base.LoadContent(shared);
      SpriteFont font = shared.Load<SpriteFont>("Font/menufont");

      menuTemplate.Title = new TextLine("Options", font, new Color(192, 192, 192));

      IList<MenuButton> items = menuTemplate.Items;

      soundMenuEntry = new MenuButton(string.Empty, font);
      soundMenuEntry.Selected += SoundMenuEntrySelected;
      items.Add(soundMenuEntry);

      controllerConfigurationMenuEntry = new MenuButton(string.Empty, font);
      controllerConfigurationMenuEntry.Selected += ControllerConfigurationMenuEntrySelected;
      items.Add(controllerConfigurationMenuEntry);

      apply = new MenuButton("Apply Changes", font);
      apply.Selected += OnApply;
      items.Add(apply);

      back = new MenuButton("Cancel", font);
      back.Selected += OnCancel;
      items.Add(back);

      // Warning messages
      firstWarningLine = new TextLine("The currently selected controller is not connected.", font, Color.Black);
      secondWarningLine = new TextLine("The change will not take effect.", font, Color.Black);
      
      // Get the current settings of the game.
      sound = Profile.Prefs.PlaySounds;
      currentControllerType = (int)Controller.InputType;
      currentControllerConnected = true; // Well, it has to be to even open this menu (for now).

      SetMenuEntryText();
    }

    protected override void OnButtonReleased(VirtualButtons button) {
      switch (button) {
      case VirtualButtons.Up:
        menuTemplate.SelectPrev();
        break;
      case VirtualButtons.Down:
        menuTemplate.SelectNext();
        break;
      case VirtualButtons.Select:
      case VirtualButtons.Context:
        menuTemplate.Confirm();
        break;
      case VirtualButtons.Delete:
        OnCancel();
        break;
      }
    }

    protected override void OnPointChanged(Point point) {
      menuTemplate.SelectAtPoint(point);
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
      base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

      // Check whether the currently selected controller is connected. This is for
      // the warning message.
      switch ((InputType)currentControllerType) {
      case InputType.KeyboardMouse:
        currentControllerConnected = true;
        break;
      case InputType.Xbox360Controller:
        currentControllerConnected = GamePad.GetState(PlayerIndex.One).IsConnected;
        break;
      }

      menuTemplate.TransitionPosition = TransitionPosition;
      menuTemplate.Update(gameTime);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
      menuTemplate.Draw(spriteBatch, gameTime);

      // Display a warning message if the currently selected controller isn't even connected.
      if (!currentControllerConnected) {
        // Um...should the template really be in charge of calling Begin and End? 
        // Or should it be the Screen's job? - Jorenz
        spriteBatch.Begin();

        GraphicsCursor cursor = new GraphicsCursor();
        cursor.Position = new Vector2(400, 400);

        // Modify the alpha to fade text out during transitions.
        cursor = (new AlphaEffect(1.0f - TransitionPosition)).ApplyTo(cursor);

        // Draw the warning messages.
        firstWarningLine.Draw(spriteBatch, cursor, gameTime);
        cursor.Y += 50;
        secondWarningLine.Draw(spriteBatch, cursor, gameTime);

        spriteBatch.End();
      }
    }

    /// <summary>
    /// Fills in the latest values for the options screen menu text.
    /// </summary>
    void SetMenuEntryText() {
      soundMenuEntry.Text = "Sound: " + (sound ? "On" : "Off");
      controllerConfigurationMenuEntry.Text = "Controller Type: " + controllerType[currentControllerType];
    }

    void OnApply() {
      // Apply any changes here.
      Profile.Prefs.PlaySounds = sound;

      // Make sure the user doesn't accidentally make the game unplayable.
      if (currentControllerConnected) {
        Controller.InputType = (InputType)currentControllerType;
      }

      Profile.Save(Configuration.UserDataPath + "/profile.xml");
      ExitScreen();
    }

    void OnCancel() {
      // Don't change anything.
      ExitScreen();
    }

    /// <summary>
    /// Event handler for when the Sound menu entry is selected.
    /// </summary>
    void SoundMenuEntrySelected() {
      sound = !sound;

      SetMenuEntryText();
    }

    /// <summary>
    /// Event handler for when the Controller Configuration menu entry is selected.
    /// </summary>
    void ControllerConfigurationMenuEntrySelected() {
      currentControllerType = (currentControllerType + 1) % controllerType.Length;

      SetMenuEntryText();
    }
  }
}
