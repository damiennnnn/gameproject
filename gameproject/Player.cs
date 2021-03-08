using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace gameproject
{
    public class Player : Entity // player entity should be treated as every other entity
    {
        public Matrix View; // view matrix to use for 3d camera
        public Matrix Projection; // projection matrix to use for 3d camera;


        MouseState PrevState; // previous mouse state for calculating mouse input

        public float FOV = 65f;
        public float Sensitivity = 15;
        float MouseSens { get => MathHelper.ToRadians(FOV) / Sensitivity;} // 
        public Player(Vector3 pos, Model model, Texture2D texture) : base(pos,model,texture){ // use base constructor as the player is an entity 
            View = Matrix.CreateLookAt(Position, new Vector3(0, 0, 0), Vector3.Up); // create view matrix, looking at centre
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), Main.Resolution.Width / (float)Main.Resolution.Height, 0.1f, 100f);
            // field of view (65 degrees), current aspect ratio (window width divided by window height)
        }

        public void UpdateFOV() { Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), Main.Resolution.Width / (float)Main.Resolution.Height, 0.1f, 100f); } // update the projection matrix if the fov changes

        void HandleMovement(GameTime gameTime)
        {
            Vector3 Cross = Vector3.Cross(Vector3.Up, WorldPosition.Forward); // calculate cross product, the direction that will be perpendicular to two other lines
            Vector3 Vel = Vector3.Zero;

                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    Vel += WorldPosition.Forward; // add the forward position to the player to move forward 
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    Vel -= WorldPosition.Forward; // subtract the forward position to go backwards
                }

                if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    Vel += Cross; // add the cross product to the player position, pointing left, to move left 
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    Vel -= Cross; // subtract the cross product to move right
                }

            Vel *= MovementSpeed; // multiply our movement vector by the defined movement speed
            Vel *= (float)gameTime.ElapsedGameTime.TotalSeconds; // multiply the movement vector by the time between updates, to compensate for any speed up or slowdown in performance and for consistent physics

            Position += Vel;
        }
        public bool LeftMouseClick = false;
        void HandleMouseInput()
        {
            EyeAngles.X -= MouseSens * (Mouse.GetState().X - PrevState.X); // mouse moves by the difference between mouse positions 
            if (EyeAngles.X > 180f) EyeAngles.X = (-180f + (EyeAngles.X % 180f)); // eye angles are capped between -180 to 180 horizontal
            if (EyeAngles.X < -180f) EyeAngles.X = (180f - (EyeAngles.X % 180f));
            EyeAngles.Y -= MouseSens * (Mouse.GetState().Y - PrevState.Y);
            if (EyeAngles.Y >= 90f) EyeAngles.Y = 89f; // eye angles are capped between -89 and 89 vertical
            if (EyeAngles.Y <= -90f) EyeAngles.Y = -89f;

            Mouse.SetPosition((int)Main.WindowCentre.X, (int)Main.WindowCentre.Y);
            PrevState = Mouse.GetState();

            if (LeftMouseClick)
            {
                Debug.Output("LMB down"); // do an action when the left mouse button is pressed
                if (Mouse.GetState().LeftButton == ButtonState.Released)
                {
                    Debug.Output("LMB up");
                    LeftMouseClick = false; // do an action when the left mouse button is released
                }
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                LeftMouseClick = true;

        }

        public new void Update(GameTime gameTime) // override update for player entity
        {
            if (!Main.Console.IsAcceptingInput) // only handle input if the console is closed
            {
                HandleMouseInput(); // eye angle/mouse movement routine
                HandleMovement(gameTime); // WASD movement for the player
            }

            DoCollision(); // share collision method with entities

            var RotateMatrix = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(EyeAngles.X)); // create a rotation matrix based on the eye angles
            var World = Matrix.CreateTranslation(Position); // create world matrix from current position

            WorldPosition = RotateMatrix * World; // apply x-axis rotation to world model
            
            EyePosition = WorldPosition.Forward; // set eye position to forward direction of model
            EyePosition = Vector3.Transform(EyePosition, Matrix.CreateFromAxisAngle(Vector3.Cross(Vector3.Up, WorldPosition.Forward), MathHelper.ToRadians(-EyeAngles.Y)));
            // apply y-axis rotation to player camera, not to the world model

            View = Matrix.CreateLookAt(Position + Vector3.Up, Position + Vector3.Up + EyePosition, Vector3.Up); // set view matrix to the direction where the player is pointing
        }
    }
}
