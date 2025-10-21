using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace _122_Zyamilov_Chzhen.Pages
{
    public partial class AddPaymentPage : Page
    {
        private Payment _currentPayment;

        public AddPaymentPage(Payment selectedPayment)
        {
            InitializeComponent();

            CBCategory.ItemsSource = Entities.GetContext().Category.ToList();
            CBUser.ItemsSource = Entities.GetContext().User.ToList();

            if (selectedPayment != null)
                _currentPayment = selectedPayment; // редактирование
            else
                _currentPayment = null; // новая запись

            // Если редактирование — заполняем поля
            if (_currentPayment != null)
            {
                TBPaymentName.Text = _currentPayment.Name;
                TBAmount.Text = _currentPayment.Price.ToString();
                TBCount.Text = _currentPayment.Num.ToString();
                DPDate.SelectedDate = _currentPayment.Date;
                CBUser.SelectedValue = _currentPayment.UserID;
                CBCategory.SelectedValue = _currentPayment.CategoryID;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            // Валидация
            if (string.IsNullOrWhiteSpace(TBPaymentName.Text))
                errors.AppendLine("Укажите название!");
            if (!decimal.TryParse(TBAmount.Text, out decimal price) || price <= 0)
                errors.AppendLine("Укажите корректную сумму!");
            if (!int.TryParse(TBCount.Text, out int num) || num <= 0)
                errors.AppendLine("Укажите корректное количество!");
            if (DPDate.SelectedDate == null)
                errors.AppendLine("Укажите дату!");
            if (CBUser.SelectedValue == null)
                errors.AppendLine("Выберите клиента!");
            if (CBCategory.SelectedValue == null)
                errors.AppendLine("Выберите категорию!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка заполнения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var context = Entities.GetContext();

            try
            {
                if (_currentPayment == null)
                {
                    // Новый платеж
                    var newPayment = new Payment
                    {
                        Name = TBPaymentName.Text.Trim(),
                        Price = price,
                        Num = num,
                        Date = DPDate.SelectedDate.Value,
                        UserID = (int)CBUser.SelectedValue,
                        CategoryID = (int)CBCategory.SelectedValue
                    };
                    context.Payment.Add(newPayment);
                }
                else
                {
                    // Редактирование
                    _currentPayment.Name = TBPaymentName.Text.Trim();
                    _currentPayment.Price = price;
                    _currentPayment.Num = num;
                    _currentPayment.Date = DPDate.SelectedDate.Value;
                    _currentPayment.UserID = (int)CBUser.SelectedValue;
                    _currentPayment.CategoryID = (int)CBCategory.SelectedValue;
                }

                context.SaveChanges();

                MessageBox.Show("Платёж успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Закрываем страницу или очищаем поля
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                var fullMessage = new StringBuilder();
                fullMessage.AppendLine("Ошибка при сохранении!");
                fullMessage.AppendLine("Тип: " + ex.GetType().Name);
                fullMessage.AppendLine("Сообщение: " + ex.Message);
                if (ex.InnerException != null)
                {
                    fullMessage.AppendLine("--- Внутреннее исключение ---");
                    fullMessage.AppendLine(ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                        fullMessage.AppendLine(ex.InnerException.InnerException.Message);
                }
                MessageBox.Show(fullMessage.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBPaymentName.Text = "";
            TBAmount.Text = "";
            TBCount.Text = "";
            DPDate.SelectedDate = null;
            CBUser.SelectedIndex = -1;
            CBCategory.SelectedIndex = -1;
        }
    }
}