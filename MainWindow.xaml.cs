using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace _122_Zyamilov_Chzhen
{
    public partial class MainWindow : Window
    {
        private bool isDarkTheme = false; 

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, ev) =>
                DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            timer.Start();
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            isDarkTheme = !isDarkTheme; 
            string uriPath = isDarkTheme ? "DictionaryDark.xaml" : "DictionaryLight.xaml";
            string buttonText = isDarkTheme ? "Светлая тема" : "Тёмная тема";

            var uri = new Uri(uriPath, UriKind.Relative);
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);

            ThemeToggleButton.Content = buttonText; 
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
            else
                MessageBox.Show("Назад перейти нельзя.");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}