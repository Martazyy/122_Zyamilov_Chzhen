using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace _122_Zyamilov_Chzhen.Pages
{
    public partial class PaymentTabPage : Page
    {
        public PaymentTabPage()
        {
            InitializeComponent();
            UpdateDataGrid();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
                UpdateDataGrid();
        }

        private void UpdateDataGrid()
        {
            var context = Entities.GetContext();

            // Перезагружаем только существующие записи, чтобы не было ошибки с Added
            context.ChangeTracker.Entries()
                .Where(x => x.Entity is Payment && x.State != System.Data.Entity.EntityState.Added)
                .ToList()
                .ForEach(x => x.Reload());

            // Обновляем DataGrid из базы
            DataGridPayment.ItemsSource = context.Payment.ToList();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу добавления нового платежа
            NavigationService?.Navigate(new AddPaymentPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridPayment.SelectedItems.Cast<Payment>().ToList();
            if (selected.Count == 0) return;

            if (MessageBox.Show($"Удалить {selected.Count} записей?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var context = Entities.GetContext();
                    context.Payment.RemoveRange(selected);
                    context.SaveChanges();
                    UpdateDataGrid();
                    MessageBox.Show("Данные удалены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var payment = (sender as Button).DataContext as Payment;
            if (payment != null)
            {
                // Переходим на страницу редактирования
                NavigationService?.Navigate(new AddPaymentPage(payment));
            }
        }
    }
}
