using System;
using System.Windows.Controls;
using System.Windows.Media;
using OOPGames;

namespace OOPGames
{
	// Referenz: Move-Logik wie in TickTackToe_Anton_Felix.cs, Steuerung über IPlayMove
	public class B5_Pong_Painter : IPaintGame2
	{
		public bool IsGameOver { get; private set; } = false;
		public int Winner { get; private set; } = 0; // 1 = links gewinnt, 2 = rechts gewinnt
		public string Name => "B5_Pong_Painter";

		// Balken und Ball-Parameter - STATIC so all instances share the same values!
		private static double leftBarY = 90;
		private static double rightBarY = 90;
		public double barHeight = 60, barWidth = 10;
		private static double ballX = 100;
		private static double ballY = 120;
		private double ballRadius = 10;
		private static double ballVX = 8;
		private static double ballVY = 6;
		private static double canvasW = 200;
		private static double canvasH = 240;
		
		// Spielstand - Punkte für linke und rechte Seite
		private static int leftScore = 0;
		private static int rightScore = 0;
		
		// Flag für Startbildschirm und Pause
		private static bool gameStarted = false;
		private static bool gamePaused = false;
		
        // PaintGameField: Initiales Zeichnen
		public void PaintGameField(Canvas canvas, IGameField currentField)
		{
			canvas.Children.Clear();
			
			// Zeige Startbildschirm wenn Spiel noch nicht gestartet wurde
			if (!gameStarted)
			{
				DrawStartScreen(canvas);
			}
			else
			{
				DrawBarsAndBall(canvas);
				
				// Nach dem ersten Zeichnen: Bars erneut in gültige Grenzen setzen
				double maxY = canvasH - barHeight;
				leftBarY = Math.Max(0, Math.Min(maxY, leftBarY));
				rightBarY = Math.Max(0, Math.Min(maxY, rightBarY));
			}
		}

		// TickPaintGameField: Animation/Ballbewegung
		public void TickPaintGameField(Canvas canvas, IGameField currentField)
		{
			canvas.Children.Clear();
			
			if (!gameStarted)
			{
				DrawStartScreen(canvas);
			}
			else if (gamePaused)
			{
				DrawBarsAndBall(canvas);
				DrawPauseScreen(canvas);
			}
			else
			{
				MoveBall();
				DrawBarsAndBall(canvas);
			}
		}

	private void DrawStartScreen(Canvas canvas)
	{
		// Update canvas dimensions
		double canvasWidth = canvas.ActualWidth > 0 ? canvas.ActualWidth : 800;
		double canvasHeight = canvas.ActualHeight > 0 ? canvas.ActualHeight : 600;
		
		// Hintergrund mit freundlichem Gradient
		var background = new System.Windows.Shapes.Rectangle
		{
			Width = canvasWidth,
			Height = canvasHeight,
			Fill = new LinearGradientBrush(
				Color.FromRgb(70, 130, 180),
				Color.FromRgb(135, 206, 235),
				90)
		};
		canvas.Children.Add(background);
		
		// Titel
		var titleText = new System.Windows.Controls.TextBlock
		{
			Text = "PONG",
			FontSize = canvasHeight * 0.12,
			FontWeight = System.Windows.FontWeights.Bold,
			Foreground = Brushes.White,
			TextAlignment = System.Windows.TextAlignment.Center,
			Width = canvasWidth
		};
		Canvas.SetLeft(titleText, 0);
		Canvas.SetTop(titleText, canvasHeight * 0.05);
		canvas.Children.Add(titleText);
		
		// Untertitel
		var subtitleText = new System.Windows.Controls.TextBlock
		{
			Text = "Herzlich Willkommen!",
			FontSize = canvasHeight * 0.04,
			FontWeight = System.Windows.FontWeights.Normal,
			Foreground = Brushes.White,
			TextAlignment = System.Windows.TextAlignment.Center,
			Width = canvasWidth
		};
		Canvas.SetLeft(subtitleText, 0);
		Canvas.SetTop(subtitleText, canvasHeight * 0.2);
		canvas.Children.Add(subtitleText);
		
		// Box für Steuerung
		var controlBox = new System.Windows.Shapes.Rectangle
		{
			Width = canvasWidth * 0.75,
			Height = canvasHeight * 0.42,
			Fill = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
			Stroke = Brushes.White,
			StrokeThickness = 3,
			RadiusX = 10,
			RadiusY = 10
		};
		Canvas.SetLeft(controlBox, canvasWidth * 0.125);
		Canvas.SetTop(controlBox, canvasHeight * 0.31);
		canvas.Children.Add(controlBox);
		
		// Steuerung Überschrift
		var controlTitle = new System.Windows.Controls.TextBlock
		{
			Text = "STEUERUNG",
			FontSize = canvasHeight * 0.045,
			FontWeight = System.Windows.FontWeights.Bold,
			Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
			TextAlignment = System.Windows.TextAlignment.Center,
			Width = canvasWidth
		};
		Canvas.SetLeft(controlTitle, 0);
		Canvas.SetTop(controlTitle, canvasHeight * 0.34);
		canvas.Children.Add(controlTitle);
		
		// Linker Spieler
		var leftPlayerText = new System.Windows.Controls.TextBlock
		{
			Text = "Linker Spieler (BLAU)",
			FontSize = canvasHeight * 0.035,
			FontWeight = System.Windows.FontWeights.Bold,
			Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 255)),
			TextAlignment = System.Windows.TextAlignment.Left
		};
		Canvas.SetLeft(leftPlayerText, canvasWidth * 0.18);
		Canvas.SetTop(leftPlayerText, canvasHeight * 0.44);
		canvas.Children.Add(leftPlayerText);
		
