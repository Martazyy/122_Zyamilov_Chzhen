using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _122_Zyamilov_Chzhen.Pages
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string login = TbLogin.Text.Trim();
            string currentPass = CurrentPasswordBox.Password;
            string newPass = NewPasswordBox.Password;
            string confirmPass = ConfirmPasswordBox.Password;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(currentPass) ||
                string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirmPass))
            {
                MessageBox.Show("Все поля обязательны к заполнению!");
                return;
            }

            using (var db = new Entities())
            {
                string hashedCurrent = GetHash(currentPass);
                var user = db.User.FirstOrDefault(u => u.Login == login && u.Password == hashedCurrent);

                if (user == null)
                {
                    MessageBox.Show("Логин или текущий пароль введены неверно!");
                    return;
                }

                // Проверка нового пароля
                if (newPass.Length < 6)
                {
                    MessageBox.Show("Новый пароль слишком короткий, минимум 6 символов!");
                    return;
                }

                bool en = true;
                bool number = false;
                for (int i = 0; i < newPass.Length; i++)
                {
                    if (newPass[i] >= '0' && newPass[i] <= '9')
                        number = true;
                    else if (!((newPass[i] >= 'A' && newPass[i] <= 'Z') || (newPass[i] >= 'a' && newPass[i] <= 'z')))
                        en = false;
                }

                if (!en)
                {
                    MessageBox.Show("Используйте только английскую раскладку!");
                    return;
                }
                if (!number)
                {
                    MessageBox.Show("Добавьте хотя бы одну цифру в пароль!");
                    return;
                }

                if (newPass != confirmPass)
                {
                    MessageBox.Show("Новый пароль и подтверждение не совпадают!");
                    return;
                }

                // Сохраняем новый пароль
                user.Password = GetHash(newPass);
                db.SaveChanges();

                MessageBox.Show("Пароль успешно изменен!");
                NavigationService?.Navigate(new AuthPage());
            }
        }

        private static string GetHash(string password)
        {
            using (SHA1 sha = SHA1.Create())
            {
                return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }
    }
}
