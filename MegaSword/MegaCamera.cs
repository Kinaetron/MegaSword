using System;

using Microsoft.Xna.Framework;

using PolyOne.Engine;
using PolyOne.Utility;

namespace MegaSword
{
    public class MegaCamera : Camera
    {
        public bool IsShaking
        {
            get { return pushforward; }
        }

        private float pushDirection;
        private float strength = 1.0f;
        private bool pushforward = false;
        private float shakeTimer;
        private const float shakeTime = 50.0f;
        private Vector2 prevPositon = Vector2.Zero;


        private const float cameraLerpFactorSide = 0.05f;
        private float cameraLerpSpeed;

        private const float cameraLerpFactorUp = 0.04f;
        private float multiplyBy = 0;
        private float newX;

        private Rectangle cameraTrap;
        public Rectangle CameraTrap
        {
            get { return cameraTrap; }
            set { cameraTrap = value; }
        }

        public void PushForward(float swingDirection)
        {
            if (pushforward == false)
            {
                pushDirection = swingDirection;
                shakeTimer = shakeTime;
                pushforward = true;
                prevPositon = Position;
            }
        }

        public void LockToTarget(Rectangle collider, int screenWidth, int screenHeight)
        {
            if (collider.Right > CameraTrap.Right)
            {
                multiplyBy = 0.4f;
                cameraLerpSpeed += cameraLerpFactorSide;
                cameraTrap.X = collider.Right - CameraTrap.Width;
            }

            if (collider.Left < CameraTrap.Left)
            {
                multiplyBy = 0.6f;
                cameraLerpSpeed += cameraLerpFactorSide;
                cameraTrap.X = collider.Left;
            }

            if(collider.Right < CameraTrap.Right && collider.Left > CameraTrap.Left) {
                cameraLerpSpeed = 0.0f;
            }

            cameraLerpSpeed = MathHelper.Clamp(cameraLerpSpeed, 0.0f, 0.6f);

            if (collider.Bottom > CameraTrap.Bottom) {
                cameraTrap.Y = collider.Bottom - CameraTrap.Height;
            }

            if (collider.Top < CameraTrap.Top) {
                cameraTrap.Y = collider.Top;
            }

            newX = cameraTrap.X + (cameraTrap.Width * multiplyBy) - (screenWidth * multiplyBy);

            if(pushforward == false) {
                Position.X = (int)Math.Round(MathHelper.Lerp(Position.X, newX, cameraLerpSpeed));
                Position.Y = (int)Math.Round((double)cameraTrap.Y + (cameraTrap.Height / 2) - (screenHeight / 2));
            }

        }

        public void MoveTrapUp(float target)
        {
            float moveCamera = target - cameraTrap.Height;
            cameraTrap.Y = (int)MathHelper.Lerp(CameraTrap.Y, moveCamera, cameraLerpFactorUp);
        }

        public void Update()
        {
            if(shakeTimer <= 0 && pushforward == true)
            {
                shakeTimer = 0;
                Position.Y = prevPositon.Y;
                pushforward = false;
            }

            if (pushforward == true) {
                shakeTimer -= Engine.DeltaTime;
                Position.Y += strength * pushDirection;
            }
        }
    }
}