		var leftControls = new System.Windows.Controls.TextBlock
		{
			Text = "W  =  Hoch\nS  =  Runter",
			FontSize = canvasHeight * 0.028,
			FontWeight = System.Windows.FontWeights.SemiBold,
			Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
			TextAlignment = System.Windows.TextAlignment.Left,
			LineHeight = canvasHeight * 0.038
		};
		Canvas.SetLeft(leftControls, canvasWidth * 0.2);
		Canvas.SetTop(leftControls, canvasHeight * 0.5);
		canvas.Children.Add(leftControls);
		
		// Rechter Spieler
		var rightPlayerText = new System.Windows.Controls.TextBlock
		{
			Text = "Rechter Spieler (ROT)",
			FontSize = canvasHeight * 0.035,
			FontWeight = System.Windows.FontWeights.Bold,
			Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
			TextAlignment = System.Windows.TextAlignment.Left
		};
		Canvas.SetLeft(rightPlayerText, canvasWidth * 0.53);
		Canvas.SetTop(rightPlayerText, canvasHeight * 0.44);
		canvas.Children.Add(rightPlayerText);
		
		var rightControls = new System.Windows.Controls.TextBlock
		{
			Text = "I  =  Hoch\nK  =  Runter",
			FontSize = canvasHeight * 0.028,
			FontWeight = System.Windows.FontWeights.SemiBold,
			Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
			TextAlignment = System.Windows.TextAlignment.Left,
			LineHeight = canvasHeight * 0.038
		};
		Canvas.SetLeft(rightControls, canvasWidth * 0.55);
		Canvas.SetTop(rightControls, canvasHeight * 0.5);
		canvas.Children.Add(rightControls);
		
		// Start Anweisung mit Animation-Effekt
		var startText = new System.Windows.Controls.TextBlock
		{
			Text = "►  Drücke die LEERTASTE zum Starten!  ◄",
			FontSize = canvasHeight * 0.035,
			FontWeight = System.Windows.FontWeights.Bold,
			FontStyle = System.Windows.FontStyles.Italic,
			Foreground = Brushes.Yellow,
			TextAlignment = System.Windows.TextAlignment.Center,
			Width = canvasWidth
		};
		Canvas.SetLeft(startText, 0);
		Canvas.SetTop(startText, canvasHeight * 0.82);
		canvas.Children.Add(startText);
		
