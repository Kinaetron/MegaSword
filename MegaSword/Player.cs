using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using PolyOne;
using PolyOne.Collision;
using PolyOne.Engine;
using PolyOne.Scenes;
using PolyOne.Input;
using PolyOne.Components;
using PolyOne.Animation;
using PolyOne.HitboxProcessor;
using PolyOne.InputManager;

using MegaSword.Platforms;

namespace MegaSword
{
    public enum PlayerAction
    {
        Idle = 0,
        Running = 1,
        Jumping = 2,
        Falling = 3,
        Attack1 = 4,
        Attack2 = 5,
        Attack3 = 6,
        MoveAttack = 7,
        AirAttack1 = 8,
        AirAttack2 = 9,
        Parry = 10
    }

    public class Player : Entity
    {
        private Texture2D playerTexture;
        private Texture2D boxTexture;

        private Texture2D healthTexture;
        private Texture2D bulletPointTexture;

        public bool IsOnJumpThrough { get; set; }
        private bool isInAir;
        private bool isOnGround;
        private bool isOnPlatform;
        private bool isOnLeftEdge;
        private bool isOnRightEdge;
        private bool playerNextToRight;
        private bool playerNextToLeft;
        private const float gravityUp = 0.31f;
        private const float gravityDown = 0.21f;
        private Vector2 remainder;
        private Vector2 velocity;
        private Vector2 prevVelocity;

        private const float fallspeed = 6.0f;
        private const float airFriction = 0.66f;
        private const float initialJumpHeight = -5.8f;
        private const float normMaxHorizSpeed = 4.0f;

        private const float runAccel = 1.00f;
        private const float turnMul = 0.9f;

        private bool buttonPushed;
        private const float graceTime = 66.9f;
        private const float graceTimePush = 66.9f;

        private int sign;
        private int lastSign;
        private bool controllerMode;
        private bool keyboardMode;
        private List<Keys> keyList = new List<Keys>(new Keys[] { Keys.W, Keys.A, Keys.S, Keys.D, Keys.Up,
                                                                 Keys.Down, Keys.Left, Keys.Right ,Keys.Space });

        private const int bulletLimit = 5;
        private int bulletCount = bulletLimit;

        CounterSet<string> counters = new CounterSet<string>();

        private bool cameraUp;
        private MegaCamera camera = new MegaCamera();
        public MegaCamera Camera
        {
            get { return camera; }
        }

        private Level level;

        private AnimationPlayer sprite;

        private AnimationData idleAnimation;
        private AnimationData runAnimation;
        private AnimationData jumpAnimation;
        private AnimationData fallAnimation;

        private AnimationData attack1Animation;
        private AnimationData attack2Animation;
        private AnimationData attack3Animation;
        private AnimationData airAttackAnimation1;
        private AnimationData airAttackAnimation2;

        private AnimationData parryAnimation;

        private HitboxData groundAttack1;
        private HitboxData groundAttack2;
        private HitboxData groundAttack3;

        private HitboxData parryBox;

        private const float pauseTime = 66.8f;

        private int parryCount;
        private const int parryLimit = 3;
        private const float parryTimer = 2000.0f;
        private bool parryTimerOn = false;

        string attackName1 = "Ground Attack 1";
        string attackName2 = "Ground Attack 2";
        string attackName3 = "Ground Attack 3";

        string airAttackName1 = "Air Attack 1";
        string airAttackName2 = "Air Attack 2";

        private SpriteEffects flip;
        public PlayerAction Action { private set; get; }

        private Move[] moves;
        private Move playerMove;
        private MoveList moveList;
        private ComboManager comboManager;

        public List<Hitbox> Hitboxes
        {
            get { return sprite.Hitboxes.Hitboxes; }
        }

        public List<Hitbox> Hurtboxes
        {
            get { return sprite.Hitboxes.Hurtboxes; }
        }

        public float HurtPoints { get; private set; }

        private const float standingAttackPoints = 10;
        private const float movingAttackPoints = 5;
        private const float airAttackPoints = 5;

        private int hitPoints = 4;

        private const float invicibleTime = 1000.0f;
        private Color colour;

        private bool stopMovement;
        private bool stopFlip;

