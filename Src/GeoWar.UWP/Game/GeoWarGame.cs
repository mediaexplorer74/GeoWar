using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using System;
using Windows.UI.Xaml.Controls;


namespace GeoWar
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GeoWarGame : Game
    {
        //GraphicsDeviceManager graphics;
        //SpriteBatch spriteBatch;
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        Vector2 baseScreenSize = new Vector2(800, 480);
        private Matrix globalTransformation;
        int backbufferWidth, backbufferHeight;


        //public static GeoWarGame _instance;
        // some helpful static properties
        public static GeoWarGame Instance { get; private set; }

        public static Viewport Viewport
        {
            get
            {
                return Instance.GraphicsDevice.Viewport;
            }
        }

        public static Vector2 ScreenSize
        {
            get
            {
                return new Vector2
                (
                    800/*Viewport.Width*/,
                    480/*Viewport.Height*/
               );
            }
        }

        // DEBUG CODE
        private SpriteFont debugTextFont;
        // DEBUG CODE

        public GeoWarGame()
        {
            Instance = this;

            // here we define the screen
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 800;//1080;
            graphics.PreferredBackBufferWidth = 480;//1920;
            graphics.IsFullScreen = true; // set it true for W10M

            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft
               | DisplayOrientation.LandscapeRight;// | DisplayOrientation.Portrait;

            Content.RootDirectory = "Content";

            // these are to unlink it from MonoGame's default frame rate and to use variable timesteps for update cycles
            Instance.IsFixedTimeStep = false;
        }

       

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //RnD
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //ParticleManager = new ParticleManager<ParticleState>
            //    (1024 * 20,
            //    ParticleState.UpdateParticle
            //    );

            //const int maxGridPoints = 1600;
            //Vector2 gridSpacing = new Vector2((float)Math.Sqrt(
            //    Viewport.Width * Viewport.Height / maxGridPoints));
            //Grid = new Grid(Viewport.Bounds, gridSpacing);

            //RnD
            ScalePresentationArea();



            // load the base initialize first
            base.Initialize();

            // all other initializations come after base
            EntityManager.Add(PlayerShip.Instance);

            // set the music to repeating
            MediaPlayer.IsRepeating = true;
            // play the music defined in the sound class
            MediaPlayer.Play(Sound.Music);

        }

        //!
        public void ScalePresentationArea()
        {
            //Work out how much we need to scale our graphics to fill the screen
            backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth - 0; // 40 - dirty hack for Astoria!
            backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            float horScaling = backbufferWidth / baseScreenSize.X;
            float verScaling = backbufferHeight / baseScreenSize.Y;

            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);

            globalTransformation = Matrix.CreateScale(screenScalingFactor);

            System.Diagnostics.Debug.WriteLine("Screen Size - Width["
                + GraphicsDevice.PresentationParameters.BackBufferWidth + "] " +
                "Height [" + GraphicsDevice.PresentationParameters.BackBufferHeight + "]");
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
            Art.Load(Instance);//(Instance);
            Sound.Load(Instance.Content);//(Instance.Content);

            // DEBUG CODE
            debugTextFont = Content.Load<SpriteFont>("debugText");
            // DEBUG CODE
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            //RnD
            //Confirm the screen has not been resized by the user
            if (backbufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
                backbufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
            {
                ScalePresentationArea();
            }
            // ->1

            Input.Update();
            
            // ->2

            EntityManager.Update(gameTime);
            EnemySpawner.Update(gameTime);
            PlayerStatus.Update(gameTime);

            base.Update(gameTime);
        }

        private void DrawRightAlignedString(string text, float y)
        {
            // this gets the width of the string in pixels
            float textWidth = Art.Font.MeasureString(text).X;
            // draws the string on the top right side of the screen 5 pixels from the right side
            spriteBatch.DrawString(Art.Font, text, new Vector2(ScreenSize.X - textWidth - 5, y), Color.White);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // the sort mode of texture uses the default deferred mode where sprites are drawn in the
            // order that they are called but ordered by texture first (don't really understand what that means though)
            // blendstate of additive adds the destination data to the source data without using alpha its how overlapping
            // textures are blended together
            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive,//);
                null, null, null, null, globalTransformation);

            EntityManager.Draw(spriteBatch);

            // if the player is in a game over state display the score in the middle of the screen
            if (PlayerStatus.IsGameOver)
            {
                string gameOverText = string.Format("Game Over\nYour Score: {0}\nHigh Score: {1}", 
                    PlayerStatus.Score, PlayerStatus.HighScore);
                Vector2 textSize = Art.Font.MeasureString(gameOverText);
                // this is using vector math to find the centre of the screen and position the text right in
                // the middle of it textSize/2 get the middle of the text screensize/2 gets the middle of the
                // screen
                spriteBatch.DrawString(Art.Font, gameOverText, ScreenSize / 2 - textSize / 2, 
                    Color.White);
            }

            // draw the players lives on the top left of the screen 5,5
            spriteBatch.DrawString(Art.Font, string.Format("Lives: {0}", PlayerStatus.Lives), 
                new Vector2(5), Color.White);
            DrawRightAlignedString(string.Format("Score: {0}", PlayerStatus.Score), 5);
            DrawRightAlignedString(string.Format("Multiplier: X{0}", PlayerStatus.Multiplier), 35);

            // draw the mouse pointer if the player is aiming with the mouse
            if (Input.isAimingWithMouse == true)
            {
                spriteBatch.Draw(Art.Pointer, Input.MousePosition, Color.White);
            }

            // DEBUG CODE
            string aimMode = Input.isAimingWithMouse ? "Mouse Aim" : "Keyboard aim";
            //spriteBatch.DrawString(debugTextFont, string.Format("Control: {3}\nAim: {0}\nMove: {1}\nOrientation: {2}\nSpawn Chance: 1 in {4}", Input.GetAimDirection(),  Input.GetMovementDirection(), PlayerShip.Instance.Orientation, aimMode, EnemySpawner.inverseSpawnchance), new Vector2(50, 70), Color.DeepSkyBlue);
            // DEBUG CODE

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
