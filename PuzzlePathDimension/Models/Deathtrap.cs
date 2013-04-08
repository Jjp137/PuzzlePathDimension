﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace PuzzlePathDimension {
  /// <summary>
  /// The Deathtrap class contains a death trap's data.
  /// </summary>
  class DeathTrap {
    /// <summary>
    /// The hard-coded height of the death trap.
    /// </summary>
    private const int _height = 60;
    /// <summary>
    /// The hard-coded width of the death trap.
    /// </summary>
    private const int _width = 60;

    /// <summary>
    /// The texture that the death trap will be drawn with.
    /// </summary>
    private Texture2D _texture;

    /// <summary>
    /// The Body that represents the death trap in the physics simulation.
    /// </summary>
    private Body _body;

    /// <summary>
    /// The upper-left corner of the death trap, in pixels.
    /// </summary>
    private Vector2 _origin;

    /// <summary>
    /// Gets the upper-left corner of the death trap, in pixels.
    /// </summary>
    public Vector2 Origin {
      get { return _origin; }
      set {
        _origin = value;
        // Moving the death trap also moves its center, so figure out the position of
        // the new center.
        _center = CalculateCenter();

        // Reposition the Body, but only if it has been initialized.
        if (_body != null) {
          _body.Position = UnitConverter.ToMeters(_center);
        }
      }
    }

    /// <summary>
    /// The position of the center of the death trap, in pixels.
    /// </summary>
    private Vector2 _center;

    /// <summary>
    /// Gets the position of the center of the death trap, in pixels.
    /// </summary>
    public Vector2 Center {
      get { return _center; }
    }

    /// <summary>
    /// Gets the death trap's height.
    /// </summary>
    public int Height {
      get { return _height; }
    }

    /// <summary>
    /// Gets the death trap's width.
    /// </summary>
    public int Width {
      get { return _width; }
    }

    /// <summary>
    /// Whether the goal was touched by a ball.
    /// </summary>
    private bool _touched;
    /// <summary>
    /// Gets whether the goal was touched by a ball.
    /// TODO: This is basically a poor man's C# event...
    /// </summary>
    public bool Touched {
      get { return _touched; }
    }

    /// <summary>
    /// Constructs a DeathTrap object.
    /// </summary>
    /// <param name="texture">The texture that the death trap will be drawn with.</param>
    /// <param name="position">The position of the death trap.</param>
    public DeathTrap(Texture2D texture, Vector2 position) {
      _texture = texture;

      // Check to make sure that the visual representation of the texture is actually the right
      // size, and print a warning to the console if that isn't the case.
      if (_texture != null && _texture.Width * _texture.Height != _width * _height) {
        Console.WriteLine("Warning: the death trap's texture does not have the expected dimensions.");
        Console.WriteLine("Expected: " + _width + ", " + _height);
        Console.WriteLine("...but the texture is: " + _texture.Width + ", " + _texture.Height);
      }
      // Figure out the origin and center of the death trap.
      _origin = position;
      _center = CalculateCenter();
      // Leave the Body object uninitialized until a World object comes by to initialize it.
      _body = null;
    }

    /// <summary>
    /// Calculates the center of the death trap.
    /// </summary>
    /// <returns>The position of the center of the death trap.</returns>
    private Vector2 CalculateCenter() {
      Vector2 center = new Vector2();
      center.X = _origin.X + (_width / 2.0f);
      center.Y = _origin.Y + (_height / 2.0f);
      return center;
    }

    /// <summary>
    /// Initializes the death trap's Body object.
    /// </summary>
    /// <param name="world">The World object that the death trap will be a part of.</param>
    public void InitBody(World world) {
      if (_body != null) {
        throw new InvalidOperationException("There is already a Body object for this death trap.");
      }
      // Obtain the radius of the death trap, in meters.
      float radius = UnitConverter.ToMeters(_width / 2);

      // Create the Body object.
      _body = BodyFactory.CreateCircle(world, radius, 1);
      // Set its position to be the center of the death trap, in meters, which is what the
      // physics engine expects.
      _body.Position = UnitConverter.ToMeters(_center);
      // The death trap should never be subjected to the World's physical forces.
      _body.BodyType = BodyType.Static;
      // The ball should not actually bounce off the death trap.
      _body.FixtureList[0].IsSensor = true;
      // Mark the body as belonging to a death trap.
      _body.UserData = "death trap";
      // Listen for collision events.
      _body.OnCollision += new OnCollisionEventHandler(HandleCollision);
    }

    /// <summary>
    /// Called when a collision with the death trap occurs.
    /// </summary>
    /// <param name="fixtureA">The first fixture that has collided.</param>
    /// <param name="fixtureB">The second fixture that has collided.</param>
    /// <param name="contact">The Contact object that contains information about the collision.</param>
    /// <returns>Whether the collision should still happen.</returns>
    private bool HandleCollision(Fixture fixtureA, Fixture fixtureB, Contact contact) {
      // Check if one of the Fixtures belongs to a ball.
      bool causedByBall = (string)fixtureA.Body.UserData == "ball" || (string)fixtureB.Body.UserData == "ball";

      // Only mark the death trap as collected if a ball collided with it for the first time.
      // Its state should get reset after it gets hit, though.
      if (contact.IsTouching() && causedByBall) {
        _touched = true;
      }
      // A death trap isn't an object that should be bounced off of, so don't actually
      // cause the collision to happen in the physics simulation.
      return false;
    }

    /// <summary>
    /// Reset the death trap's state.
    /// </summary>
    public void Reset() {
      _touched = false;
    }

    public void Draw(SpriteBatch spriteBatch) {
      // Draw the death trap, using its center as the origin to draw from.
      Vector2 origin = new Vector2((_width / 2.0f), (_height / 2.0f));
      spriteBatch.Draw(_texture, _center, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
    }
  }
}
