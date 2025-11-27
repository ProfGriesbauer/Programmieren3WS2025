using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace OOPGames
{
	// Painter draws only: a fixed sine-ground, no game logic.
	public class A4_ShellStrikeLegendsV2_Painter : IPaintGame, IPaintGame2
	{
		public string Name => "A4 ShellStrikeLegends V2 Painter";

		private Path _cachedTerrainPath;
		private int _cachedW, _cachedH;
		private int _cachedHash;

		public void PaintGameField(Canvas canvas, IGameField currentField)
		{
			if (canvas == null) return;
			canvas.Children.Clear();

			// Background (dark sky like reference)
			canvas.Background = new SolidColorBrush(Color.FromRgb(16, 18, 26));

			double w = canvas.ActualWidth; if (w <= 0) w = 800;
			double h = canvas.ActualHeight; if (h <= 0) h = 400;
			int wi = (int)Math.Round(w);
			int hi = (int)Math.Round(h);

			if (currentField is not A4_ShellStrikeLegendsV2_GameField field)
			{
				// If no field is supplied, draw a default sine terrain anyway (render-only)
				var temp = new A4_ShellStrikeLegendsV2_Terrain();
				temp.BuildFixedSine(wi, hi);
				DrawTerrain(canvas, temp);
				return;
			}

			field.EnsureSetup(wi, hi);
			DrawTerrain(canvas, field.Terrain);
			if (field.Tank1 != null)
			{
				DrawHull(canvas, field.Terrain, field.Tank1);
			}
		}

		public void TickPaintGameField(Canvas canvas, IGameField currentField) => PaintGameField(canvas, currentField);

		private void DrawTerrain(Canvas canvas, A4_ShellStrikeLegendsV2_Terrain terrain)
		{
			if (terrain.Heights == null || terrain.Heights.Length == 0) return;

			int wi = terrain.CanvasWidth;
			int hi = terrain.CanvasHeight;
			int hash = QuickHash(terrain.Heights);
			if (_cachedTerrainPath == null || _cachedW != wi || _cachedH != hi || _cachedHash != hash)
			{
				_cachedW = wi; _cachedH = hi; _cachedHash = hash;
				var geom = new StreamGeometry();
				using (var ctx = geom.Open())
				{
					ctx.BeginFigure(new Point(0, hi), isFilled: true, isClosed: true);
					ctx.LineTo(new Point(0, terrain.Heights[0]), true, false);
					for (int x = 1; x < terrain.Heights.Length; x++)
					{
						ctx.LineTo(new Point(x, terrain.Heights[x]), true, false);
					}
					ctx.LineTo(new Point(wi - 1, hi), true, false);
				}
				geom.Freeze();

				// Bright cyan-ish ground fill to resemble the screenshot
				_cachedTerrainPath = new Path
				{
					Data = geom,
					Fill = new SolidColorBrush(Color.FromRgb(0x5A, 0xD5, 0xFF)),
					Stroke = null,
					StrokeThickness = 0
				};
			}
			canvas.Children.Add(_cachedTerrainPath);
		}

		private static int QuickHash(int[] arr)
		{
			unchecked
			{
				int h = 17;
				int step = Math.Max(1, arr.Length / 97);
				for (int i = 0; i < arr.Length; i += step) h = h * 31 + arr[i];
				return h;
			}
		}

		private readonly System.Collections.Generic.Dictionary<string, ImageSource> _cache = new();
		private ImageSource LoadImage(string path)
		{
			if (_cache.TryGetValue(path, out var src)) return src;
			// 1) Try absolute disk path under output directory
			try
			{
				string baseDir = AppContext.BaseDirectory;
				string full = System.IO.Path.Combine(baseDir, path.Replace('/', System.IO.Path.DirectorySeparatorChar));
				if (System.IO.File.Exists(full))
				{
					var bi = new BitmapImage();
					bi.BeginInit();
					bi.UriSource = new Uri(full, UriKind.Absolute);
					bi.CacheOption = BitmapCacheOption.OnLoad;
					bi.EndInit();
					bi.Freeze();
					_cache[path] = bi;
					return bi;
				}
			}
			catch { }

			// 2) Try pack URI (requires Resource build action)
			try
			{
				var bi = new BitmapImage();
				bi.BeginInit();
				bi.UriSource = new Uri($"pack://application:,,,/{path}", UriKind.Absolute);
				bi.CacheOption = BitmapCacheOption.OnLoad;
				bi.EndInit();
				bi.Freeze();
				_cache[path] = bi;
				return bi;
			}
			catch { return null; }
		}

		private void DrawHull(Canvas canvas, A4_ShellStrikeLegendsV2_Terrain terrain, A4_ShellStrikeLegendsV2_Tank tank)
		{
			if (string.IsNullOrWhiteSpace(tank.HullSpritePath)) return;
			var src = LoadImage(tank.HullSpritePath);
			if (src == null)
			{
				// Fallback: draw a simple rectangle to visualize the tank position
				double fw = A4_ShellStrikeLegendsV2_Config.TankBodyWidthPx;
				double fh = A4_ShellStrikeLegendsV2_Config.TankBodyHeightPx;
				double fx = tank.X;
				double fyTop = tank.Y;
				var rect = new Rectangle { Width = fw, Height = fh, Fill = Brushes.DarkGreen };
				Canvas.SetLeft(rect, fx - fw / 2.0);
				Canvas.SetTop(rect, fyTop);
				canvas.Children.Add(rect);
				Canvas.SetZIndex(rect, 10);
				return;
			}

			// Always render at configured size
			double w = A4_ShellStrikeLegendsV2_Config.TankBodyWidthPx;
			double h = A4_ShellStrikeLegendsV2_Config.TankBodyHeightPx;

			double x = tank.X;
			double yTop = tank.Y; // top-left y comes from rules (falling)

			var img = new Image { Source = src, Width = w, Height = h };
			Canvas.SetLeft(img, x - w / 2.0);
			Canvas.SetTop(img, yTop);
			canvas.Children.Add(img);
			Canvas.SetZIndex(img, 10);
		}
	}
}