        private float attackMoveTime;
        private float attackMoveVel;

        private float airTime;
        private bool airTimeBool = false;
        private bool airAttackDone = false; 

        public bool IsDead { get; private set; }

        public Player(Vector2 position)
         : base(position)
        {
            this.Tag((int)GameTags.Player);
            this.Collider = new Hitbox((float)32.0f, (float)32.0f, -16.0f, -6.0f);
            this.Visible = true;

            sprite = new AnimationPlayer();

            playerTexture = Engine.Instance.Content.Load<Texture2D>("Player/Idle");
            healthTexture = Engine.Instance.Content.Load<Texture2D>("Player/Healthpoint");
            bulletPointTexture = Engine.Instance.Content.Load<Texture2D>("Player/bulletPoint");

            idleAnimation = new AnimationData(Engine.Instance.Content.Load<Texture2D>("Player/Idle"), 200, 96, true);
            runAnimation = new AnimationData(Engine.Instance.Content.Load<Texture2D>("Player/Run"), 75, 96, true);
            jumpAnimation = new AnimationData(Engine.Instance.Content.Load<Texture2D>("Player/Jump"), 200, 96, true);
            fallAnimation = new AnimationData(Engine.Instance.Content.Load<Texture2D>("Player/Fall"), 200, 96, true);


            groundAttack1 = Engine.Instance.Content.Load<HitboxData>("Player/Combo1Box");
            groundAttack2 = Engine.Instance.Content.Load<HitboxData>("Player/Combo2Box");
            groundAttack3 = Engine.Instance.Content.Load<HitboxData>("Player/Combo3Box");
            parryBox = Engine.Instance.Content.Load<HitboxData>("Player/ParryBox");

            attack1Animation = new AnimationData(Engine.Instance.Content.Load<Texture2D>("Player/Combo1"), 50, 96, false, groundAttack1);
            attack2Animation = new AnimationData(Engine.Instance.Content.Load<Texture2D>("Player/Combo2"), 50, 96, false, groundAttack2);
            attack3Animation = new AnimationData(Engine.Instance.Content.Load<Texture2D>("Player/Combo1"), 50, 96, false, groundAttack3);

            airAttackAnimation1 = new AnimationData(Engine.Instance.Content.Load<Texture2D>("Player/Combo1"), 50, 96, false, groundAttack1);
            airAttackAnimation2 = new AnimationData(Engine.Instance.Content.Load<Texture2D>("Player/Combo2"), 50, 96, false, groundAttack2);

            parryAnimation = new AnimationData(Engine.Instance.Content.Load<Texture2D>("Player/Parry"), 50, 96, false, parryBox);

            flip = SpriteEffects.None;
            sprite.PlayAnimation(idleAnimation);

            boxTexture = Engine.Instance.Content.Load<Texture2D>("Tiles/Red");
            Camera.CameraTrap = new Rectangle((int)this.Right, (int)this.Bottom - 100, 50, 70);


            moves = new Move[]
            {
                new Move(attackName1,  Buttons.X) { IsSubMove = true, IsVertMove = true },
                new Move(attackName2,  Buttons.X, Buttons.X) { IsSubMove = true, IsVertMove = true },
                new Move(attackName3,  Buttons.X, Buttons.X, Buttons.X) { IsVertMove = true },

                new Move(attackName1,  Buttons.X) { IsSubMove = true },
                new Move(attackName2,  Buttons.X, Buttons.X) { IsSubMove = true },
                new Move(attackName3,  Buttons.X, Buttons.X, Buttons.X),

                new Move(airAttackName1,   Buttons.X) { IsSubMove = true, IsAirMove = true },
                new Move(airAttackName1,   Buttons.X) { IsSubMove = true, IsAirMove = true, IsVertMove = true },

                new Move(airAttackName2,   Buttons.X, Buttons.X) { IsAirMove = true },
                new Move(airAttackName2,   Buttons.X, Buttons.X) { IsAirMove = true, IsVertMove = true }
            };

            moveList = new MoveList(moves);
            comboManager = new ComboManager((PlayerIndex.One), moveList.LongestMoveLength);

            this.Add(counters);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (base.Scene is Level) {
                this.level = (base.Scene as Level);
            }
        }

