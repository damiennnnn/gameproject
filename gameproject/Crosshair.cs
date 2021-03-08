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
    public class Crosshair
    {
        public static GraphicsDevice gDevice;
        public Texture2D CrosshairVertical;
        public Texture2D CrosshairHorizontal;
        public Vector2[] CrosshairPosition = new Vector2[4];

        int len = 15;
        int thickness = 3;
        int gap = 5;
        Color col = Color.LightGreen;


        public int Length { get => len; set { len = value; Create(); } }
        public int Thickness { get => thickness; set { thickness = value; Create(); } }
        public int CentreGap { get => gap; set { gap = value; Create(); } }
        public Color Colour { get => col; set { col = value; Create(); } } // crosshair will automatically update when the values are changed

        public void Create()
        {
            CrosshairVertical = new Texture2D(gDevice, Thickness, Length);
            CrosshairHorizontal = new Texture2D(gDevice, Length, Thickness);

            Color[] data = new Color[Length * Thickness];
            for (int i = 0; i < data.Length; ++i) data[i] = Colour;
            CrosshairVertical.SetData(data);
            CrosshairHorizontal.SetData(data);
            CrosshairPosition[0] = new Vector2(Main.WindowCentre.X - (Thickness / 2), Main.WindowCentre.Y + CentreGap);
            CrosshairPosition[1] = new Vector2(Main.WindowCentre.X - (Thickness / 2), (Main.WindowCentre.Y - Length) - CentreGap);
            CrosshairPosition[2] = new Vector2(Main.WindowCentre.X + CentreGap, Main.WindowCentre.Y - (Thickness / 2));
            CrosshairPosition[3] = new Vector2((Main.WindowCentre.X - Length) - CentreGap, Main.WindowCentre.Y - (Thickness / 2));
        }

        public Crosshair()
        {
            Create();
        }

    }
}
