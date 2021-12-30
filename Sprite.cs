using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace SpriteMasterNew {

    public class Sprite {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }

        public int Width { get; set; }

        public int[] Image { get; set; }
        public char Character { get; set; }


        public Sprite() { }


        public Sprite(string name, int x, int y, int height, int width, int[] image) {
            name = Name;
            x = X;
            y = Y;
            height = Height;
            width = Width;

        }
        public Sprite(string name, int x, int y, int height, int width) {
            name = Name;
            x = X;
            y = Y;
            height = Height;
            width = Width;

        }

        public int Center() {
            var center = Height / 2;

            return center;
        }



    }
}
