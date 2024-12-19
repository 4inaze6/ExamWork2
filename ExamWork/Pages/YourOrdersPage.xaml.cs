using ServiceLayer.Models;
using ServiceLayer.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;
using ServiceLayer;

namespace ExamWork.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class YourOrdersPage : Page
    {
        public static List<ExamOrder> createdByGuestOrdersList = [];

        public static List<ExamOrder> examCreatedOrdersList = [];

        public static readonly ExamOrderService _orderService = new();

        public static readonly ExamOrderProductService _orderProductService = new();

        public static readonly ExamProductService _productService = new();

        public YourOrdersPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            examCreatedOrdersList = CurrentUser.IsGuest ? createdByGuestOrdersList : await _orderService.GetOrdersByUserIdAsync(CurrentUser.UserID);

            GetOrderList();

            if (CurrentUser.IsGuest)
                CurrentUserLabel.Content = "Вы вошли как гость";
            else
                CurrentUserLabel.Content = $"{CurrentUser.UserName.Substring(0, 1)}.{CurrentUser.UserPatronymic.Substring(0, 1)}. {CurrentUser.UserSurname}";

            if (CurrentUser.RoleID != 2)
                GoToAllOrdersButton.Visibility = Visibility.Visible;
            else
                GoToAllOrdersButton.Visibility = Visibility.Collapsed;
        }

        private void PrintOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Button printOrderButton = (Button)sender;

            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Текстовый файл (*.txt)|*.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string textToSave = printOrderButton.Tag.ToString();

                File.WriteAllText(saveFileDialog.FileName, textToSave);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new ShopPage());
        }

        private void GoToAllOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new AllOrdersPage());
        }
        private async void GetOrderList()
        {
            YourOrdersStackPanel.Children.Clear();
            int productsCount = examCreatedOrdersList.Count;
            for (int i = 0; i < productsCount; i++)
            {
                List<ExamOrderProduct> examOrderProducts = await _orderProductService.GetProductsInOrder(examCreatedOrdersList[i].OrderId);

                Border orderBorder = new()
                {
                    Width = 600,
                    Margin = new Thickness(80, 5, 0, 5),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC6600")),
                    BorderThickness = new(3)
                };

                StackPanel orderPanel = new()
                {
                    Tag = i,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCC99"))
                };

                Label orderIdLabel = new()
                {
                    Content = $"Заказ №{examCreatedOrdersList[i].OrderId}"
                };

                orderPanel.Children.Add(orderIdLabel);

                Label orderDateLabel = new()
                {
                    Content = $"Дата: {examCreatedOrdersList[i].OrderDate}"
                };

                orderPanel.Children.Add(orderDateLabel);

                StackPanel orderCompositionPanel = new()
                {
                    Orientation = Orientation.Horizontal
                };

                Label orderCompositionLabel = new();
                string orderComposition = "";

                for (int j = 0; j < examOrderProducts.Count; j++)
                {
                    orderComposition += $"\n{await _productService.GetProductNameByArticleAsync(examOrderProducts[j].ProductArticleNumber)}({await _orderProductService.GetProductAmountInOrderWithArticle(examCreatedOrdersList[i].OrderId, examOrderProducts[j].ProductArticleNumber)})";
                }

                orderCompositionLabel.Content = $"Состав заказа:{orderComposition}";
                orderCompositionPanel.Children.Add(orderCompositionLabel);
                orderPanel.Children.Add(orderCompositionPanel);

                Label orderSumLabel = new();
                decimal? orderSum = _orderProductService.GetSumOrder(examCreatedOrdersList[i].OrderId);
                string orderSumStr = orderSum.HasValue ? orderSum.Value.ToString("F2") : "0.00";
                orderSumLabel.Content = $"Сумма заказа: " + orderSumStr;
                orderPanel.Children.Add(orderSumLabel);

                Label orderDiscountLabel = new();
                decimal? orderDiscount = _orderProductService.GetDiscountOrder(examCreatedOrdersList[i].OrderId);
                string orderDiscontStr = orderDiscount.HasValue ? orderDiscount.Value.ToString("F2") : "0.00";
                orderDiscountLabel.Content = $"Сумма скидки в заказе: " + orderDiscontStr;
                orderPanel.Children.Add(orderDiscountLabel);

                Label orderPickupPointLabel = new()
                {
                    Content = $"Пункт выдачи: {examCreatedOrdersList[i].OrderPickupPoint}"
                };

                orderPanel.Children.Add(orderPickupPointLabel);

                DockPanel dockPanel = new();

                Label orderPickupCodeLabel = new()
                {
                    Content = $"Код получения: {examCreatedOrdersList[i].OrderPickupCode}"
                };

                Button printOrderButton = new()
                {
                    Content = "Распечатать талон заказа",
                    Margin = new(5),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCC99")),
                    BorderThickness = new Thickness(4)
                };

                printOrderButton.Click += PrintOrderButton_Click;
                printOrderButton.Tag = $"Заказ №{examCreatedOrdersList[i].OrderId}\nДата: {examCreatedOrdersList[i].OrderDate}\nСостав заказа:{orderComposition}\nСумма заказа: {(orderSum.HasValue ? orderSum.Value.ToString("F2") : "0.00")}\nСумма скидки в заказе: {(orderDiscount.HasValue ? orderDiscount.Value.ToString("F2") : "0.00")}\nПункт выдачи: {examCreatedOrdersList[i].OrderPickupPoint}\nКод получения: {examCreatedOrdersList[i].OrderPickupCode}";
                DockPanel.SetDock(printOrderButton, Dock.Right);
                dockPanel.Children.Add(printOrderButton);
                dockPanel.Children.Add(orderPickupCodeLabel);
                orderPanel.Children.Add(dockPanel);

                orderBorder.Child = orderPanel;
                YourOrdersStackPanel.Children.Add(orderBorder);
            }
        }
    }
}