        public override void Update()
        {
            base.Update();

            Input();
            comboManager.Update();
            MovementState();
            Movement();
            Jump();
            Shoot();
            sprite.Update();
            AnimationState();
            MoveWithPlatform();
            IsOnGround();
            Parry();
            ParryTimer();


            TranslateUpdate();
            AirTimeUpdate();
            IsOnEdge();


            isInAir = false;

            if(hitPoints <= 0) {
                IsDead = true;
            }

            if (isOnGround == false || counters["graceTimer"] <= 0) {
                isInAir = true;
            }

            if(isOnGround == true || counters["graceTimer"] > 0) {
                airAttackDone = false;
            }

            bool isMoving = Math.Abs(velocity.X) > 0;

            Move newMove = moveList.DetectMove(comboManager, isInAir, isMoving, Buttons.A);

            if(newMove != null)
            {
                if(newMove.Name == attackName1 && attack3Animation.AnimationFinished == false && 
                   Action == PlayerAction.Attack3)
                {
                    comboManager.Cancel();
                    return;
                }

                if(airAttackDone == true) {
                    comboManager.Cancel();
                    return;
                }

                playerMove = newMove;
            }

            if (isOnGround == true && buttonPushed == false) {
                Camera.MoveTrapUp(Position.Y);
            }

            velocity.X = MathHelper.Clamp(velocity.X, -normMaxHorizSpeed, normMaxHorizSpeed);
            MovementHorizontal(velocity.X);

            velocity.Y = MathHelper.Clamp(velocity.Y, initialJumpHeight, fallspeed);
            MovementVerical(velocity.Y);

            Camera.LockToTarget(this.Rectangle, Engine.VirtualWidth, Engine.VirtualHeight);
            Camera.ClampToArea((int)level.Tile.MapWidthInPixels - Engine.VirtualWidth, (int)level.Tile.MapHeightInPixels - Engine.VirtualHeight);
            camera.Update();

            prevVelocity = velocity;

            if(counters["invicibleTimer"] > 0) {
                colour = Color.Green;
            }
            else {
                colour = Color.White;
            }
        }

        private void IsOnGround()
        {
            isOnGround = base.CollideCheck((int)GameTags.Solid, this.Position + Vector2.UnitY);

            if (isOnGround == false) {
                isOnGround = base.CollideCheck((int)GameTags.FallingPlatform, this.Position + Vector2.UnitY);
            }

            if (isOnGround == false && playerNextToLeft == false && playerNextToRight == false) {
                isOnGround = base.CollideCheck((int)GameTags.MovingPlatform, this.Position + Vector2.UnitY);
            }

            if (isOnGround == false) {
                isOnGround = base.CollideCheck((int)GameTags.MovingPlatformVert, this.Position - Vector2.UnitY);
            }

            if (isOnGround == false) {
                isOnGround = base.CollideCheck((int)GameTags.Enemy, this.Position +  Vector2.UnitY);
            }

            using (List<Entity>.Enumerator enumerator = base.Scene[(int)GameTags.Oneway].GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Oneway one = (Oneway)enumerator.Current;
                    if (one.IsOnJumpThrough == true && isOnGround == false)
                    {
                        isOnGround = true;
                    }
                }
            }

