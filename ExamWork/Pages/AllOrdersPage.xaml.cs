using ServiceLayer;
using ServiceLayer.DTOs;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExamWork.Pages
{
    /// <summary>
    /// Логика взаимодействия для AllOrdersPage.xaml
    /// </summary>
    public partial class AllOrdersPage : Page
    {
        public static List<OrderSummaryDTO> Orders = [];
        public static readonly ExamOrderProductService _orderProductService = new();
        public static readonly ExamOrderService _orderService = new();
        public static readonly ExamUserService _userService = new();
        public static readonly ExamProductService _productService = new();
        public AllOrdersPage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GetOrderList();
            CurrentUserLabel.Content = $"{CurrentUser.UserName.Substring(0, 1)}.{CurrentUser.UserPatronymic.Substring(0, 1)}. {CurrentUser.UserSurname}";
        }

        private async void ChangeStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedIndex == 0)
                await _orderService.UpdateExamOrderStatus("Новый", Convert.ToInt32(comboBox.Tag));
            else
                await _orderService.UpdateExamOrderStatus("Завершен", Convert.ToInt32(comboBox.Tag));
            GetOrderList();
        }


        private void SearchByIdButton_Click(object sender, RoutedEventArgs e)
        {
            var targetBorder = ordersStackPanel.Children
                                    .OfType<Border>()
                                    .FirstOrDefault(b => b.Tag.ToString() == IdTextBox.Text);

            if (targetBorder != null)
            {
                var scrollViewer = FindParent<ScrollViewer>(ordersStackPanel);
                if (scrollViewer != null)
                {
                    var position = targetBorder.TransformToAncestor(ordersStackPanel)
                                                .Transform(new Point(0, 0));
                    scrollViewer.ScrollToVerticalOffset(position.Y);
                }
            }
            else
            {
                MessageBox.Show("Заказа с данным id не найдено");
            }
        }
        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            Button changeButton = sender as Button;
            StackPanel stackPanel = changeButton.Parent as StackPanel;
            TextBox newDeliveryDateTextBox = stackPanel.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "changeTextBox");
            try
            {
                _orderService.UpdateExamOrderDeliveryDate(Convert.ToDateTime(newDeliveryDateTextBox.Text), Convert.ToInt32(changeButton.Tag));
                MessageBox.Show("Дата доставки изменена");
            }
            catch
            {
                MessageBox.Show("Введен некорректный формат даты");
            }
            GetOrderList();
        }

        private void ToStartButton_Click(object sender, RoutedEventArgs e)
        {
            var scrollViewer = FindParent<ScrollViewer>(ordersStackPanel);
            scrollViewer.ScrollToVerticalOffset(0);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentFrame.CanGoBack)
                App.CurrentFrame.GoBack();
        }

        private async void GetOrderList()
        {
            ordersStackPanel.Children.Clear();
            Orders = await _orderService.GetOrdersAsync();
            int productsCount = Orders.Count;
            for (int i = 0; i < productsCount; i++)
            {
                List<ExamOrderProduct> orderProducts = await _orderProductService.GetProductsInOrder(Orders[i].OrderID);

                Border orderBorder = new()
                {
                    Tag = Orders[i].OrderID.ToString(),
                    Width = 600,
                    Margin = new Thickness(80, 5, 0, 5),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC6600")),
                    BorderThickness = new(3)
                };

                StackPanel orderPanel = new()
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCC99"))
                };

                DockPanel statusPanel = new();

                Label orderIdLabel = new()
                {
                    Content = $"Заказ №{Orders[i].OrderID}"
                };

                Label orderStatus = new();
                DockPanel.SetDock(orderStatus, Dock.Right);
                orderStatus.Content = Orders[i].OrderStatus;
                statusPanel.Children.Add(orderStatus);
                statusPanel.Children.Add(orderIdLabel);
                orderPanel.Children.Add(statusPanel);

                StackPanel orderCompositionPanel = new()
                {
                    Orientation = Orientation.Horizontal
                };

                Label orderCompositionLabel = new();
                string orderComposition = "";
                for (int j = 0; j < orderProducts.Count; j++)
                {
                    var product = await _productService.GetProductNameByArticleAsync(orderProducts[j].ProductArticleNumber);
                    orderComposition += $"\n{product}({await _orderProductService.GetProductAmountInOrderWithArticle(Orders[i].OrderID, orderProducts[j].ProductArticleNumber)})";
                }
                orderCompositionLabel.Content = $"Состав заказа:{orderComposition}";
                orderCompositionPanel.Children.Add(orderCompositionLabel);
                orderPanel.Children.Add(orderCompositionPanel);

                Label orderSumLabel = new();
                decimal? orderSum = _orderProductService.GetSumOrder(Orders[i].OrderID);
                string orderSumStr = orderSum.HasValue ? orderSum.Value.ToString("F2") : "0.00";
                orderSumLabel.Content = $"Сумма заказа: {orderSumStr}";
                orderPanel.Children.Add(orderSumLabel);

                Label orderDiscountLabel = new();
                decimal? orderDiscount = _orderProductService.GetDiscountOrder(Orders[i].OrderID);
                string orderDiscontStr = orderDiscount.HasValue ? orderDiscount.Value.ToString("F2") : "0.00";
                orderDiscountLabel.Content = $"Сумма скидки в заказе: {orderDiscontStr}";
                orderPanel.Children.Add(orderDiscountLabel);

                if (Orders[i].UserID != 0)
                {
                    Label orderPickupPointLabel = new();
                    string? fullName = await _userService.GetUserFullNameWithOrderIdAsync(Orders[i].OrderID);
                    orderPickupPointLabel.Content = !string.IsNullOrWhiteSpace(fullName) ? $"ФИО клиента: {fullName}" : "Заказчик неавторизован";
                    orderPanel.Children.Add(orderPickupPointLabel);
                }

                Label orderDateLabel = new()
                {
                    Content = $"Дата: {Orders[i].OrderDate}"
                };

                orderPanel.Children.Add(orderDateLabel);

                StackPanel deliveryStackPanel = new()
                {
                    Orientation = Orientation.Horizontal
                };

                Label orderDeliveryDateLabel = new()
                {
                    Content = $"Дата доставки:\n{Orders[i].OrderDeliveryDate:yyyy-MM-dd}"
                };

                StackPanel changeDeliveryDateStackPanel = new();

                Label changeDeliveryDateLabel = new()
                {
                    Content = "Изменить дату доставки:"
                };

                TextBox changeDeliveryDateTextBox = new();
                changeDeliveryDateTextBox.Name = "changeTextBox";

                Button changeButton = new()
                {
                    Tag = Orders[i].OrderID,
                    Content = "Изменить"
                };

                changeButton.Click += ChangeButton_Click;
                StackPanel statusStackPanel = new();

                Label changeStatusLabel = new()
                {
                    Content = "Статус:"
                };

                ComboBox changeStatusComboBox = new()
                {
                    Tag = Orders[i].OrderID
                };

                changeStatusComboBox.Items.Add("Новый");
                changeStatusComboBox.Items.Add("Завершен");
                changeStatusComboBox.SelectionChanged += ChangeStatusComboBox_SelectionChanged;
                statusStackPanel.Children.Add(changeStatusLabel);
                statusStackPanel.Children.Add(changeStatusComboBox);
                changeDeliveryDateStackPanel.Children.Add(changeDeliveryDateLabel);
                changeDeliveryDateStackPanel.Children.Add(changeDeliveryDateTextBox);
                changeDeliveryDateStackPanel.Children.Add(changeButton);
                deliveryStackPanel.Children.Add(orderDeliveryDateLabel);
                deliveryStackPanel.Children.Add(changeDeliveryDateStackPanel);
                deliveryStackPanel.Children.Add(statusStackPanel);
                orderPanel.Children.Add(deliveryStackPanel);

                orderBorder.Child = orderPanel;
                ordersStackPanel.Children.Add(orderBorder);
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            return parentObject is T parent ? parent : FindParent<T>(parentObject);
        }
    }
}
