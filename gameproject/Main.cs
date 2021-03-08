using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using QuakeConsole;
using System.Linq;
using System.Collections.Generic;
using System;

namespace gameproject
{
    public class Main : Game
    {
        public static (int Width, int Height) Resolution = (1280, 720);
        public static Vector2 WindowCentre = Vector2.Zero;

        public static ConsoleComponent Console; // text console for manipulating some variables at runtime
        public static PythonInterpreter Interpreter = new PythonInterpreter(); // python interpreter for user input
        public FrameCount FPS = new FrameCount(); // frames per second counter
        private GraphicsDeviceManager _graphics;
        private SpriteBatch SprBatch;
        private SpriteFont Font;

        public Dictionary<string, Model> Models = new Dictionary<string, Model>(); // dictionary for storing 3d models
        public Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>(); // dictionary for storing textures
        public List<Cube> World = new List<Cube>(); // world is constructed out of cubes

        public List<Entity> Entities = new List<Entity>();
        public Player BasePlayer; // current player entity
        public Crosshair CrosshairPlayer; // crosshair
        public BasicEffect Effect;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsFixedTimeStep = false; // allow the game to update as fast as the hardware allows
            IsMouseVisible = false; // hide mouse cursor

            Console = new ConsoleComponent(this);
            Console.Padding = 10f;
            Console.InputPrefixColor = Color.MonoGameOrange;
            Console.InputPrefix = ">";
            Console.FontColor = Color.White;
            Console.Interpreter = Interpreter;
            SetupCommands();
            Components.Add(Console);
        }

        private void SetupCommands()
        {
            //Interpreter.AddVariable("main", this);
            Interpreter.AddVariable("exit", new Action(Exit)); // add exit command to close game
            Interpreter.AddVariable("connect", new Action<string>(Networking.Connect)); // add command to connect to specified IP, multiplayer
            Interpreter.AddVariable("clear", new Action(() => Console.Clear())); // clear console
        }

        protected override void Initialize()
        {
            // initialise
            _graphics.PreferredBackBufferWidth = Resolution.Width;
            _graphics.PreferredBackBufferHeight = Resolution.Height;
            _graphics.SynchronizeWithVerticalRetrace = false; // disable vsync (syncing with monitor refresh rate)
            _graphics.ApplyChanges();

            Effect = new BasicEffect(GraphicsDevice);
            Effect.LightingEnabled = false;
            Effect.TextureEnabled = false;
            Effect.VertexColorEnabled = true;

            WindowCentre = new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            Mouse.SetPosition((int)WindowCentre.X, (int)WindowCentre.Y);

            // load models and textures from disk
            Models.Add("penguin", Content.Load<Model>("PenguinBaseMesh"));
            Textures.Add("penguin", Content.Load<Texture2D>("PenguinTexture"));

            Cube.CubeModel = Content.Load<Model>("simplecube");
            //
            Font = Content.Load<SpriteFont>("DefaultFont"); // load default font for 2d text rendering

            BasePlayer = new Player(new Vector3(-5, 0, 0), Models["penguin"], Textures["penguin"]);

            Entities.Add(new Entity(new Vector3(0, 0, 0), Models["penguin"], Textures["penguin"]));
            Entities.Add(new Entity(new Vector3(0, 0, 3), Models["penguin"], Textures["penguin"], "cool", true));

            Crosshair.gDevice = GraphicsDevice;
            CrosshairPlayer = new Crosshair();

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    World.Add(new Cube(new Vector3(x, -1, y))); // create a basic 5x5 grid of cubes
                }
            }

            Interpreter.AddVariable("BasePlayer", BasePlayer); // baseplayer values can be modified at runtime through the console
            Interpreter.AddVariable("CubeColour", new Action<Color>(Cube.ChangeColour)); // 
            Interpreter.AddVariable("Entities", Entities); // entity values can be modified at runtime through the console

            Interpreter.AddVariable("xhair", CrosshairPlayer); // user can modify crosshair variables to preference

            base.Initialize();
        }
        protected override void LoadContent()
        {
            SprBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.OemTilde))
            {
                Console.ToggleOpenClose(); // toggle the console on ~ key press
            }

            // update logic every frame
            BasePlayer.Update(gameTime); // update player entity

            foreach (var Entity in Entities)
                Entity.Update(gameTime); // update every other entity

            IsMouseVisible = Console.IsAcceptingInput; // show cursor when the console is enabled
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // draw every frame
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            FPS.Update(delta);

            foreach (var Entity in Entities)
            {
                Entity.Render(BasePlayer);
                var buffer = Debug.CreateBoundingBoxBuffers(Entity.Box, GraphicsDevice);
                Debug.DrawBoundingBox(buffer, Effect, GraphicsDevice, BasePlayer.View, BasePlayer.Projection);
            }


            foreach (var Cube in World)
                Cube.Render(BasePlayer);

            SprBatch.Begin();
            SprBatch.DrawString(Font, FPS.AverageFramesPerSecond.ToString(), new Vector2(10, 10), Color.Yellow); // fps counter
            SprBatch.DrawString(Font, gameTime.ElapsedGameTime.TotalSeconds.ToString(), new Vector2(10, 30), Color.Yellow); // time between each Draw() call

            SprBatch.Draw(CrosshairPlayer.CrosshairVertical, CrosshairPlayer.CrosshairPosition[0], Color.White);
            SprBatch.Draw(CrosshairPlayer.CrosshairVertical, CrosshairPlayer.CrosshairPosition[1], Color.White);
            SprBatch.Draw(CrosshairPlayer.CrosshairHorizontal, CrosshairPlayer.CrosshairPosition[2], Color.White);
            SprBatch.Draw(CrosshairPlayer.CrosshairHorizontal, CrosshairPlayer.CrosshairPosition[3], Color.White);

            SprBatch.End();
            base.Draw(gameTime);
        }
    }
}
