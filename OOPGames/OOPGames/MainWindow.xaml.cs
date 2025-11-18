using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OOPGames
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    /// Kommentar zu GitHub Desktop
    public partial class MainWindow : Window
    {
        IGamePlayer _CurrentPlayer = null;
        IPaintGame _CurrentPainter = null;
        IGameRules _CurrentRules = null;
        IGamePlayer _CurrentPlayer1 = null;
        IGamePlayer _CurrentPlayer2 = null;
        B3_Jarde_Roeder b3Fishing;


        System.Windows.Threading.DispatcherTimer _PaintTimer = null;

        public MainWindow()
        {
            InitializeComponent();
            
            //REGISTER YOUR CLASSES HERE
            
            //Register A5 Classes first
            A5_Gomeringer register = new A5_Gomeringer();
            register.Register();

            //Register Snake Game
            A5_Main.Register(OOPGamesManager.Singleton);

            //Painters
            OOPGamesManager.Singleton.RegisterPainter(new X_TicTacToePaint());

            //Rules
            OOPGamesManager.Singleton.RegisterRules(new X_TicTacToeRules());
            
            
            //Players
            OOPGamesManager.Singleton.RegisterPlayer(new X_TicTacToeHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new X_TicTacToeComputerPlayer());


            //A4 Painters
       
            OOPGamesManager.Singleton.RegisterPainter(new A4_TicTacToePaint());
            OOPGamesManager.Singleton.RegisterRules(new A4_TicTacToeRules());
            OOPGamesManager.Singleton.RegisterPlayer(new A4_TicTacToeHumanPlayer());
            // Register A4 computer players so they appear in the Player dropdowns
            OOPGamesManager.Singleton.RegisterPlayer(new A4_ComputerNormal());
            OOPGamesManager.Singleton.RegisterPlayer(new A4_ComputerUnbeatable());

            //A2 Painters
            OOPGamesManager.Singleton.RegisterPainter(new A2_Painter());
            OOPGamesManager.Singleton.RegisterRules(new A2_Rules());
            OOPGamesManager.Singleton.RegisterPlayer(new A2_HumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new A2_ComputerPlayer());  
            //OOPGamesManager.Singleton.RegisterPlayer(new A2_HumanPlayer());  
              

            //A3_LEA TicTacToe
            OOPGamesManager.Singleton.RegisterPainter(new A3_LEA_TicTacToePaint());
            OOPGamesManager.Singleton.RegisterRules(new A3_LEA_TicTacToeRules());
            OOPGamesManager.Singleton.RegisterPlayer(new A3_LEA_TicTacToeHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new A3_LEA_TicTacToeComputerPlayer());

            //A3_LEA IQ Puzzle
            OOPGamesManager.Singleton.RegisterPainter(new A3_LEA_IQPuzzlePaint());
            OOPGamesManager.Singleton.RegisterRules(new A3_LEA_IQPuzzleRules());
            OOPGamesManager.Singleton.RegisterPlayer(new A3_LEA_IQPuzzleHumanPlayer());
            
            //A3_LEA Schiffe Versenken
            OOPGamesManager.Singleton.RegisterPainter(new A3_LEA_SchiffePaint());
            OOPGamesManager.Singleton.RegisterRules(new A3_LEA_SchiffeRules());
            OOPGamesManager.Singleton.RegisterPlayer(new A3_LEA_HumanSchiffePlayer());
            
            
            // B3 Jarde_Roeder
            b3Fishing = new B3_Jarde_Roeder();
            OOPGamesManager.Singleton.RegisterPainter(b3Fishing);
            OOPGamesManager.Singleton.RegisterRules(b3Fishing);
            OOPGamesManager.Singleton.RegisterPainter(new B3_Mika_Roeder_Paint());
            OOPGamesManager.Singleton.RegisterRules(new B3_Mika_Roeder_Rules());
            OOPGamesManager.Singleton.RegisterPlayer(new B3_Mika_Roeder_HumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new B3_Mika_Roeder_ComputerPlayer());

            //B4 TicTacToe (Justus_Lorenz)
            OOPGamesManager.Singleton.RegisterPainter(new B4_TicTacToePaint());
            OOPGamesManager.Singleton.RegisterRules(new B4_TicTacToeRules());
            OOPGamesManager.Singleton.RegisterPlayer(new B4_TicTacToeHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new B4_TicTacToeComputerPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new B4_TicTacToeHardComputer());
            OOPGamesManager.Singleton.RegisterPlayer(new B4_TicTacToeMediumComputer());
            
            FlappyBird flappy = new FlappyBird();
            flappy.Register();

            FroggoGame froggo = new FroggoGame();
            froggo.Register();


            // B2 group (Moritz & Tobias) - TicTacToe
            OOPGamesManager.Singleton.RegisterPainter(new B2_TicTacToePainter());
            OOPGamesManager.Singleton.RegisterRules(new B2_TicTacToeRules());
            OOPGamesManager.Singleton.RegisterPlayer(new B2_HumanTicTacToePlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new B2_ComputerTicTacToePlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new B2_SmartComputerTicTacToePlayer());

            // B2 group (Moritz & Tobias) - Maze Game (2 Players)
            OOPGamesManager.Singleton.RegisterPainter(new B2_MazePainter2Player());
            OOPGamesManager.Singleton.RegisterRules(new B2_MazeRules());
            OOPGamesManager.Singleton.RegisterPlayer(new B2_MazeHumanPlayer());

            //b5 TicTacToe (Felix_Anton)test
            OOPGamesManager.Singleton.RegisterPainter(new B5_TicTacToe_Painter());
            OOPGamesManager.Singleton.RegisterRules(new B5_TicTacToe_Rules());
            OOPGamesManager.Singleton.RegisterPlayer(new B5_TicTacToe_HumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new B5_TicTacToe_ComputerPlayer());

            //b5 Shellshock (Felix_Anton)
            OOPGamesManager.Singleton.RegisterPainter(new B5_Shellshock_Painter());
            OOPGamesManager.Singleton.RegisterRules(new B5_Shellshock_Rules());
            OOPGamesManager.Singleton.RegisterPlayer(new B5_Shellshock_HumanPlayer());

            

            // Populate ListBoxes with registered items
            PaintList.ItemsSource = OOPGamesManager.Singleton.Painters;
            Player1List.ItemsSource = OOPGamesManager.Singleton.Players;
            Player2List.ItemsSource = OOPGamesManager.Singleton.Players;
            RulesList.ItemsSource = OOPGamesManager.Singleton.Rules;
            
            _PaintTimer = new System.Windows.Threading.DispatcherTimer();
            _PaintTimer.Interval = new TimeSpan(0, 0, 0, 0, 40);
            _PaintTimer.Tick += _PaintTimer_Tick; 
            _PaintTimer.Start();
        }
        /// Halllo
        private void _PaintTimer_Tick(object sender, EventArgs e)
        {
            if (_CurrentPainter != null &&
                _CurrentRules != null)
            {
                if (_CurrentPainter is IPaintGame2 &&
                    _CurrentRules.CurrentField != null &&
                    _CurrentRules.CurrentField.CanBePaintedBy(_CurrentPainter))
                {
                    ((IPaintGame2)_CurrentPainter).TickPaintGameField(PaintCanvas, _CurrentRules.CurrentField);
                }

                if (_CurrentRules is IGameRules2)
                {
                    ((IGameRules2)_CurrentRules).TickGameCall();
                }

                //Call MouseMoved event for HumanGamePlayers with mouse support
                if (_CurrentPlayer is IHumanGamePlayerWithMouse humanPlayerWithMouse)
                {
                    //  get the current mouse event args on PaintCanvas
                    var mousePos = Mouse.GetPosition(PaintCanvas);
                    MouseEventArgs mouseEventArgs = new MouseEventArgs(Mouse.PrimaryDevice, 0)
                    {
                        RoutedEvent = Mouse.MouseMoveEvent,
                        Source = this
                    };
                    humanPlayerWithMouse.OnMouseMoved(mouseEventArgs);
                }
            }
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            List<IGamePlayer> activePlayers = new List<IGamePlayer>();
            _CurrentPlayer1 = null;
            if (Player1List.SelectedItem is IGamePlayer)
            {
                _CurrentPlayer1 = ((IGamePlayer)Player1List.SelectedItem).Clone();
                _CurrentPlayer1.SetPlayerNumber(1);
                activePlayers.Add(_CurrentPlayer1);
            }
            _CurrentPlayer2 = null;
            if (Player2List.SelectedItem is IGamePlayer)
            {
                _CurrentPlayer2 = ((IGamePlayer)Player2List.SelectedItem).Clone();
                _CurrentPlayer2.SetPlayerNumber(2);
                activePlayers.Add(_CurrentPlayer2);
            }

            _CurrentPlayer = null;
            _CurrentPainter = PaintList.SelectedItem as IPaintGame;
            _CurrentRules = RulesList.SelectedItem as IGameRules;

            OOPGamesManager.Singleton.RegisterActivePlayers(activePlayers);
            OOPGamesManager.Singleton.RegisterActivePainter(_CurrentPainter);
            OOPGamesManager.Singleton.RegisterActiveRules(_CurrentRules);

            if (_CurrentRules is IGameRules2)
            {
                ((IGameRules2)_CurrentRules).StartedGameCall();
            }

            if (_CurrentPainter != null && 
                _CurrentRules != null && _CurrentRules.CurrentField.CanBePaintedBy(_CurrentPainter))
            {
                _CurrentPlayer = _CurrentPlayer1;
                Status.Text = "Game startet!";
                Status.Text = "Player " + _CurrentPlayer.PlayerNumber + "'s turn!";
                _CurrentRules.ClearField();
                PaintCanvas.Focus();
                _CurrentPainter.PaintGameField(PaintCanvas, _CurrentRules.CurrentField);
                DoComputerMoves();
            }
        }

        private void DoComputerMoves()
        {
            int winner = _CurrentRules.CheckIfPLayerWon();
            if (winner > 0)
            {
                Status.Text = "Player " + winner + " Won!";
                ScheduleRestartIfNeeded();
            }
            else
            {
                while (_CurrentRules.MovesPossible &&
                       winner <= 0 &&
                       _CurrentPlayer is IComputerGamePlayer)
                {
                    IPlayMove pm = ((IComputerGamePlayer)_CurrentPlayer).GetMove(_CurrentRules.CurrentField);
                    if (pm != null)
                    {
                        _CurrentRules.DoMove(pm);
                        _CurrentPainter.PaintGameField(PaintCanvas, _CurrentRules.CurrentField);
                        _CurrentPlayer = _CurrentPlayer == _CurrentPlayer1 ? _CurrentPlayer2 : _CurrentPlayer1;
                        Status.Text = "Player " + _CurrentPlayer.PlayerNumber + "'s turn!";
                    }

                    winner = _CurrentRules.CheckIfPLayerWon();
                    if (winner > 0)
                    {
                        Status.Text = "Player " + winner + " Won!";
                        ScheduleRestartIfNeeded();
                    }
                }

                // If the board is full and there's no winner, it's a draw — schedule restart as well
                //A4 Restart Logic
                if (!_CurrentRules.MovesPossible && winner <= 0)
                {
                    Status.Text = "Draw!";
                    ScheduleRestartIfNeeded();
                }
            }
        }

        private void PaintCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int winner = _CurrentRules.CheckIfPLayerWon();
            if (winner > 0)
            {
                Status.Text = "Player " + winner + " Won!";
            }
            else
            {
                if (_CurrentRules.MovesPossible &&
                    _CurrentPlayer is IHumanGamePlayer)
                {
                    IClickSelection sel;
                    var px = (int)e.GetPosition(PaintCanvas).X;
                    var py = (int)e.GetPosition(PaintCanvas).Y;
                    var btn = (int)e.ChangedButton;
                        sel = new A3_LEA_ClickSelection(px, py, btn);

                    IPlayMove pm = null;

                    // Let the player's GetMove method handle the click mapping
                    pm = ((IHumanGamePlayer)_CurrentPlayer).GetMove(sel, _CurrentRules.CurrentField);
                    
                    if (pm != null)
                    {
                        _CurrentRules.DoMove(pm);
                        _CurrentPainter.PaintGameField(PaintCanvas, _CurrentRules.CurrentField);
                        _CurrentPlayer = _CurrentPlayer == _CurrentPlayer1 ? _CurrentPlayer2 : _CurrentPlayer1;
                        Status.Text = "Player " + _CurrentPlayer.PlayerNumber + "'s turn!";
                    }

                    DoComputerMoves();
                    // If the human move caused a win, schedule restart von Gruppe A4 :)
                    winner = _CurrentRules.CheckIfPLayerWon();
                    if (winner > 0)
                    {
                        ScheduleRestartIfNeeded();
                    }
                }
            }
        }

        private void PaintCanvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Behandle Rechtsklick im Preview-Event (bevor Context-Menü öffnet)
            int winner = _CurrentRules.CheckIfPLayerWon();
            if (winner <= 0 && _CurrentRules.MovesPossible && _CurrentPlayer is IHumanGamePlayer)
            {
                // Erstelle Click-Selection für Rechtsklick (Button=1)
                var px = (int)e.GetPosition(PaintCanvas).X;
                var py = (int)e.GetPosition(PaintCanvas).Y;
                    var sel = new A3_LEA_ClickSelection(px, py, 1);

                IPlayMove pm = ((IHumanGamePlayer)_CurrentPlayer).GetMove(sel, _CurrentRules.CurrentField);
                
                if (pm != null)
                {
                    _CurrentRules.DoMove(pm);
                    _CurrentPainter.PaintGameField(PaintCanvas, _CurrentRules.CurrentField);
                    _CurrentPlayer = _CurrentPlayer == _CurrentPlayer1 ? _CurrentPlayer2 : _CurrentPlayer1;
                    Status.Text = "Player " + _CurrentPlayer.PlayerNumber + "'s turn!";
                    DoComputerMoves();
                }

                // Markiere Event als verarbeitet, damit kein Context-Menü öffnet
                e.Handled = true;
            }
        }

        private void PaintCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Weitergabe des MouseWheel-Events an den IHumanGamePlayerWithMouse
            if (_CurrentPlayer is IHumanGamePlayerWithMouse humanPlayerWithMouse)
            {
                humanPlayerWithMouse.OnMouseMoved(e);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _PaintTimer.Tick -= _PaintTimer_Tick;
            _PaintTimer.Stop();
            _PaintTimer = null;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_CurrentRules == null) return;
            int winner = _CurrentRules.CheckIfPLayerWon();
            if (winner > 0)
            {
                Status.Text = "Player" + winner + " Won!";
            }
            else
            {
                if (_CurrentRules.MovesPossible &&
                    _CurrentPlayer is IHumanGamePlayer)
                {
                    IPlayMove pm = ((IHumanGamePlayer)_CurrentPlayer).GetMove(new KeySelection(e.Key), _CurrentRules.CurrentField);
                    if (pm != null)
                    {
                        _CurrentRules.DoMove(pm);
                        _CurrentPlayer = _CurrentPlayer == _CurrentPlayer1 ? _CurrentPlayer2 : _CurrentPlayer1;
                        Status.Text = "Player " + _CurrentPlayer.PlayerNumber + "'s turn!";
                    }
                    //Restart Logic for Gruppe A4 :)
                    DoComputerMoves();
                    winner = _CurrentRules.CheckIfPLayerWon();
                    if (winner > 0)
                    {
                        ScheduleRestartIfNeeded();
                    }
                }
            }
        }

        // Schedule a restart after 3 seconds if the AutoRestart checkbox is checked
        private void ScheduleRestartIfNeeded()
        {
            try
            {
                if (AutoRestartCheck != null && AutoRestartCheck.IsChecked == true)
                {
                    var timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(3);
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        RestartGame();
                    };
                    timer.Start();
                }
            }
            catch
            {
                // ignore scheduling errors
            }
        }

        // Restarts the current game: clears the rules field, repaints and resets the status and current player
        private void RestartGame()
        {
            if (_CurrentRules == null || _CurrentPainter == null) return;

            _CurrentRules.ClearField();
            // repaint
            _CurrentPainter.PaintGameField(PaintCanvas, _CurrentRules.CurrentField);

            // Reset current player to player1 if available
            if (_CurrentPlayer1 != null)
            {
                _CurrentPlayer = _CurrentPlayer1;
                Status.Text = "Game restarted! Player " + _CurrentPlayer.PlayerNumber + "'s turn!";
                // If the first player is a computer, let it make its move(s)
                DoComputerMoves();
            }
            else
            {
                Status.Text = "Game restarted!";
            }
        }
    }
}
