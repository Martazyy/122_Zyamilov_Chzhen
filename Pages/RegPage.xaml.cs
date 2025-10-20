using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace _122_Zyamilov_Chzhen.Pages
{
    public partial class RegPage : Page
    {
        private byte[] photoData; // Для хранения данных фотографии

        public RegPage()
        {
            InitializeComponent();
            comboBxRole.SelectedIndex = 0; // Устанавливаем роль "User" по умолчанию
        }

        private void btnUploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.png)|*.jpg;*.png|All files (*.*)|*.*",
                Title = "Выберите фотографию"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    photoData = File.ReadAllBytes(openFileDialog.FileName);
                    if (photoData.Length > 5 * 1024 * 1024)
                    {
                        MessageBox.Show("Файл слишком большой! Максимальный размер: 5 МБ.", "Ошибка");
                        photoData = null;
                        lblPhotoFileName.Text = "Файл не выбран";
                        return;
                    }
                    lblPhotoFileName.Text = Path.GetFileName(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}", "Ошибка");
                    photoData = null;
                    lblPhotoFileName.Text = "Файл не выбран";
                }
            }
        }

        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            string login = txtbxLog.Text;
            string password = passBxFrst.Visibility == Visibility.Visible
                ? passBxFrst.Password
                : passBxFrstText.Text;
            string confirmPassword = passBxScnd.Visibility == Visibility.Visible
                ? passBxScnd.Password
                : passBxScndText.Text;
            string fio = txtbxFIO.Text;

            // Проверка заполнения обязательных полей
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(fio))
            {
                MessageBox.Show("Заполните все обязательные поля!");
                return;
            }

            // Проверка уникальности логина
            using (var db = new Entities())
            {
                var user = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == login);

                if (user != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return;
                }
            }

            // Проверка формата пароля
            if (password.Length < 6)
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                return;
            }

            bool en = true;
            bool number = false;
            for (int i = 0; i < password.Length; i++)
            {
                if (password[i] >= '0' && password[i] <= '9')
                    number = true;
                else if (!((password[i] >= 'A' && password[i] <= 'Z') || (password[i] >= 'a' && password[i] <= 'z')))
                    en = false;
            }

            if (!en)
            {
                MessageBox.Show("Используйте только английскую раскладку!");
                return;
            }
            if (!number)
            {
                MessageBox.Show("Добавьте хотя бы одну цифру!");
                return;
            }

            // Проверка совпадения паролей
            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            // Регистрация пользователя
            using (var db = new Entities())
            {
                User userObject = new User
                {
                    FIO = fio,
                    Login = login,
                    Password = GetHash(password),
                    Role = (comboBxRole.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Photo = photoData
                };
                db.User.Add(userObject);
                db.SaveChanges();
                MessageBox.Show("Пользователь успешно зарегистрирован!");
                txtbxLog.Text = "";
                passBxFrst.Password = "";
                passBxFrstText.Text = "";
                passBxScnd.Password = "";
                passBxScndText.Text = "";
                txtbxFIO.Text = "";
                comboBxRole.SelectedIndex = 0;
                photoData = null;
                lblPhotoFileName.Text = "Файл не выбран";
                ShowPasswordCheckBox.IsChecked = false;
                passBxFrst.Visibility = Visibility.Visible;
                passBxFrstText.Visibility = Visibility.Hidden;
                passBxScnd.Visibility = Visibility.Visible;
                passBxScndText.Visibility = Visibility.Hidden;
                NavigationService?.Navigate(new AuthPage());
            }
        }

        private static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Копируем содержимое PasswordBox в TextBox
            passBxFrstText.Text = passBxFrst.Password;
            passBxScndText.Text = passBxScnd.Password;

            // Скрываем PasswordBox, показываем TextBox
            passBxFrst.Visibility = Visibility.Hidden;
            passBxScnd.Visibility = Visibility.Hidden;
            passBxFrstText.Visibility = Visibility.Visible;
            passBxScndText.Visibility = Visibility.Visible;

            // Устанавливаем фокус на активное поле
            if (passBxFrst.IsFocused)
                passBxFrstText.Focus();
            else if (passBxScnd.IsFocused)
                passBxScndText.Focus();
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Копируем содержимое TextBox в PasswordBox
            passBxFrst.Password = passBxFrstText.Text;
            passBxScnd.Password = passBxScndText.Text;

            // Скрываем TextBox, показываем PasswordBox
            passBxFrstText.Visibility = Visibility.Hidden;
            passBxScndText.Visibility = Visibility.Hidden;
            passBxFrst.Visibility = Visibility.Visible;
            passBxScnd.Visibility = Visibility.Visible;

            // Устанавливаем фокус на активное поле
            if (passBxFrstText.IsFocused)
                passBxFrst.Focus();
            else if (passBxScndText.IsFocused)
                passBxScnd.Focus();
        }
    }
}