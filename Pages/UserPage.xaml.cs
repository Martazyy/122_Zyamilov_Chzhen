using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _122_Zyamilov_Chzhen.Pages
{
    public partial class UserPage : Page
    {
        private List<User> allUsers;

        public UserPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            using (var db = new Entities())
            {
                allUsers = db.User.ToList();
            }
            UpdateUsers();
        }

        private void UpdateUsers()
        {
            if (allUsers == null) return;

            var filtered = allUsers.AsEnumerable();

            // Фильтр по ФИО
            if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
                filtered = filtered.Where(u => u.FIO.ToLower().Contains(fioFilterTextBox.Text.ToLower()));

            // Только админы
            if (onlyAdminCheckBox.IsChecked == true)
                filtered = filtered.Where(u => u.Role == "Admin");

            // Сортировка
            filtered = sortComboBox.SelectedIndex == 0
                ? filtered.OrderBy(u => u.FIO)
                : filtered.OrderByDescending(u => u.FIO);

            ListUser.ItemsSource = filtered.ToList();
        }

        private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateUsers();
        private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateUsers();
        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e) => UpdateUsers();
        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e) => UpdateUsers();
        private void clearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            fioFilterTextBox.Text = "";
            sortComboBox.SelectedIndex = 0;
            onlyAdminCheckBox.IsChecked = false;
            UpdateUsers();
        }
    }
}
