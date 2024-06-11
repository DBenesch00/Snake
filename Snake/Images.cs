using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Snake
{
    public static class Images
    {
        public readonly static ImageSource Empty = LoadImage("Empty.png");
        public readonly static ImageSource Body = LoadImage("Body.png");
        public readonly static ImageSource Head = LoadImage("Head.png");
        public readonly static ImageSource Food = LoadImage("Food.png");
        public readonly static ImageSource DeadBody = LoadImage("DeadBody.png");
        public readonly static ImageSource DeadHead = LoadImage("DeadHead.png");
        public readonly static ImageSource SuperFood = LoadImage("SuperFood.png");
        public readonly static ImageSource AntiFood = LoadImage("AntiFood.png");
        public readonly static ImageSource Obstacle = LoadImage("Obstacle.png");
        public readonly static ImageSource Red = LoadImage("Red.png");
        public readonly static ImageSource Blue = LoadImage("Blau.png");
        public readonly static ImageSource Gold = LoadImage("Gold.png");

        private static ImageSource LoadImage(string fileName)
        {
            return new BitmapImage(new Uri($"Assets/{fileName}", UriKind.Relative));
        }
    }
}
