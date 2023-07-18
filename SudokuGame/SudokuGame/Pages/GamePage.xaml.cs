using SudokuBoardLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;
using PropertyChanged;
using MaterialDesignThemes.Wpf;
using System.Runtime.Remoting.Lifetime;
using System.Media;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Security.Principal;

namespace SudokuGame.Pages
{
    /// <summary>
    /// Interaction logic for GamePage.xaml
    /// </summary>

    [AddINotifyPropertyChangedInterface]
    public partial class GamePage : Page, IDisposable
    {
        private static GamePage instance;
        private Button previousButton;
        private string currentdiff = ConfigurationManager.AppSettings["Level"];
        private string WindowTitle => $"Sudoku ETH - {currentdiff}";
        private TextBox[,] textBoxArray = new TextBox[9, 9];
        private int[,] arrBorad;
        private int[,] arrBoradClone;
        SudokuBorad borad;
        private DispatcherTimer timer;
        private DateTime startTime;
        private TimeSpan elapsedTime;
        TextBox focusedTextBox;
        private bool isWin;

        public static GamePage Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GamePage();
                }
                else
                {
                    if (instance.focusedTextBox != null)
                    {
                        //instance.focusedTextBox.GotFocus -= instance.TextBox_GotFocus;
                        //instance.focusedTextBox.Focus();

                    }
                    instance.startTime = DateTime.Now - instance.elapsedTime;
                    instance.timer.Start();
                    ((MainWindow)Application.Current.MainWindow).Title = instance.WindowTitle;
                }