            using (List<Entity>.Enumerator enumerator = base.Scene[(int)GameTags.MovingPlatformVert].GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MovingPlatformVert one = (MovingPlatformVert)enumerator.Current;
                    if (one.IsPlayerOn == true && isOnGround == false) {
                        isOnGround = true;
                    }
                }
            }
        }

        private void IsOnEdge()
        {
            isOnLeftEdge = false;
            isOnRightEdge = false;

            bool isOnSolid = base.CollideCheck((int)GameTags.Solid, this.Position + Vector2.UnitY);

            if (isOnSolid == true)
            {
                isOnRightEdge = !base.CollideCheck((int)GameTags.Solid, new Vector2(this.Position.X + this.Width, this.Position.Y + 1));
                isOnLeftEdge = !base.CollideCheck((int)GameTags.Solid, new Vector2(this.Position.X - this.Width, this.Position.Y + 1));
            }

            bool isOnOneWay = base.CollideCheck((int)GameTags.Oneway, this.Position + Vector2.UnitY);

            if (isOnOneWay == true)
            {
                isOnRightEdge = !base.CollideCheck((int)GameTags.Oneway, new Vector2(this.Position.X + this.Width, this.Position.Y + 1));
                isOnLeftEdge = !base.CollideCheck((int)GameTags.Oneway, new Vector2(this.Position.X - this.Width, this.Position.Y + 1));
            }

            bool isOnFallingPlat = base.CollideCheck((int)GameTags.FallingPlatform, this.Position + Vector2.UnitY);

            if (isOnFallingPlat == true)
            {
                using (List<Entity>.Enumerator enumerator = base.Scene[(int)GameTags.FallingPlatform].GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        FallingPlatform falling = (FallingPlatform)enumerator.Current;

                        if (this.Right >= falling.Right) {
                            isOnRightEdge = true;
                        }
                        else if (this.Left <= falling.Left) {
                            isOnLeftEdge = true;
                        }
                    }
                }
            }

            bool isOnMovingPlat = base.CollideCheck((int)GameTags.MovingPlatform, this.Position + Vector2.UnitY);

            if (isOnMovingPlat == true)
            {
                using (List<Entity>.Enumerator enumerator = base.Scene[(int)GameTags.MovingPlatform].GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MovingPlatform movingPlat = (MovingPlatform)enumerator.Current;

                        if (this.Right >= movingPlat.Right) {
                            isOnRightEdge = true;
                        }
                        else if (this.Left <= movingPlat.Left) {
                            isOnLeftEdge = true;
                        }
                    }
                }
            }

            if (isOnRightEdge == false && isOnLeftEdge == false)
            {
                using (List<Entity>.Enumerator enumerator = base.Scene[(int)GameTags.MovingPlatformVert].GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MovingPlatformVert one = (MovingPlatformVert)enumerator.Current;
                        if (one.IsPlayerOn == true)
                        {
                            if (this.Right >= one.Right) {
                                isOnRightEdge = true;
                            }
                            else if (this.Left <= one.Left) {
                                isOnLeftEdge = true;
                            }
                        }
                    }
                }
            }
        }

        private void MoveWithPlatform()
        {
            isOnPlatform = false;

            using (List<Entity>.Enumerator enumerator = base.Scene[(int)GameTags.MovingPlatform].GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MovingPlatform platform = (MovingPlatform)enumerator.Current;

                    if (platform.IsPlayerOn == true && sign == 0) {
                        velocity.X = platform.Velocity;
                    }

                    playerNextToRight = base.CollideCheck((int)GameTags.MovingPlatform, Position - Vector2.UnitX);
                    playerNextToLeft = base.CollideCheck((int)GameTags.MovingPlatform, Position + Vector2.UnitX);

                    if (playerNextToRight == true || playerNextToLeft == true) {
                        velocity.X = platform.Velocity;
                    }
                }
            }

            using (List<Entity>.Enumerator enumerator = base.Scene[(int)GameTags.MovingPlatformVert].GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MovingPlatformVert platform = (MovingPlatformVert)enumerator.Current;

                    if (platform.IsPlayerOn == true && buttonPushed == false)
                    {
                        velocity.Y = platform.Velocity;
                        isOnPlatform = true;
                    }
                }
            }
        }


        private void Shoot()
        {
            if(controllerMode == true)
            {
                if(PolyInput.GamePads[0].Pressed(Buttons.B) == true) {
                    BulletCreation();
                }
            }
            else if(keyboardMode == true)
            {
                if(PolyInput.Keyboard.Pressed(Keys.B) == true) {
                    BulletCreation();
                }
            }
        }

        private void BulletCreation()
        {
            if (bulletCount <= 0)
                return;

            bulletCount--;
            PlayerBullet bullet;

            if (flip == SpriteEffects.None) {
                bullet = new PlayerBullet(new Vector2(Position.X + 10, Position.Y + 8), BulletDirection.Right);
            }
            else {
                bullet = new PlayerBullet(new Vector2(Position.X - 30, Position.Y + 8), BulletDirection.Left);
            }

            this.Scene.Add(bullet);
            bullet.Added(this.Scene);
        }

        private void MovementState()
        {
            if((Action == PlayerAction.Attack1 || Action == PlayerAction.Attack2 || 
                Action == PlayerAction.Attack3) || Action == PlayerAction.AirAttack1 ||
                Action == PlayerAction.AirAttack2 || Action == PlayerAction.Parry)
            {
                stopMovement = true;
            }

            if (Action != PlayerAction.Attack1 && Action != PlayerAction.Attack2 &&
                Action != PlayerAction.Attack3 && Action != PlayerAction.AirAttack1 &&
                Action != PlayerAction.AirAttack2 && Action != PlayerAction.Parry)
            {
                stopMovement = false;
            }
        }

        private void Input()
        {
            InputMode();

            sign = 0;

            if (controllerMode == true && stopMovement == false)
            {
                if (PolyInput.GamePads[0].LeftStickHorizontal(0.3f) > 0.4f ||
                    PolyInput.GamePads[0].DPadRightCheck == true)
                {
                    sign = 1;
                }
                else if (PolyInput.GamePads[0].LeftStickHorizontal(0.3f) < -0.4f ||
                         PolyInput.GamePads[0].DPadLeftCheck == true)
                {
                    sign = -1;
                }

                if(sign != 0) {
                    lastSign = sign;
                }
            }
            else if (keyboardMode == true && stopMovement == false)
            {
                if (PolyInput.Keyboard.Check(Keys.Right) ||
                    PolyInput.Keyboard.Check(Keys.D))
                {
                    sign = 1;
                }
                else if (PolyInput.Keyboard.Check(Keys.Left) ||
                         PolyInput.Keyboard.Check(Keys.A))
                {
                    sign = -1;
                }

                if (sign != 0) {
                    lastSign = sign;
                }
            }
        }

        public void HitEnemy()
        {
            sprite.PauseAnimation(pauseTime);
            PolyInput.GamePads[0].Rumble(0.8f, 50.0f);

            if(Action == PlayerAction.Attack1 || Action == PlayerAction.AirAttack1 || 
               Action == PlayerAction.AirAttack2)
            {
                camera.PushForward(1);
            }
            else {
                camera.PushForward(-1);
            }

            if(counters["stopTimer"] <= 0) {
                velocity.X = 0;
                counters["stopTimer"] = 50.0f;
            }

            if(bulletCount < bulletLimit) {
                bulletCount++;
            }
        }

        public void Hurt(int hurtPoints)
        {
            if(counters["invicibleTimer"] <= 0)
            {
                counters["invicibleTimer"] = invicibleTime;
                sprite.PauseAnimation(pauseTime);
                PolyInput.GamePads[0].Rumble(0.8f, 50.0f);
                hitPoints -= hurtPoints;
            }
        }

        public void Pause(float time) {
            sprite.PauseAnimation(time);
        }

        private void AnimationState()
        {
            stopFlip = false;
            
            if((Action == PlayerAction.AirAttack1 || Action == PlayerAction.AirAttack2)
               && isOnGround == true)
            {
                playerMove = null;
                comboManager.Cancel();
            }

            if(playerMove == null)
            {
                if ((isOnGround == false && counters["graceTimer"] <= 0) && Math.Abs(velocity.Y) > Math.Abs(prevVelocity.Y) && Action != PlayerAction.Parry) {
                    Action = PlayerAction.Falling;
                }
                else if (sign != 0 && isOnGround == true && Action != PlayerAction.Jumping && 
                    Action != PlayerAction.Parry)
                {
                    Action = PlayerAction.Running;
                }
                else if (sign == 0 && isOnGround == true && Action != PlayerAction.Jumping && 
                    Action != PlayerAction.Parry)
                {
                    Action = PlayerAction.Idle;
                }
            }

            if(playerMove != null)
            {
                if (playerMove.Name == attackName1)
                {
                    Action = PlayerAction.Attack1;
                    HurtPoints = standingAttackPoints;
                }
                else if (playerMove.Name == attackName2)
                {
                    if (attack1Animation.NextMove == true)
                    {
                        attack1Animation.NextMove = false;
                        Action = PlayerAction.Attack2;
                        HurtPoints = standingAttackPoints;
                    }
                }
                else if (playerMove.Name == attackName3)
                {
                    if (attack2Animation.NextMove == true)
                    {
                        attack2Animation.NextMove = false;
                        Action = PlayerAction.Attack3;
                        HurtPoints = standingAttackPoints;
                    }
                }
                else if (playerMove.Name == airAttackName1)
                {
                    Action = PlayerAction.AirAttack1;
                    HurtPoints = airAttackPoints;
                }
                else if (playerMove.Name == airAttackName2)
                {
                    Action = PlayerAction.AirAttack2;
                    HurtPoints = airAttackPoints;
                }
            }

            if (Action == PlayerAction.Running) {
                sprite.PlayAnimation(runAnimation);
            }

            if (Action == PlayerAction.Jumping)
            {
                sprite.PlayAnimation(jumpAnimation);
            }

            if(Action == PlayerAction.Parry)
            {
                sprite.PlayAnimation(parryAnimation);

                if (sprite.FrameIndex == 3) {
                    Action = PlayerAction.Idle;
                }

                if(isOnGround == false && sprite.FrameIndex == 0) {
                    AirTime(100.2f);
                }
            }

            if (Action == PlayerAction.Falling) {
                sprite.PlayAnimation(fallAnimation);
            }

            if (Action == PlayerAction.Attack1) {
                sprite.PlayAnimation(attack1Animation);

                if (sprite.FrameIndex == 0) {
                    Translate(3.0f, 16.7f);
                }
            }


            if (Action == PlayerAction.Attack2) {
                sprite.PlayAnimation(attack2Animation);

                if (sprite.FrameIndex == 0) {
                    Translate(3.0f, 33.4f);
                }
            }

            if (Action == PlayerAction.Attack3) {
                sprite.PlayAnimation(attack3Animation);

                if (sprite.FrameIndex == 0) {
                    Translate(3.0f, 50.1f);
                }
            }

            if (Action == PlayerAction.AirAttack1)
            {
                sprite.PlayAnimation(airAttackAnimation1);
                stopFlip = true;

                if (sprite.FrameIndex == 0) {
                    AirTime(100.2f);
                }
            }

            if (Action == PlayerAction.AirAttack2)
            {
                sprite.PlayAnimation(airAttackAnimation2);
                stopFlip = true;

                if (sprite.FrameIndex == 0) {
                    AirTime(100.2f);
                    airAttackDone = true;
                }
            }

            if (Action == PlayerAction.Idle) {
                sprite.PlayAnimation(idleAnimation);
            }


            if(Action == PlayerAction.Attack1 && attack1Animation.NextMove == true) {
                playerMove = null;
            }

            if (Action == PlayerAction.Attack2 && attack2Animation.NextMove == true) {
                playerMove = null;
            }

            if (Action == PlayerAction.Attack3 && attack3Animation.AnimationFinished == true) {
                playerMove = null;
            }

            if (Action == PlayerAction.AirAttack1 && airAttackAnimation1.AnimationFinished == true) {
                playerMove = null;
            }

            if (Action == PlayerAction.AirAttack2 && airAttackAnimation2.AnimationFinished == true) {
                playerMove = null;
            }

            if (sign < 0 && stopMovement == false && stopFlip == false) {
                flip = SpriteEffects.FlipHorizontally;
            }
            else if (sign > 0 && stopMovement == false && stopFlip == false) {
                flip = SpriteEffects.None;
            }
        }

        private void Translate(float vel, float time)
        {
            if(isOnRightEdge == true && flip == SpriteEffects.None) {
                return;
            }

            if (isOnLeftEdge == true && flip == SpriteEffects.FlipHorizontally) {
                return;
            }

            if (attackMoveTime <= 0 || vel > attackMoveVel)
            {
                attackMoveTime = time;
                attackMoveVel = vel;
            }
        }

        private void TranslateUpdate()
        {
            if(attackMoveTime > 0)
            {
                attackMoveTime -= Engine.DeltaTime;
                float movement = (flip == SpriteEffects.None) ? 1 : -1;

                velocity.X = attackMoveVel * movement;
                MovementHorizontal(velocity.X);
            }
        }

        private void AirTime(float time)
        {
            if(airTime <= 0) {
                airTime = time;
                velocity.Y = 0;
            }
        }

        private void AirTimeUpdate()
        {
            if(airTime > 0)
            {
                airTime -= Engine.DeltaTime;
                airTimeBool = true;
            }
            else {
                airTimeBool = false;
            }
        }

        private void InputMode()
        {
            foreach (Keys key in keyList)
            {
                if (PolyInput.Keyboard.Check(key) == true)
                {
                    controllerMode = false;
                    keyboardMode = true;
                }
            }
            if (PolyInput.GamePads[0].ButtonCheck() == true)
            {
                controllerMode = true;
                keyboardMode = false;
            }

            if (controllerMode == false && keyboardMode == false) {
                keyboardMode = true;
            }
        }

        private void Movement()
        {
            float currentSign = Math.Sign(velocity.X);

            if(isOnGround == false) {
                velocity.X *= airFriction;
            }

            if (sign != 0 && currentSign != sign) {
                velocity.X *= turnMul;
            }

            if (graceTimePush > 0 && velocity.Y < 0 && isOnGround == false && airTimeBool == false) {
                velocity.Y += gravityUp;
            }
            else if(isOnGround == false && airTimeBool == false) {
                velocity.Y += gravityDown;
            }

            if (counters["stopTimer"] <= 0 && stopMovement == false) {
                velocity.X += runAccel * sign;
            }

            if (sign == 0 && playerNextToRight == false && 
                             playerNextToLeft == false)
            {
                velocity.X = 0.0f;
            }
        }

        private void Jump()
        {
            if (isOnGround == true)
            {
                buttonPushed = false;
                counters["graceTimer"] = graceTime;
            }

            if (controllerMode == true)
            {
                if (PolyInput.GamePads[0].Pressed(Buttons.A) == true) {
                    counters["graceTimerPush"] = graceTimePush;
                    cameraUp = true;
                }

                if (counters["graceTimerPush"] > 0)
                {
                    if (isOnGround == true || counters["graceTimer"] > 0)
                    {
                        buttonPushed = true;
                        counters["graceTimerPush"] = 0.0f;
                        velocity.Y = initialJumpHeight;
                        Action = PlayerAction.Jumping;
                        comboManager.Cancel();
                        playerMove = null;
                    }
                }
                else if (PolyInput.GamePads[0].Released(Buttons.A) == true && velocity.Y < 0.0f)
                {
                    counters["graceTimerPush"] = 0.0f;
                    velocity.Y = 0.0f;
                }
            }
            else if (keyboardMode == true)
            {
                if (PolyInput.Keyboard.Pressed(Keys.Space) == true) {
                    counters["graceTimerPush"] = graceTimePush;
                }

                if (counters["graceTimerPush"] > 0)
                {
                    if (isOnGround == true || counters["graceTimer"] > 0)
                    {
                        buttonPushed = true;
                        counters["graceTimerPush"] = 0.0f;
                        velocity.Y = initialJumpHeight;
                        Action = PlayerAction.Jumping;
                        comboManager.Cancel();
                        playerMove = null;
                    }
                }
                else if (PolyInput.Keyboard.Released(Keys.Space) == true && velocity.Y < 0.0f)
                {
                    counters["graceTimerPush"] = 0.0f;
                    velocity.Y = 0.0f;
                }
            }
        }

        private void Parry()
        {
            if(controllerMode == true)
            {
                if (PolyInput.GamePads[0].RightTriggerPressed(0.6f) == true) {
                    ParryAction();
                }
            }
            else if(keyboardMode == true)
            {
                if (PolyInput.Keyboard.Pressed(Keys.V) == true) {
                    ParryAction();
                }
            }
        }

        private void ParryAction()
        {
            if(parryCount >= parryLimit) {
                return;
            }

            parryCount++;
            Action = PlayerAction.Parry;
            comboManager.Cancel();
            playerMove = null;
        }

        private void ParryTimer()
        {
            if(counters["parryTime"] <= 0 && parryTimerOn == true) {
                parryCount = 0;
                parryTimerOn = false;
            }

            if(parryCount == 1 && parryTimerOn == false) {
                counters["parryTime"] = parryTimer;
                parryTimerOn = true;
            }
        }

        private void MovementHorizontal(float amount)
        {
            remainder.X += amount;
            int move = (int)Math.Round((double)remainder.X);

            if (move != 0)
            {
                remainder.X -= move;
                int sign = Math.Sign(move);

                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(sign, 0);

                    if (this.CollideFirst((int)GameTags.Enemy, newPosition) != null)
                    { 
                        remainder.X = 0;
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.Solid, newPosition) != null)
                    {
                        remainder.X = 0;
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.MovingPlatform, newPosition) != null)
                    {
                        remainder.X = 0;
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.MovingPlatformVert, newPosition) != null && isOnPlatform == false)
                    {
                        remainder.X = 0;
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.FallingPlatform, newPosition) != null)
                    {
                        remainder.X = 0;
                        break;
                    }
                    Position.X += sign;
                    move -= sign;
                }
            }
        }

        private void MovementVerical(float amount)
        {
            remainder.Y += amount;
            int move = (int)Math.Round((double)remainder.Y);

            if (move < 0)
            {
                remainder.Y -= move;
                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(0, -1.0f);

                    if (this.CollideFirst((int)GameTags.Enemy, newPosition) != null)
                    {
                        remainder.Y = 0;
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.Solid, newPosition) != null)
                    {
                        remainder.Y = 0;
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.MovingPlatform, newPosition) != null)
                    {
                        remainder.Y = 0;
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.FallingPlatform, newPosition) != null)
                    {
                        remainder.Y = 0;
                        break;
                    }
                    Position.Y += -1.0f;
                    move -= -1;
                }
            }
            else if (move > 0)
            {
                remainder.Y -= move;
                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(0, 1.0f);

                    if (this.CollideFirst((int)GameTags.Enemy, newPosition) != null)
                    {
                        remainder.Y = 0;
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.Solid, newPosition) != null)
                    {
                        remainder.Y = 0;
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.MovingPlatform, newPosition) != null)
                    {
                        remainder.Y = 0;
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.MovingPlatformVert, newPosition) != null)
                    {
                        remainder.Y = 0;
                        break;
                    }

                    if (this.CollideFirst((int)GameTags.FallingPlatform, newPosition) != null)
                    {
                        remainder.Y = 0;
                        break;
                    }

                    if (this.CollideFirstOutside((int)GameTags.Oneway, newPosition) != null)
                    {
                        remainder.Y = 0;
                        break;
                    }
                    Position.Y += 1.0f;
                    move -= 1;
                }
            }
        }

        public override void Draw()
        {
            //Engine.SpriteBatch.Draw(playerTexture, new Vector2(Position.X - Collider.Width, Position.Y - Collider.Height), Color.White);
            sprite.Draw(new Vector2(this.Position.X, this.Position.Y), 0.0f, flip, colour);

            for (int i = 0; i < hitPoints; i++) {
                Engine.SpriteBatch.Draw(healthTexture, new Vector2(camera.Position.X + 15 * i + 20, camera.Position.Y + 20), Color.White);
            }

            for (int i = 0; i < bulletCount; i++) {
                Engine.SpriteBatch.Draw(bulletPointTexture, new Vector2(camera.Position.X + 15 * i + 20, camera.Position.Y + 40), Color.White);
            }

            //if (sprite.Hitboxes != null)
            //{
            //    foreach (var item in Hitboxes) {
            //        Engine.SpriteBatch.Draw(boxTexture, new Rectangle((int)item.Position.X, (int)item.Position.Y, (int)item.Width, (int)item.Height), Color.White);
            //    }

            //    foreach (var ret in Hurtboxes) {
            //        Engine.SpriteBatch.Draw(boxTexture, new Rectangle((int)ret.Position.X, (int)ret.Position.Y, (int)ret.Width, (int)ret.Height), Color.Blue);
            //    }
            //}
            //Engine.SpriteBatch.Draw(boxTexture, new Rectangle((int)(Position.X + Collider.Position.X), (int)(Position.Y + Collider.Position.Y), (int)Collider.Width, (int)Collider.Height), Color.White);
            //Engine.SpriteBatch.Draw(boxTexture, camera.CameraTrap, Color.White);

            base.Draw();
        }
    }
}
