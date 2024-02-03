//---------------------------------------------------------------------------------
// Written by Michael Hoffman
// Find the full tutorial at: http://gamedev.tutsplus.com/series/vector-shooter-xna/
//----------------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Diagnostics;
using System.Linq;

namespace NeonShooter
{
    static class Input
	{
		private static KeyboardState keyboardState, lastKeyboardState;

		private static TouchCollection mouseState; // MouseState
        private static TouchCollection lastMouseState; // MouseState

        private static GamePadState gamepadState, lastGamepadState;

        private static bool isTouchDetected = true;

		private static bool isAimingWithMouse = true;//false;

		public static Vector2 MousePosition 
		{ 
			get 
			{
				float x = 0f;
                float y = 0f;

                
                try//if (mouseState.IsConnected)
                {
					if (isTouchDetected)
					{
						x = mouseState[0].Position.X;
						y = mouseState[0].Position.Y;
					}
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ex] mouseState[0] error: " + ex.Message);
                    Debug.WriteLine("[i] mouseState.IsConnected = " + mouseState.IsConnected);
					isTouchDetected = false;
                }

                return new Vector2(x, y); 
			} 
		}

		public static void Update()
		{
			lastKeyboardState = keyboardState;
			lastMouseState = mouseState;
			lastGamepadState = gamepadState;

			keyboardState = Keyboard.GetState();
			mouseState = TouchPanel.GetState();//Mouse.GetState();

			gamepadState = GamePad.GetState(PlayerIndex.One);

			float lx = 0;
			float ly = 0;
			try//if (mouseState.IsConnected)
			{
				if (isTouchDetected)
				{
					lx = lastMouseState[0].Position.X;
					ly = lastMouseState[0].Position.Y;
				}
			}
			catch (Exception ex) 
			{
				Debug.WriteLine("[ex] lastMouseState[0] error: " + ex.Message);
                Debug.WriteLine("[i] mouseState.IsConnected = " + mouseState.IsConnected);
				isTouchDetected = false;
            }

			// If the player pressed one of the arrow keys or is using a gamepad to aim, we want to disable mouse aiming. Otherwise,
			// if the player moves the mouse, enable mouse aiming.
			if (new[] { Keys.Left, Keys.Right, Keys.Up, Keys.Down }.Any
				  (x => keyboardState.IsKeyDown(x))
				  || gamepadState.ThumbSticks.Right != Vector2.Zero)
			{
				isAimingWithMouse = false;
			}
			else// if (MousePosition != new Vector2(lx, ly))
			{
				isAimingWithMouse = true;
			}
		}

		// Checks if a key was just pressed down
		public static bool WasKeyPressed(Keys key)
		{
			return lastKeyboardState.IsKeyUp(key) 
				&& keyboardState.IsKeyDown(key);
		}

		public static bool WasButtonPressed(Buttons button)
		{
			return lastGamepadState.IsButtonUp(button) 
				&& gamepadState.IsButtonDown(button);
		}

		public static Vector2 GetMovementDirection()
		{
			
			Vector2 direction = gamepadState.ThumbSticks.Left;
			direction.Y *= -1;	// invert the y-axis

			if (keyboardState.IsKeyDown(Keys.A))
				direction.X -= 1;
			if (keyboardState.IsKeyDown(Keys.D))
				direction.X += 1;
			if (keyboardState.IsKeyDown(Keys.W))
				direction.Y -= 1;
			if (keyboardState.IsKeyDown(Keys.S))
				direction.Y += 1;

			// Clamp the length of the vector to a maximum of 1.
			if (direction.LengthSquared() > 1)
				direction.Normalize();

			return direction;
		}

		public static Vector2 GetAimDirection()
		{
			if (isAimingWithMouse)
				return GetMouseAimDirection();

			Vector2 direction = gamepadState.ThumbSticks.Right;
			direction.Y *= -1;

			if (keyboardState.IsKeyDown(Keys.Left))
				direction.X -= 1;
			if (keyboardState.IsKeyDown(Keys.Right))
				direction.X += 1;
			if (keyboardState.IsKeyDown(Keys.Up))
				direction.Y -= 1;
			if (keyboardState.IsKeyDown(Keys.Down))
				direction.Y += 1;

			// If there's no aim input, return zero.
			// Otherwise normalize the direction to have a length of 1.
			if (direction == Vector2.Zero)
				return Vector2.Zero;
			else
				return Vector2.Normalize(direction);
		}

		private static Vector2 GetMouseAimDirection()
		{
			Vector2 direction = MousePosition - PlayerShip.Instance.Position;

			if (direction == Vector2.Zero)
				return Vector2.Zero;
			else
				return Vector2.Normalize(direction);
		}

		//RnD
		public static bool WasBombButtonPressed()
		{
			return WasButtonPressed(Buttons.LeftTrigger) 
				|| WasButtonPressed(Buttons.RightTrigger) 
				|| WasKeyPressed(Keys.Space) 
				|| (mouseState.Count > 0);
		}
	}
}