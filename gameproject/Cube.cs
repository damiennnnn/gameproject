using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gameproject
{
    
    public class Cube // cube object that the world is constructed with 
    {
        public static Model CubeModel; // every cube should be sharing the same model, no need to store for each individual cube

        public static Color Colour = Color.Cyan; // colour that the cube uses
        public Vector3 Position; // cubes position in the world
        public BoundingBox Bounding; // bounding box for collision (not implemented yet)

        public static void ChangeColour(Color col)
        {
            Colour = col;
        }

        public Cube(Vector3 Pos) // option of defining your own colour or using a default
        {
            Position = Pos;
        }
        public Cube(Vector3 Pos, Color Col)
        {
            Position = Pos;
            Colour = Col;
        }

        public void Render(Player player)
        {
            foreach (ModelMesh Mesh in CubeModel.Meshes)
            {
                foreach (BasicEffect Effect in Mesh.Effects)
                {
                    Effect.EnableDefaultLighting();
                    Effect.AmbientLightColor = Colour.ToVector3(); // set colour of the cube
                    Effect.View = player.View; // use players current view 
                    Effect.Projection = player.Projection;
                    Effect.World = Matrix.CreateTranslation(Position); // create a world matrix from cubes position
                }
                Mesh.Draw();
            }
        }
    }
}
