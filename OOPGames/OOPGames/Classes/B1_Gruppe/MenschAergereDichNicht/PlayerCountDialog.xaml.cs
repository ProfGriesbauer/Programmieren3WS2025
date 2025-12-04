using System.Windows;
using System.Windows.Controls;

namespace OOPGames.B1_Gruppe.MenschAergereDichNicht
{
    public partial class PlayerCountDialog : Window
    {
        public int SelectedPlayerCount { get; private set; } = 4;
        public bool[] IsComputerPlayer { get; private set; } = new bool[4]; // true = Computer, false = Human
        public bool AdvancedMode { get; private set; } = false;

        public PlayerCountDialog()
        {
            InitializeComponent();
            
            // Sicherheitsprüfung
            if (Radio4Players != null)
            {
                Radio4Players.IsChecked = true;
            }
            
            UpdatePlayerPanelVisibility();
        }

        private void PlayerCountChanged(object sender, RoutedEventArgs e)
        {
            UpdatePlayerPanelVisibility();
        }

        private void UpdatePlayerPanelVisibility()
        {
            // Null-Checks für alle Controls
            if (Player3Panel == null || Player4Panel == null || 
                Radio2Players == null || Radio3Players == null || Radio4Players == null)
                return;
                
            if (Radio2Players.IsChecked == true)
            {
                Player3Panel.Visibility = Visibility.Collapsed;
                Player4Panel.Visibility = Visibility.Collapsed;
            }
            else if (Radio3Players.IsChecked == true)
            {
                Player3Panel.Visibility = Visibility.Visible;
                Player4Panel.Visibility = Visibility.Collapsed;
            }
            else if (Radio4Players.IsChecked == true)
            {
                Player3Panel.Visibility = Visibility.Visible;
                Player4Panel.Visibility = Visibility.Visible;
            }
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Null-Checks für alle Controls
                if (Radio2Players == null || Radio3Players == null || 
                    Player1Computer == null || Player2Computer == null || 
                    Player3Computer == null || Player4Computer == null)
                {
                    System.Windows.MessageBox.Show("Dialog nicht vollständig initialisiert!");
                    DialogResult = false;
                    Close();
                    return;
                }
                
                // Bestimme Spieleranzahl
                if (Radio2Players.IsChecked == true)
                    SelectedPlayerCount = 2;
                else if (Radio3Players.IsChecked == true)
                    SelectedPlayerCount = 3;
                else
                    SelectedPlayerCount = 4;

                // Bestimme Spieler-Typen
                IsComputerPlayer[0] = Player1Computer.IsChecked == true;
                IsComputerPlayer[1] = Player2Computer.IsChecked == true;
                IsComputerPlayer[2] = Player3Computer.IsChecked == true;
                IsComputerPlayer[3] = Player4Computer.IsChecked == true;

                // Bestimme Spielmodus
                AdvancedMode = AdvancedModeCheckBox?.IsChecked == true;

                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Fehler: {ex.Message}");
                DialogResult = false;
                Close();
            }
        }
    }
}