		// Dekorative Linie unten
		var bottomLine = new System.Windows.Shapes.Line
		{
			X1 = canvasWidth * 0.1,
			X2 = canvasWidth * 0.9,
			Y1 = canvasHeight * 0.92,
			Y2 = canvasHeight * 0.92,
			Stroke = Brushes.White,
			StrokeThickness = 3
		};
		canvas.Children.Add(bottomLine);
	}

	private void DrawPauseScreen(Canvas canvas)
	{
		double canvasWidth = canvas.ActualWidth > 0 ? canvas.ActualWidth : 800;
		double canvasHeight = canvas.ActualHeight > 0 ? canvas.ActualHeight : 600;
		
		// Pause Symbol - zwei vertikale parallele Striche
		double centerX = canvasWidth / 2;
		double centerY = canvasHeight / 2;
		double barWidth = canvasWidth * 0.04;
		double barHeight = canvasHeight * 0.2;
		double spacing = canvasWidth * 0.03;
		
		// Linker Strich
		var leftBar = new System.Windows.Shapes.Rectangle
		{
			Width = barWidth,
			Height = barHeight,
			Fill = Brushes.White,
			Stroke = Brushes.Black,
			StrokeThickness = 3
		};
		Canvas.SetLeft(leftBar, centerX - spacing - barWidth);
		Canvas.SetTop(leftBar, centerY - barHeight / 2);
		canvas.Children.Add(leftBar);
		
		// Rechter Strich
		var rightBar = new System.Windows.Shapes.Rectangle
		{
			Width = barWidth,
			Height = barHeight,
			Fill = Brushes.White,
			Stroke = Brushes.Black,
			StrokeThickness = 3
		};
		Canvas.SetLeft(rightBar, centerX + spacing);
		Canvas.SetTop(rightBar, centerY - barHeight / 2);
		canvas.Children.Add(rightBar);
		
		// Text mit weißem Hintergrund
		double textBoxWidth = canvasWidth * 0.5;
		double textBoxHeight = canvasHeight * 0.08;
		var textBackground = new System.Windows.Shapes.Rectangle
		{
			Width = textBoxWidth,
			Height = textBoxHeight,
			Fill = Brushes.White,
			RadiusX = 10,
			RadiusY = 10
		};
		Canvas.SetLeft(textBackground, (canvasWidth - textBoxWidth) / 2);
		Canvas.SetTop(textBackground, centerY + barHeight / 2 + 20);
		canvas.Children.Add(textBackground);
		
		// Schwarzer Text
		var pauseText = new System.Windows.Controls.TextBlock
		{
			Text = "Drücke P zum Weitermachen",
			FontSize = canvasHeight * 0.04,
			FontWeight = System.Windows.FontWeights.Bold,
			Foreground = Brushes.Black,
			TextAlignment = System.Windows.TextAlignment.Center,
			Width = canvasWidth
		};
		Canvas.SetLeft(pauseText, 0);
		Canvas.SetTop(pauseText, centerY + barHeight / 2 + 30);
		canvas.Children.Add(pauseText);
	}

	private void DrawBarsAndBall(Canvas canvas)
	{
		// Update canvas dimensions first
		double newCanvasW = canvas.ActualWidth > 0 ? canvas.ActualWidth / 2 : 200;
		double newCanvasH = canvas.ActualHeight > 0 ? canvas.ActualHeight : 240;
		
		// Only update if values changed significantly to avoid flickering
		if (Math.Abs(canvasW - newCanvasW) > 1) canvasW = newCanvasW;
		if (Math.Abs(canvasH - newCanvasH) > 1) canvasH = newCanvasH;

		System.Diagnostics.Debug.WriteLine($"DrawBarsAndBall: canvasW={canvasW}, canvasH={canvasH}, leftBarY={leftBarY}, rightBarY={rightBarY}");

		// Linker Balken
		var leftBar = new System.Windows.Shapes.Rectangle
		{
			Width = barWidth,
			Height = barHeight,
			Fill = Brushes.Blue
		};
		Canvas.SetLeft(leftBar, 10);
		Canvas.SetTop(leftBar, leftBarY);
		canvas.Children.Add(leftBar);

		// Rechter Balken - verschoben um eine Spielfeldbreite (canvasW) nach rechts
		var rightBar = new System.Windows.Shapes.Rectangle
		{
			Width = barWidth,
			Height = barHeight,
			Fill = Brushes.Red
		};
		// Positioniere den rechten Balken direkt am rechten Rand + canvasW offset
		double rightBarX = canvasW - barWidth + canvasW;
		System.Diagnostics.Debug.WriteLine($"Drawing RIGHT bar at X={rightBarX}, Y={rightBarY}, canvasH={canvasH}");
		Canvas.SetLeft(rightBar, rightBarX);
		Canvas.SetTop(rightBar, rightBarY);
		canvas.Children.Add(rightBar);			// Ball
			var ball = new System.Windows.Shapes.Ellipse
			{
				Width = ballRadius * 2,
				Height = ballRadius * 2,
				Fill = Brushes.Black
			};
			Canvas.SetLeft(ball, ballX - ballRadius);
			Canvas.SetTop(ball, ballY - ballRadius);
			canvas.Children.Add(ball);
			
			// Spielstand anzeigen
			var scoreText = new System.Windows.Controls.TextBlock
			{
				Text = $"{leftScore}  :  {rightScore}",
				FontSize = 36,
				FontWeight = System.Windows.FontWeights.Bold,
				Foreground = Brushes.Black
			};
			Canvas.SetLeft(scoreText, canvasW - 50);
			Canvas.SetTop(scoreText, 20);
			canvas.Children.Add(scoreText);
		}

		private void MoveBall()
		{
			ballX += ballVX;
			ballY += ballVY;

            canvasW = canvasW > 0 ? canvasW : 200;
            canvasH = canvasH > 0 ? canvasH : 240;

			// Abprallen unten/oben - allow ball to wrap or bounce at screen edges
			if (ballY + ballRadius >= canvasH)
			{
				ballY = canvasH - ballRadius;
				ballVY = -Math.Abs(ballVY);
			}
			if (ballY - ballRadius <= 0)
			{
				ballY = ballRadius;
				ballVY = Math.Abs(ballVY);
			}

			// Abprallen an Balken links
			if (ballX - ballRadius <= 10 + barWidth && ballY >= leftBarY && ballY <= leftBarY + barHeight)
			{
				ballX = 10 + barWidth + ballRadius;
				// Increase speed and reverse X direction (must be positive/right)
				double speed = Math.Sqrt(ballVX * ballVX + ballVY * ballVY) + 0.5;
				double angle = Math.Atan2(ballVY, Math.Abs(ballVX));
				ballVX = Math.Abs(speed * Math.Cos(angle)); // Force positive (right direction)
				ballVY = speed * Math.Sin(angle);
			}
			// Abprallen an Balken rechts - check at actual visual position (2*canvasW - barWidth)
			double rightBarXPos = canvasW * 2 - barWidth;
			if (ballX + ballRadius >= rightBarXPos && ballY >= rightBarY && ballY <= rightBarY + barHeight)
			{
				ballX = rightBarXPos - ballRadius;
				// Increase speed and reverse X direction (must be negative/left)
				double speed = Math.Sqrt(ballVX * ballVX + ballVY * ballVY) + 0.5;
				double angle = Math.Atan2(ballVY, Math.Abs(ballVX));
				ballVX = -Math.Abs(speed * Math.Cos(angle)); // Force negative (left direction)
				ballVY = speed * Math.Sin(angle);
			}

			// Ball verlässt links (unter x-Position des blauen Balkens): Punkt für rechte Seite
			if (ballX < 10)
			{
				rightScore++;
				System.Diagnostics.Debug.WriteLine($"RIGHT scores! Score: {leftScore} - {rightScore}");
				Reset();
			}
			// Ball verlässt rechts (über x-Position des roten Balkens): Punkt für linke Seite
			if (ballX > rightBarXPos + barWidth)
			{
				leftScore++;
				System.Diagnostics.Debug.WriteLine($"LEFT scores! Score: {leftScore} - {rightScore}");
				Reset();
			}
		}

		// Methoden zum Steuern der Balken
		public void SetLeftBarY(double y)
		{
			// Don't move bars when paused
			if (gamePaused) return;
			
			// Left (blue) bar stays within normal canvas bounds (not allowed to go beyond bottom)
			double maxY = canvasH > 0 ? canvasH - barHeight : 180;
			System.Diagnostics.Debug.WriteLine($"SetLeftBarY BEFORE: leftBarY={leftBarY}, y={y}, maxY={maxY}, canvasH={canvasH}");
			leftBarY = Math.Max(0, Math.Min(maxY, y));
			System.Diagnostics.Debug.WriteLine($"SetLeftBarY AFTER: leftBarY={leftBarY}");
		}
	public void SetRightBarY(double y)
	{
		// Don't move bars when paused
		if (gamePaused) return;
		
		// Right (red) bar stays within normal canvas bounds
		double maxY = canvasH > 0 ? canvasH - barHeight : 180;
		System.Diagnostics.Debug.WriteLine($"SetRightBarY BEFORE: rightBarY={rightBarY}, y={y}, maxY={maxY}, canvasH={canvasH}");
		rightBarY = Math.Max(0, Math.Min(maxY, y));
		System.Diagnostics.Debug.WriteLine($"SetRightBarY AFTER: rightBarY={rightBarY}");
	}		// Getter methods for bar positions
		public double GetLeftBarY() => leftBarY;
		public double GetRightBarY() => rightBarY;
		
		// Static methods for game start
		public static bool IsGameStarted() => gameStarted;
		public static void StartGame() => gameStarted = true;
		
		// Static methods for pause
		public static bool IsGamePaused() => gamePaused;
		public static void TogglePause() => gamePaused = !gamePaused;

		// Reset/clear the game state
		public void Reset()
		{
		canvasW = 200; canvasH = 240;
		leftBarY = 90;  // Start at reasonable position
		rightBarY = 90;
		ballX = 100; ballY = 120;
		ballVX = 8; ballVY = 6;
		IsGameOver = false; Winner = 0;
		}
		// Balkensteuerung per Move
		public void ApplyMove(IPlayMove move)
		{
			// Spieler 1: W/S, Spieler 2: Pfeiltasten
			if (move is KeyMove km)
			{
				System.Diagnostics.Debug.WriteLine($"ApplyMove: Player {km.PlayerNumber}, Key {km.Key}");
				if (km.PlayerNumber == 1)
				{
					if (km.Key == System.Windows.Input.Key.W)
					{
						System.Diagnostics.Debug.WriteLine($"Moving LEFT bar UP from {leftBarY}");
						SetLeftBarY(leftBarY - 10);
						System.Diagnostics.Debug.WriteLine($"LEFT bar now at {leftBarY}");
					}
					else if (km.Key == System.Windows.Input.Key.S)
					{
						System.Diagnostics.Debug.WriteLine($"Moving LEFT bar DOWN from {leftBarY}");
						SetLeftBarY(leftBarY + 10);
						System.Diagnostics.Debug.WriteLine($"LEFT bar now at {leftBarY}");
					}
				}
				else if (km.PlayerNumber == 2)
				{
					if (km.Key == System.Windows.Input.Key.OemPlus)  // Ü key
					{
						System.Diagnostics.Debug.WriteLine($"Moving RIGHT bar UP from {rightBarY}");
						SetRightBarY(rightBarY - 10);
						System.Diagnostics.Debug.WriteLine($"RIGHT bar now at {rightBarY}");
					}
				}
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"ApplyMove: move is NOT KeyMove, type is {move?.GetType().Name}");
			}
		}

            // Nested KeyMove class so callers can use B5_Pong_Painter.KeyMove(...)
            public class KeyMove : IPlayMove
            {
                public int PlayerNumber { get; }
                public System.Windows.Input.Key Key { get; }
                public KeyMove(int player, System.Windows.Input.Key key)
                {
                    PlayerNumber = player;
                    Key = key;
                }
            }
        }

		// Human player for Pong: converts selections into moves and directly moves bars with the painter
	public class B5_Pong_HumanPlayer : IHumanGamePlayer, IHumanGamePlayerWithMouse
	{
        private int _playerNumber = 1;
        private readonly B5_Pong_Painter _painter;
        
        // Static key processing - stores last pressed key so both players can react
        private static System.Windows.Input.Key? _lastKey = null;
        private static bool _keyProcessed = false;

        public B5_Pong_HumanPlayer(B5_Pong_Painter painter = null)
        {
            _painter = painter;
            Name = "B5 Pong Human";
        }

        public string Name { get; private set; }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public int PlayerNumber => _playerNumber;

        public bool CanBeRuledBy(IGameRules rules)
        {
            // This human player can work with the Pong rules implemented here
            return rules is B5_Pong_Rules;
        }

        public IGamePlayer Clone()
        {
            return new B5_Pong_HumanPlayer(_painter) { Name = this.Name };
        }

        // Convert selection into a play move. For key selections we return a KeyMove
        // For clicks we move the corresponding bar directly and return null.
        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            // Keyboard input: Check ALL keys for BOTH players and move bars DIRECTLY
            if (selection is IKeySelection ks)
            {
                // Get the shared painter from rules
                var painter = B5_Pong_Rules.SharedPainter;
                
                // Start game only on space key press if not started yet
                if (painter != null && !B5_Pong_Painter.IsGameStarted())
                {
                    if (ks.Key == System.Windows.Input.Key.Space)
                    {
                        B5_Pong_Painter.StartGame();
                    }
                    return new B5_Pong_Painter.KeyMove(_playerNumber, ks.Key);
                }
                
                // Toggle pause with P key
                if (ks.Key == System.Windows.Input.Key.P)
                {
                    B5_Pong_Painter.TogglePause();
                    return new B5_Pong_Painter.KeyMove(_playerNumber, ks.Key);
                }
                
                // Don't process game controls if paused
                if (B5_Pong_Painter.IsGamePaused())
                {
                    return new B5_Pong_Painter.KeyMove(_playerNumber, ks.Key);
                }
                
                // Always reset the flag for each new key press
                _lastKey = ks.Key;
                _keyProcessed = false;
                
                // Debug: Show which key was pressed
                System.Diagnostics.Debug.WriteLine($"Key pressed: {ks.Key} (by player {_playerNumber}), keyProcessed: {_keyProcessed}");
                
                // Process ALL keys regardless of player number
                // Player 1 keys (W/S) - move left bar directly
                if (ks.Key == System.Windows.Input.Key.W)
                {
                    System.Diagnostics.Debug.WriteLine("W pressed - moving left bar UP");
                    if (painter != null && !_keyProcessed)
                    {
                        painter.SetLeftBarY(painter.GetLeftBarY() - 10);
                        _keyProcessed = true;
                    }
                    return new B5_Pong_Painter.KeyMove(1, ks.Key);
                }
                if (ks.Key == System.Windows.Input.Key.S)
                {
                    System.Diagnostics.Debug.WriteLine("S pressed - moving left bar DOWN");
                    if (painter != null && !_keyProcessed)
                    {
                        painter.SetLeftBarY(painter.GetLeftBarY() + 10);
                        _keyProcessed = true;
                    }
                    return new B5_Pong_Painter.KeyMove(1, ks.Key);
                }
                
                // Player 2 keys - I and K
                // I key moves right bar up
                if (ks.Key == System.Windows.Input.Key.I)
                {
                    System.Diagnostics.Debug.WriteLine($"I pressed - moving right bar UP, rightBarY before: {painter?.GetRightBarY()}");
                    if (painter != null && !_keyProcessed)
                    {
                        double before = painter.GetRightBarY();
                        painter.SetRightBarY(before - 10);
                        _keyProcessed = true;
                        System.Diagnostics.Debug.WriteLine($"Right bar moved from {before} to {painter.GetRightBarY()}");
                    }
                    return new B5_Pong_Painter.KeyMove(2, ks.Key);
                }
                // K key moves right bar down
                if (ks.Key == System.Windows.Input.Key.K)
                {
                    System.Diagnostics.Debug.WriteLine($"K pressed - moving right bar DOWN, rightBarY before: {painter?.GetRightBarY()}");
                    if (painter != null && !_keyProcessed)
                    {
                        double before = painter.GetRightBarY();
                        painter.SetRightBarY(before + 10);
                        _keyProcessed = true;
                        System.Diagnostics.Debug.WriteLine($"Right bar moved from {before} to {painter.GetRightBarY()}");
                    }
                    return new B5_Pong_Painter.KeyMove(2, ks.Key);
                }
                
                // Mark key as processed even for unknown keys
                _keyProcessed = true;
                
                // For any other key, use the player's number
                return new B5_Pong_Painter.KeyMove(_playerNumber, ks.Key);
            }

            // If no painter is attached, we cannot handle mouse interactions
            if (_painter == null)
            {
                return null;
            }

            // Mouse click: move the player's bar to the click Y
            if (selection is IClickSelection cs)
            {
                double y = cs.YClickPos;
                if (_playerNumber == 1)
                    _painter.SetLeftBarY(y - (_painter == null ? 0 : 0));
                else
                    _painter.SetRightBarY(y - (_painter == null ? 0 : 0));
                return null;
            }

            return null;
        }

        // Mouse moved: move the player's bar smoothly
        public void OnMouseMoved(System.Windows.Input.MouseEventArgs e)
        {
            if (_painter == null) return;
            var pos = e.GetPosition(null);
            double y = pos.Y;
            if (_playerNumber == 1)
                _painter.SetLeftBarY(y - _painter.barHeight / 2);
            else
                _painter.SetRightBarY(y - _painter.barHeight / 2);
        }
    }

	// Minimal game field for Pong
	public class B5_Pong_Field : IGameField
	{
		public bool CanBePaintedBy(IPaintGame painter)
		{
			return painter is B5_Pong_Painter;
		}
	}

	// Rules for Pong: enforces win conditions and forwards moves to the painter
	public class B5_Pong_Rules : IGameRules2
	{
		private readonly B5_Pong_Field _field;
		private readonly B5_Pong_Painter _painter;
		
		// Static shared painter instance so HumanPlayers can access it
		private static B5_Pong_Painter _sharedPainter = null;

		public B5_Pong_Rules(B5_Pong_Painter painter = null)
		{
			_field = new B5_Pong_Field();
			_painter = painter ?? new B5_Pong_Painter();
			_sharedPainter = _painter;  // Share this painter instance
			ClearField();
		}
		
		// Public accessor for shared painter
		public static B5_Pong_Painter SharedPainter => _sharedPainter;

		public string Name => "B5 Pong Rules";

		public IGameField CurrentField => _field;

		public bool MovesPossible => !_painter.IsGameOver;

		// Accepts IPlayMove instances; forwards keyboard moves to the painter
		public void DoMove(IPlayMove move)
		{
			if (move == null) 
            {
                return;
            }

			// If move is the KeyMove defined in the painter, forward it
			if (move.GetType().Name == "KeyMove" || move is IPlayMove)
			{
				// Many game components construct a KeyMove with PlayerNumber and Key
				// Forward directly to painter which handles the logic
				_painter.ApplyMove(move);
			}
		}

		public void ClearField()
		{
			_painter.Reset();
		}

		// Returns player number who won, -1 if no player won, 0 if game still running
		public int CheckIfPLayerWon()
		{
			if (_painter.IsGameOver)
            {
				return _painter.Winner;
            }
			return 0;
		}

		// Called when the game is started
		public void StartedGameCall()
		{
			ClearField();
		}

		// Called every tick (40ms) to advance game state
		public void TickGameCall()
		{
			// The painter already updates ball movement when TickPaintGameField is called by the framework.
			// Here we only check for the end condition and can perform additional rule checks.
			if (_painter.IsGameOver)
			{
				// nothing to do for now; CheckIfPLayerWon will report the winner
			}
		}
	}
}