                return instance;
            }
        }

        // SoundPlayer[] soundPlayer = null;
        Dictionary<string, SoundPlayer> soundPlayers = null;

        private Dictionary<string, (int filledStars, TimeSpan time, int sum)> difficultyLevels;
        public ObservableCollection<bool> Stars { get; set; }
        public string PopupTitle { get; set; }

        public string PopupContent { get; set; }


        public GamePage()
        {
            InitializeComponent();

            arrBorad = new int[9, 9];
            focusedTextBox = new TextBox();

            soundPlayers = new Dictionary<string, SoundPlayer>();

            string soundFilePath = System.IO.Path.Combine("..", "..", "Sounds");
            string[] soundFiles = Directory.GetFiles(soundFilePath);

            for (int i = 0; i < soundFiles.Length; i++)
            {
                string soundFileName = System.IO.Path.GetFileNameWithoutExtension(soundFiles[i]);
                soundPlayers[soundFileName] = new SoundPlayer(soundFiles[i]);
            }

            borad = new SudokuBorad(arrBorad, currentdiff);
            ((MainWindow)Application.Current.MainWindow).Title = WindowTitle;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            startTime = DateTime.Now;
            timer.Start();
            previousButton = stPanelDiff.Children.OfType<Button>().Where(but =>
            but.Content.ToString() == currentdiff).FirstOrDefault();
            previousButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0072e3"));

            CreateButtonsNumbers();
            FillDictionaryDiffLevels();
            GenerateTextBoxesAndBindTo2DArray();
            DataContext = this;
        }
        private void FillDictionaryDiffLevels()
        {
            difficultyLevels = new Dictionary<string, (int filledStars, TimeSpan time, int sum)>
            {
                { "Impossible", (6, TimeSpan.FromHours(8), 200) },
                { "Extreme", (5, TimeSpan.FromHours(6), 100) },
                { "Hard", (4, TimeSpan.FromHours(3), 80) },
                { "Medium", (3, TimeSpan.FromMinutes(40), 50) },
                { "Easy", (2, TimeSpan.FromMinutes(20), 30) },
                { "Very Easy", (1, TimeSpan.FromMinutes(10), 20) }
            };

            difficultyLevels.TryGetValue(currentdiff, out var level);
            int filledStars = level.filledStars;
            Stars = new ObservableCollection<bool>();
            for (int i = 0; i < difficultyLevels.Keys.Count; i++)
            {
                Stars.Add(i < filledStars);
            }

        }

        SolidColorBrush GenerateSimilarColor(string hexColor)
        {
            string mappedColor = ((ColorDictionary)Application.Current.Resources["colorDictionary"])[hexColor.ToLower()];
            return (SolidColorBrush)new BrushConverter().ConvertFromString(mappedColor);
        }

        private void CreateButtonsNumbers()
        {
            StackPanel st = new StackPanel();

            for (int i = 1; i <= 9; i++)
            {
                Button button = new Button();
                button.Content = i.ToString();
                //  button.Click += Button_Click_Number;
                button.PreviewMouseDown += Button_PreviewMouseDown_Number;
                //button.FontFamily = (FontFamily)Application.Current.Resources["CustomFont"];
                //button.Name = $"button_{i}";
                button.FontFamily = new FontFamily("Copperplate Gothic Light");
                st.Children.Add(button);
                if (i % 3 == 0)
                {
                    stPanelNumbers.Children.Add(st);
                    st = new StackPanel();
                }
            }
        }
        private void UpdateStars()
        {
            difficultyLevels.TryGetValue(currentdiff, out var level);
            int filledStars = level.filledStars;
            for (int i = 0; i < Stars.Count; i++)
            {
                Stars[i] = (i < filledStars);
            }
        }
        void OrderBorderThickness(Border borCurrent, int i, int j)
        {
            //  Border Thickness of borad
            if (i % 3 == 0)
            {
                borCurrent.BorderThickness = new Thickness(0, 1.5, 0, 0);
            }
            else if (i == 8)
            {
                borCurrent.BorderThickness = new Thickness(0, 0, 0, 1.5);
            }
            if (j % 3 == 0)
            {
                borCurrent.BorderThickness = new Thickness(1.5, borCurrent.BorderThickness.Top, 0, borCurrent.BorderThickness.Bottom);
            }
            else if (j == 8)
            {
                borCurrent.BorderThickness = new Thickness(0, borCurrent.BorderThickness.Top, 1.5, borCurrent.BorderThickness.Bottom);
            }
        }
        public void GenerateTextBoxesAndBindTo2DArray()
        {
            spTable.Children.Clear();

            Resources["TextBoxBackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#71c7ec"));
            // Create the grid
            Grid grid = new Grid();

            // Create the rows and columns of the grid
            for (int i = 0; i < 9; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(52) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(52) });
            }

            borad.FillBorad(arrBorad, currentdiff);
            arrBoradClone = (int[,])arrBorad.Clone();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {

                    Border border = new Border();
                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#212F3C"));
                    OrderBorderThickness(border, i, j);

                    TextBox textBox = new TextBox();
                    textBox.BorderThickness = new Thickness(0.6, 0.6, 0.6, 0.6);
                    textBox.Name = "textBox_" + i + "_" + j;
                    textBox.Text = arrBorad[i, j] != 0 ? arrBorad[i, j].ToString() : "";
                    //  textBox.FontFamily = new FontFamily("Copperplate Gothic Light");

                    int submatrixRow = i / 3;
                    int submatrixColumn = j / 3;
                    int submatrixNumber = submatrixRow * 3 + submatrixColumn;

                    int row = i;
                    int column = j;
                    textBox.Tag = (row, column, submatrixNumber);

                    if (arrBorad[i, j] != 0)
                    {
                        textBox.IsReadOnly = true;
                        textBox.Foreground = new SolidColorBrush(Color.FromArgb(255, 59, 68, 75));
                    }

                    textBoxArray[i, j] = textBox;
                    border.Child = textBox;

                    // Add the text box to the grid
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    grid.Children.Add(border);

                }
            }

            spTable.Children.Add(grid);
        }
        void SetRowColumnBackgroundColor()
        {

            var tag = (ValueTuple<int, int, int>)focusedTextBox.Tag;

            textBoxArray.OfType<TextBox>().ToList().ForEach(textBox =>
            {
                var tagTextBoxCurrent = (ValueTuple<int, int, int>)textBox.Tag;

                if (textBox != focusedTextBox)
                {

                    if (focusedTextBox.Text != "" && focusedTextBox.Text == textBox.Text)
                    {
                        textBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9bf4a2"));
                    }
                    else if (textBox != focusedTextBox
                    && (tagTextBoxCurrent.Item1 == tag.Item1 ||
                         tagTextBoxCurrent.Item2 == tag.Item2 ||
                         tagTextBoxCurrent.Item3 == tag.Item3))
                    {
                        textBox.Background = GenerateSimilarColor(((SolidColorBrush)focusedTextBox.Background).Color.ToString());
                    }

                    else
                    {
                        textBox.Background = Brushes.White;
                    }

                }

            }

            );
        }
        void SetColorTextBoxForeground(TextBox changedTextBox, string text)
        {

            string[] nameParts = changedTextBox.Name.Split('_');
            int row = int.Parse(nameParts[1]), col = int.Parse(nameParts[2]);
            int value = int.Parse(text);
            // int indexSound = 0;
            string nameSound = "error-sound";
            var tag = (dynamic)changedTextBox.Tag;
            // Color blue 
            var colorValid = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x72, 0xE3));

            string tbText = changedTextBox.Text;

            if (text != changedTextBox.Text && value != 0)
            {

                if (!borad.validSudoku.IsBoardValid(value, new int[] { row, col }))
                {
                    changedTextBox.Foreground = Brushes.Red;
                }
                else
                {
                    // index 5 is sucess sound
                    nameSound = "success-sound";
                    // indexSound = 5;
                    changedTextBox.Foreground = colorValid;
                }

                changedTextBox.Text = text;
                arrBorad[row, col] = value;
                soundPlayers[nameSound].Play();

            }
            else
            {
                UpdateColorTextBoxesErrorForeground(tag, value);
                changedTextBox.Text = "";
                arrBorad[row, col] = 0;

            }
            if (tbText != "")
            {
                UpdateColorTextBoxesErrorForeground(tag, Convert.ToInt32(tbText));
            }

            SetRowColumnBackgroundColor();

        }
        void UpdateColorTextBoxesErrorForeground(dynamic tag, int value)
        {

            // get list of indexes to for those indexes that need a color change
            var li_indexes = borad.validSudoku.GetIndexesToChnageColor(tag.Item1, tag.Item2, tag.Item3, value);


            // update foreground of textboxes with errors
            foreach (var indexs in li_indexes)
            {
                var tbChnage = textBoxArray[indexs[0], indexs[1]];
                if (!tbChnage.IsReadOnly && (arrBorad[indexs[0], indexs[1]] != 0))
                {
                    tbChnage.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x72, 0xE3));
                }
            }
        }
        void HandleBoardSolved()
        {

            bool IsBoardSolved()
            {
                return textBoxArray.OfType<TextBox>().All(
                    textbox =>
                    textbox.Text != "" && textbox.Foreground != Brushes.Red);
            }

            if (IsBoardSolved())
            {
                PopupTitle = "Congratulations!";
                difficultyLevels.TryGetValue(currentdiff, out var level);
                PopupContent = $"You managed to solve the puzzle on difficulty level {currentdiff}" + Environment.NewLine +
                               $"The amount that will be transferred to your account is: {level.sum}SE." + Environment.NewLine +
                               "Please confirm the transfer and click Ok so that the transfer is done. " + Environment.NewLine +
                               "You can continue playing again while the amount you earned is immediately transferred to your account!";
                isWin = true;
                ShowPopup();
                timer.Stop();
            }

        }
        void ResetGame()
        {
            int row = arrBorad.GetLength(0);
            int col = arrBorad.GetLength(1);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    textBoxArray[i, j].Background = new SolidColorBrush(Colors.White);
                    arrBorad[i, j] = arrBoradClone[i, j];
                    textBoxArray[i, j].Text = arrBorad[i, j] != 0 ? arrBorad[i, j].ToString() : "";
                }
            }
            focusedTextBox = new TextBox();

            Resources["TextBoxBackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#71c7ec"));
        }
        private void ResetTimer()
        {
            //timer.Stop();
            timerTextBlock.Text = "00:00:00";
            startTime = DateTime.Now;
            timer.Start();
        }
        private void ShowPopup()
        {
            PopupHost.IsOpen = true;

        }

        //events 
        private void Button_PreviewMouseDown_Number(object sender, MouseButtonEventArgs e)
        {
            string buttonContent = ((Button)sender).Content.ToString();
            TextBox tb = focusedTextBox;
            if (tb.IsFocused)
            {
                e.Handled = true;
                if (tb.IsReadOnly)
                {
                    return;
                }
                SetColorTextBoxForeground(tb, buttonContent);
                HandleBoardSolved();
            }

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            focusedTextBox = (TextBox)sender;
            focusedTextBox.Background = (SolidColorBrush)Resources["TextBoxBackgroundColor"];
            SetRowColumnBackgroundColor();
            soundPlayers["message-sound"].Play();
            //soundPlayer[3].Play();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox changedTextBox = (TextBox)sender;


            if (e.Key == Key.Back && !changedTextBox.IsReadOnly)
            {

                if (changedTextBox.Text != "")
                {
                    int value = int.Parse(changedTextBox.Text);
                    changedTextBox.Text = "";
                    soundPlayers["remove-sound"].Play();
                    //soundPlayer[4].Play();

                    //update foreground of textboxes with errors
                    var tag = (dynamic)changedTextBox.Tag;
                    arrBorad[tag.Item1, tag.Item2] = 0;

                    UpdateColorTextBoxesErrorForeground(tag, value);

                }

                SetRowColumnBackgroundColor();
                e.Handled = true;
            }
            else if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                changedTextBox.PreviewTextInput += TextBox_PreviewTextInput;
            }

            else
            {
                string[] nameParts = changedTextBox.Name.Split('_');

                int rowIndex = int.Parse(nameParts[1]), columnIndex = int.Parse(nameParts[2]);

                switch (e.Key)
                {
                    case Key.Left:
                        columnIndex = borad.validSudoku.GetMoveValidInBorad(--columnIndex);
                        break;
                    case Key.Right:
                        columnIndex = borad.validSudoku.GetMoveValidInBorad(++columnIndex);
                        break;
                    case Key.Up:
                        rowIndex = borad.validSudoku.GetMoveValidInBorad(--rowIndex);
                        break;
                    case Key.Down:
                        rowIndex = borad.validSudoku.GetMoveValidInBorad(++rowIndex);
                        break;
                    default:
                        changedTextBox.PreviewTextInput -= TextBox_PreviewTextInput;
                        e.Handled = true;
                        return;
                }
                textBoxArray[rowIndex, columnIndex].Focus();

            }
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //Prevent caret of textbox 
            if (sender != null)
            {
                TextBox tb = (sender as TextBox);
                e.Handled = true;
                if (tb.SelectionLength != 0)
                    tb.SelectionLength = 0;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox changedTextBox = (TextBox)sender;


            if (changedTextBox.IsReadOnly)
            {
                return;
            }
            try
            {
                SetColorTextBoxForeground(changedTextBox, e.Text);
            }
            catch
            {

            }
            e.Handled = true;

            HandleBoardSolved();

        }

        private void Button_click_Option(object sender, RoutedEventArgs e)
        {

            switch (((Button)sender).Name)
            {
                case "buNewGame":
                    GenerateTextBoxesAndBindTo2DArray();
                    break;
                case "buResetGame":
                    ResetGame();
                    break;
                case "buErase":
                    break;
                case "buUndo":
                    break;
                default:
                    break;
            }


        }

        private void Click_Change_Difficulty(object sender, RoutedEventArgs e)
        {
            if (previousButton != null)
            {
                previousButton.Foreground = Brushes.Black; // Set the previous button's color back to black
            }

            Button butCurrent = (sender as Button);
            butCurrent.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0072e3"));
            previousButton = butCurrent;
            currentdiff = butCurrent.Content.ToString();
            Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            config.AppSettings.Settings["Level"].Value = currentdiff;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            GenerateTextBoxesAndBindTo2DArray();
            UpdateStars();
            ((MainWindow)Application.Current.MainWindow).Title = WindowTitle;
        }

        private void Change_Background_Color(object sender, RoutedEventArgs e)
        {

            Button button = (Button)sender;
            SolidColorBrush buttonColor = (SolidColorBrush)button.Background;
            Resources["TextBoxBackgroundColor"] = buttonColor;
            focusedTextBox.Focus();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedTime = DateTime.Now - startTime;
            timerTextBlock.Text = elapsedTime.ToString(@"hh\:mm\:ss");
            difficultyLevels.TryGetValue(currentdiff, out var level);
            if (elapsedTime >= level.time)
            {

                PopupTitle = $"Game Over!";
                PopupContent = "We regret to inform you that your Sudoku game session has ended" + Environment.NewLine +
                               "due to the expiration of the allocated time." + Environment.NewLine +
                               "Unfortunately, you were unable to complete the puzzle within the given timeframe." + Environment.NewLine +
                                "As a result, a certain amount will be deducted from your account." + Environment.NewLine +
                                "Please note that the deducted amount is based on the terms and conditions outlined" + Environment.NewLine +
                                "in our game policy. If you have any questions or concerns regarding this deduction," + Environment.NewLine +
                                "please don't hesitate to contact our support team." + Environment.NewLine +
                                "We are here to assist you and address any queries you may have. You can keep playing again and try to solve and earn";

                isWin = false;
                ShowPopup();
                timer.Stop();
            }

        }


        private async void ChargeLoss()
        {

            Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            var requestData = new
            {
                from = config.AppSettings.Settings["Account"].Value,
                to = config.AppSettings.Settings["MangerAccount"].Value,
                amount = difficultyLevels[currentdiff].sum / 10
            };
            //  MessageBox.Show($"from = {requestData.from} to = {requestData.to} amount = {requestData.amount}");
            string json = System.Text.Json.JsonSerializer.Serialize(requestData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.PostAsync("https://sodukusmartcontractsce.onrender.com/transferFrom", new StringContent(json, Encoding.UTF8, "application/json"));


        }


        private async void DepositToWin()
        {

            Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            var requestData = new
            {
                account = config.AppSettings.Settings["Account"].Value,
                amount = difficultyLevels[currentdiff].sum
            };
            string json = System.Text.Json.JsonSerializer.Serialize(requestData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.PostAsync("https://sodukusmartcontractsce.onrender.com/transfer", new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {

                var myMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(6));

                // Enqueue the message
                myMessageQueue.Enqueue("The amount " + difficultyLevels[currentdiff].sum + "SE has been deposited into the account successfully!");

                // Assign the SnackbarMessageQueue to the Snackbar control
                snackbarDeposit.MessageQueue = myMessageQueue;

                // Show the Snackbar
                snackbarDeposit.IsActive = true;
            }
        }
        private void PopupOK_Click(object sender, RoutedEventArgs e)
        {
            PopupHost.IsOpen = false;
            if (isWin)
            {
                DepositToWin();
            }
            else
            {
                ChargeLoss();
            }

            GenerateTextBoxesAndBindTo2DArray();
            ResetTimer();
        }



        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {


            if (buIconArrow.Kind == PackIconKind.ArrowForward)
            {
                buIconArrow.Kind = PackIconKind.ArrowBack;
                menuPopup.IsOpen = false;
            }
            else
            {
                buIconArrow.Kind = PackIconKind.ArrowForward;
                menuPopup.IsOpen = true;
            }
            //buIconArrow.Kind = buIconArrow.Kind == PackIconKind.ArrowForward ? PackIconKind.ArrowBack : PackIconKind.ArrowForward;


            //if (menuPopup.IsOpen)
            //{
            //    menuPopup.IsOpen = false;
            //}
            //else
            //{
            //    menuPopup.IsOpen = true;
            //}
            //if (stMenuGame.Visibility == Visibility.Collapsed)
            //{
            //    stMenuGame.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    stMenuGame.Visibility = Visibility.Collapsed;
            //}

        }


        private void menuPopup_Closed(object sender, EventArgs e)
        {
            menuPopup.IsOpen = false;
            buIconArrow.Kind = PackIconKind.ArrowBack;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle the menu item selection
            switch (int.Parse((sender as Button)?.Tag.ToString()))
            {
                case 0:
                    break;
                case 1:
                    ((MainWindow)Application.Current.MainWindow).BackMainWinodw();
                    break;
                default:
                    Application.Current.Shutdown();
                    break;
            }
            menuPopup.IsOpen = false;
        }
        public void Dispose()
        {
            // Stop and dispose the timer
            timer.Stop();
            //timer.Tick -= Timer_Tick;
            //timer = null;
        }

        public void ExitGame()
        {

            if (PopupHost.IsOpen)
            {
                if (isWin)
                {
                    DepositToWin();
                }
                else
                {
                    ChargeLoss();
                }

            }
        }


    }
}
