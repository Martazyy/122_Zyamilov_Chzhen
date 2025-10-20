using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _122_Zyamilov_Chzhen
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            string login = TextBoxLogin.Text;
            string password = PasswordBox.Visibility == Visibility.Visible ? PasswordBox.Password : PasswordTextBox.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                LoginErrorText.Text = "Введите логин";
                PasswordErrorText.Text = "Введите пароль";
                LoginErrorText.Visibility = string.IsNullOrEmpty(login) ? Visibility.Visible : Visibility.Hidden;
                PasswordErrorText.Visibility = string.IsNullOrEmpty(password) ? Visibility.Visible : Visibility.Hidden;
                return;
            }

            LoginErrorText.Visibility = Visibility.Hidden;
            PasswordErrorText.Visibility = Visibility.Hidden;

            if (failedAttempts >= 3)
            {
                bool isCaptchaValid = ShowCaptchaWindow();
                if (!isCaptchaValid)
                {
                    return;
                }
                failedAttempts = 0;
            }

            string hashedPassword = GetHash(password);

            using (var db = new Entities())
            {
                var user = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == login && u.Password == hashedPassword);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!");
                    failedAttempts++;
                    return;
                }
                else
                {
                    MessageBox.Show("Пользователь успешно найден!");
                    switch (user.Role)
                    {
                        case "User":
                            NavigationService?.Navigate(new Pages.UserPage());
                            break;
                        case "Admin":
                            //NavigationService?.Navigate(new AdminPage());
                            break;
                    }
                }
            }
        }

        private bool ShowCaptchaWindow()
        {
            // Создание всплывающего окна
            Window captchaWindow = new Window
            {
                Title = "Капча",
                Height = 300,
                Width = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                Owner = Window.GetWindow(this),
                Background = (System.Windows.Media.Brush)Application.Current.Resources["CenterBackground"]
            };

            // Создание сетки для элементов
            Grid grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Метка капчи
            Label labelCaptcha = new Label
            {
                Content = "Капча:",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 5)
            };
            Grid.SetRow(labelCaptcha, 0);
            grid.Children.Add(labelCaptcha);

            // Текст капчи
            TextBox captchaTextBox = new TextBox
            {
                Name = "captcha",
                Height = 42,
                Foreground = System.Windows.Media.Brushes.Red,
                FontStyle = FontStyles.Italic,
                FontWeight = FontWeights.Heavy,
                Background = null,
                FontStretch = FontStretches.UltraCondensed,
                IsReadOnly = true,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 20,
                Margin = new Thickness(0, 0, 0, 10)
            };
            CommandManager.AddPreviewExecutedHandler(captchaTextBox, textBox_PreviewExecuted);
            captchaTextBox.ContextMenu = null;
            Grid.SetRow(captchaTextBox, 1);
            grid.Children.Add(captchaTextBox);

            // Поле для ввода
            TextBox captchaInput = new TextBox
            {
                Name = "captchaInput",
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 34,
                TextWrapping = TextWrapping.Wrap,
                Text = "",
                Width = 160,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(captchaInput, 2);
            grid.Children.Add(captchaInput);

            // Кнопка подтверждения
            Button submitCaptcha = new Button
            {
                Content = "Подтвердить",
                ToolTip = "Подтвердить ввод капчи",
                Height = 34,
                Width = 120,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(submitCaptcha, 3);
            grid.Children.Add(submitCaptcha);

            // Установка содержимого окна
            captchaWindow.Content = grid;

            // Генерация CAPTCHA
            string captchaText = CaptchaChange();
            captchaTextBox.Text = captchaText;

            // Флаг успешной проверки CAPTCHA
            bool isCaptchaValid = false;

            // Обработчик кнопки подтверждения
            submitCaptcha.Click += (s, e) =>
            {
                if (captchaInput.Text != captchaText)
                {
                    MessageBox.Show("Неверно введена капча", "Ошибка");
                    captchaText = CaptchaChange();
                    captchaTextBox.Text = captchaText;
                    captchaInput.Text = "";
                }
                else
                {
                    MessageBox.Show("Капча введена успешно, можете продолжить авторизацию", "Успех");
                    isCaptchaValid = true;
                    captchaWindow.Close();
                }
            };

            // Показ окна в модальном режиме
            captchaWindow.ShowDialog();

            return isCaptchaValid;
        }

        private string CaptchaChange()
        {
            string allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z," +
                               "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z," +
                               "1,2,3,4,5,6,7,8,9,0";
            char[] separator = { ',' };
            string[] ar = allowchar.Split(separator);
            string pwd = "";
            Random r = new Random();

            for (int i = 0; i < 6; i++)
            {
                pwd += ar[r.Next(0, ar.Length)];
            }
            return pwd;
        }

        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Pages.RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Pages.ChangePassPage());
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoginErrorText.Visibility = Visibility.Hidden;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordErrorText.Visibility = Visibility.Hidden;
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Hidden;
            PasswordTextBox.Visibility = Visibility.Visible;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordTextBox.Visibility = Visibility.Hidden;
            PasswordBox.Visibility = Visibility.Visible;
        }
    }
}