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
    public class Entity
    {
        public string Name = string.Empty; // name of entity
        public string ID = string.Empty;   // unique ID for entity

        // physics-related fields
        public Vector3 Position = Vector3.Zero; // position in world as vector
        public Matrix WorldPosition = Matrix.Identity; // position in world as matrix
        public Matrix Rotation = Matrix.Identity; // rotation around position as matrix
        public Vector3 Velocity = Vector3.Zero; // entity movement velocity
        public Vector2 EyeAngles = Vector2.Zero; // entity camera angles in degrees (x - horizontal, y - vertical)
        public Vector3 EyePosition = Vector3.Zero; // entity eye position in world
        public Vector3 Acceleration = Vector3.Zero; // entity movement acceleration
        public BoundingBox ModelBoundingBox; // bounding box for the model
        public BoundingBox Box; // entity bounding box
        public float MovementSpeed = 5f; // movement speed of the entity

        // rendering
        public Model Model; // model to use for rendering
        public Texture2D Texture; // texture to apply
        public Vector3 Tint = Vector3.Zero; // colour tint
        bool Movement = false;
        public Entity(Vector3 pos, Model model, Texture2D texture, string name = "", bool Move = false)
        {
            Position = pos;
            Model = model;
            Texture = texture;
            Name = name;
            WorldPosition = Matrix.CreateWorld(pos, Vector3.Forward, Vector3.Up);
            Movement = Move;
            // generate unique id
            var guid = Guid.NewGuid();
            ID = guid.ToString();
            // testing
            Console.WriteLine(string.Format("new entity name: {0} id: {1}", Name, ID));
            UpdateBoundingBox(); // create bounding box for entity from model
        }

        public void UpdateBoundingBox() // updates the entity bounding box from the model for collisions 
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (ModelMeshPart mesh_part in mesh.MeshParts)
                {
                    int vertex_stride = mesh_part.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertex_buffer_size = mesh_part.NumVertices * vertex_stride;
                    float[] vertex_data = new float[vertex_buffer_size / sizeof(float)];
                    mesh_part.VertexBuffer.GetData<float>(vertex_data);

                    for (int i = 0; i < vertex_buffer_size / sizeof(float); i += vertex_stride / sizeof(float))
                    {
                        Vector3 pos = new Vector3(vertex_data[i], vertex_data[i + 1], vertex_data[i + 2]);
                        min = Vector3.Min(min, pos);
                        max = Vector3.Max(max, pos);
                    } // calculate the minimum and maximum points from a model mesh

                }
            }
            ModelBoundingBox = new BoundingBox(min, max);
        }

        public void DoCollision()
        {
            var min = ModelBoundingBox.Min + Position;
            var max = ModelBoundingBox.Max + Position;
            Box = new BoundingBox(min, max); // modify the existing bounding box instead of recreating it every frame, improved performance


        }

        void HandleMovement(GameTime gameTime)
        {
            Vector3 Cross = Vector3.Cross(Vector3.Up, WorldPosition.Forward);
            Vector3 Vel = Vector3.Zero;

            if (Keyboard.GetState().IsKeyDown(Keys.I))
            {
                Vel += WorldPosition.Forward;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.K))
            {
                Vel -= WorldPosition.Forward;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.J))
            {
                Vel += Cross;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.L))
            {
                Vel -= Cross;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left)) // rotate our entity with the Left and Right arrow keys
                EyeAngles.X += 0.2f;
            else if (Keyboard.GetState().IsKeyDown(Keys.Right))
                EyeAngles.X -= 0.2f;

            if (EyeAngles.X > 180f) EyeAngles.X = (-180f + (EyeAngles.X % 180f)); // eye angles are capped between -180 to 180 horizontal
            if (EyeAngles.X < -180f) EyeAngles.X = (180f - (EyeAngles.X % 180f));

            Vel *= MovementSpeed;
            Vel *= (float)gameTime.ElapsedGameTime.TotalSeconds;

            Position += Vel;
            // copy of player entity movement
        }


        public void Update(GameTime gameTime) {

            if (Movement)
            HandleMovement(gameTime); // copy of player movement code, for testing

           var RotateMatrix = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(EyeAngles.X)); // create a rotation matrix based on the eye angles
           var World = Matrix.CreateTranslation(Position); // create world matrix from current position
           WorldPosition = RotateMatrix * World; // apply x-axis rotation to world model

           DoCollision(); // handle bounding box updates and do collision with the world
        } // update entity every frame

        public void Render(Player player)
        {
            foreach (ModelMesh Mesh in Model.Meshes)
            {
                foreach (BasicEffect Effect in Mesh.Effects)
                {
                    Effect.EnableDefaultLighting();
                    Effect.AmbientLightColor = Tint;
                    Effect.View = player.View;
                    Effect.Projection = player.Projection;
                    Effect.World = WorldPosition;
                    Effect.Texture = Texture;
                    Effect.TextureEnabled = true;
                }
                Mesh.Draw();
            }
        }
    }
}
