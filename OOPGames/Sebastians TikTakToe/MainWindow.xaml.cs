using System.Windows;
using System.Windows.Controls;

namespace SebastiansTikTacToe
{
    public partial class MainWindow : Window
    {
        private TicTacToeGame _game;

        public MainWindow()
        {
            InitializeComponent();
            _game = new TicTacToeGame();
            UpdateUI();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            bool computer = ComputerCheck.IsChecked == true;
            bool computerStarts = (StartPlayerCombo.SelectedIndex == 1);
            _game.Reset(computer, computerStarts);
            UpdateUI();
            if (_game.IsComputerTurn)
            {
                DoComputerMoveIfNeeded();
            }
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Name.Length == 5)
            {
                int r = b.Name[3] - '0';
                int c = b.Name[4] - '0';
                if (_game.MakeMove(r, c))
                {
                    UpdateUI();
                    if (_game.Winner != 0 || _game.IsDraw)
                    {
                        ShowEndState();
                        return;
                    }

                    DoComputerMoveIfNeeded();
                }
            }
        }

        private async void DoComputerMoveIfNeeded()
        {
            if (_game.IsComputerTurn && !_game.IsGameOver)
            {
                await System.Threading.Tasks.Task.Delay(300);
                _game.MakeComputerMove();
                UpdateUI();
                if (_game.Winner != 0 || _game.IsDraw)
                {
                    ShowEndState();
                }
            }
        }

        private void ShowEndState()
        {
            if (_game.Winner != 0)
            {
                StatusText.Text = _game.Winner == 1 ? "Player X wins!" : "Player O wins!";
            }
            else if (_game.IsDraw)
            {
                StatusText.Text = "Draw!";
            }
        }

        private void UpdateUI()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    string name = $"Btn{r}{c}";
                    if (this.FindName(name) is Button btn)
                    {
                        int val = _game.GetCell(r, c);
                        btn.Content = val == 1 ? "X" : val == 2 ? "O" : string.Empty;
                        btn.IsEnabled = (val == 0 && !_game.IsGameOver && !_game.IsComputerTurn);
                    }
                }
            }

            if (_game.IsGameOver)
            {
                if (_game.Winner != 0) StatusText.Text = _game.Winner == 1 ? "Player X wins!" : "Player O wins!";
                else if (_game.IsDraw) StatusText.Text = "Draw!";
            }
            else
            {
                StatusText.Text = _game.CurrentPlayer == 1 ? "Player X's turn" : "Player O's turn";
            }
        }
    }
}
