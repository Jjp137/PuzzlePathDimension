﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PuzzlePathDimension {
  public class Xbox360ControllerAdapter : IVirtualAdapter {
    /// <summary>
    /// Gets whether the Xbox 360 controller is connected.
    /// </summary>
    public bool Connected {
      get { return GamePad.GetState(PlayerIndex.One).IsConnected; }
    }

    public void Update(WritableVirtualController controller, GameTime gameTime) {
      controller.SetButtonState(VirtualButtons.Delete, IsButtonDown(Buttons.B));
      controller.SetButtonState(VirtualButtons.Select, IsButtonDown(Buttons.A));
      controller.SetButtonState(VirtualButtons.Pause, IsButtonDown(Buttons.Start));
      controller.SetButtonState(VirtualButtons.Context, IsButtonDown(Buttons.Y));

      controller.SetButtonState(VirtualButtons.Up, IsButtonDown(Buttons.DPadUp) || IsButtonDown(Buttons.LeftThumbstickUp));
      controller.SetButtonState(VirtualButtons.Down, IsButtonDown(Buttons.DPadDown) || IsButtonDown(Buttons.LeftThumbstickDown));
      controller.SetButtonState(VirtualButtons.Left, IsButtonDown(Buttons.DPadLeft) || IsButtonDown(Buttons.LeftThumbstickLeft));
      controller.SetButtonState(VirtualButtons.Right, IsButtonDown(Buttons.DPadRight) || IsButtonDown(Buttons.LeftThumbstickRight));

      controller.Point = new Point(0, 0);

      // deprecated buttons
      controller.SetButtonState(VirtualButtons.Back, controller.IsButtonPressed(VirtualButtons.Delete));
      controller.SetButtonState(VirtualButtons.Confirm, controller.IsButtonPressed(VirtualButtons.Select));
    }


    private bool IsButtonDown(Buttons button) {
      return GamePad.GetState(PlayerIndex.One).IsButtonDown(button);
    }
  }
}
