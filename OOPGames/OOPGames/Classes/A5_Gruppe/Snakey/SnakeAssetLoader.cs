using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IOPath = System.IO.Path;
using IOFile = System.IO.File;

namespace OOPGames
{
    /// <summary>
    /// Verwaltet das Laden von Bild-Assets
    /// </summary>
    public class SnakeAssetLoader
    {
        private bool _loaded = false;

        public ImageBrush GrassBrush { get; private set; }
        public ImageBrush SnakeHeadBrush { get; private set; }
        public ImageBrush BodyBrush { get; private set; }
        public ImageBrush RattleBrush { get; private set; }
        public ImageBrush FoodBrush { get; private set; }
        
        // Player 2 Assets
        public ImageBrush Snake2HeadBrush { get; private set; }
        public ImageBrush Snake2BodyBrush { get; private set; }
        public ImageBrush Snake2RattleBrush { get; private set; }

        private const string GRASS_IMAGE = "grass_new.png";
        private const string SNAKE_IMAGE = "snake.png";
        private const string TAIL_IMAGE = "tail.png";
        private const string RATTLE_IMAGE = "rattletail.png";
        private const string FOOD_IMAGE = "strawberry.png";
        
        // Player 2 Images
        private const string SNAKE2_HEAD_IMAGE = "snake2_head.png";
        private const string SNAKE2_BODY_IMAGE = "snake2_body.png";
        private const string SNAKE2_RATTLE_IMAGE = "snake2_rattle.png";

        public void LoadAssets()
        {
            if (_loaded) return;
            _loaded = true;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string assetsPath = IOPath.Combine(baseDir, "Assets", "Snake");

            GrassBrush = LoadImageBrush(IOPath.Combine(assetsPath, GRASS_IMAGE));
            SnakeHeadBrush = LoadImageBrush(IOPath.Combine(assetsPath, SNAKE_IMAGE));
            BodyBrush = LoadImageBrush(IOPath.Combine(assetsPath, TAIL_IMAGE));
            RattleBrush = LoadImageBrush(IOPath.Combine(assetsPath, RATTLE_IMAGE));
            FoodBrush = LoadImageBrush(IOPath.Combine(assetsPath, FOOD_IMAGE));
            
            // Player 2 Assets
            Snake2HeadBrush = LoadImageBrush(IOPath.Combine(assetsPath, SNAKE2_HEAD_IMAGE));
            Snake2BodyBrush = LoadImageBrush(IOPath.Combine(assetsPath, SNAKE2_BODY_IMAGE));
            Snake2RattleBrush = LoadImageBrush(IOPath.Combine(assetsPath, SNAKE2_RATTLE_IMAGE));
        }

        private ImageBrush LoadImageBrush(string imagePath)
        {
            if (!IOFile.Exists(imagePath)) return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return new ImageBrush(bitmap) { Stretch = Stretch.Fill };
            }
            catch
            {
                return null;
            }
        }
    }
}
