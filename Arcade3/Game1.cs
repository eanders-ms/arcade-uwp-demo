using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Arcade3
{
    public class Game1 : Game
    {
        public static Game1 Instance;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _screen;
        private Pxt _pxt;
        private int[] _pixels = new int[Pxt.BUF_SIZE_IN_PIXELS];

        public Game1()
        {
            Instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _pxt = new Pxt();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _pxt.Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _screen = new Texture2D(GraphicsDevice, Pxt.WIDTH, Pxt.HEIGHT, false, SurfaceFormat.Bgra32);

            _pxt.LoadContent();


            // TODO: use this.Content to load your game content here
        }

        protected override void UnloadContent()
        {
            _pxt.UnloadContent();

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            _pxt.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _pxt.GetPixels(_pixels);
            _screen.SetData(_pixels);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);

            _spriteBatch.Draw(_screen, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
