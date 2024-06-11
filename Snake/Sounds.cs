using System;
using System.IO;
using System.Media;
using System.Reflection;

namespace Snake
{
    public static class Sounds
    {
        public readonly static SoundPlayer EatSound = LoadSound("Snake.Assets.Eat.wav");
        public readonly static SoundPlayer GameOverSound = LoadSound("Snake.Assets.Die.wav");

        private static SoundPlayer LoadSound(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("Resource not found: " + resourceName);
                }

                var soundPlayer = new SoundPlayer(stream);
                soundPlayer.Load();
                return soundPlayer;
            }
        }
    }
}
