using ServiceLayer;
using ServiceLayer.DTOs;
using ServiceLayer.Models;
using ServiceLayer.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ExamWork.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderPage.xaml
    /// </summary>
    public partial class MakeOrderPage : Page
    {
        public static List<ProductDTO> ExamOrderList = [];
        public static int orderInProductsCount;
        public static decimal? totalCost;
        public static decimal? totalDiscount;
        public static readonly ExamPickupPointService _pickupPointService = new();
        public static readonly ExamOrderService _orderService = new();
        public static readonly ExamOrderProductService _orderProductService = new();
        public static List<ExamPickupPoint> examPickupPoints = [];
        public static List<int> existingPickupCodes = [];

        public MakeOrderPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CreateOrderList();

            examPickupPoints = await _pickupPointService.GetPickupPointsAsync();
            existingPickupCodes = await _orderService.GetExistingPickupCodesAsync();

            if (CurrentUser.IsGuest)
                CurrentUserLabel.Content = "Вы вошли как гость";
            else
                CurrentUserLabel.Content = $"{CurrentUser.UserName.Substring(0, 1)}.{CurrentUser.UserPatronymic.Substring(0, 1)}. {CurrentUser.UserSurname}";

            PickupPointsComboBox.ItemsSource = examPickupPoints;

            if (CurrentUser.RoleID != 2)
                GoToAllOrdersButton.Visibility = Visibility.Visible;
            else
                GoToAllOrdersButton.Visibility = Visibility.Collapsed;
        }

        private void UpdateDiscount()
        {
            int productsCount = ExamOrderList.Count;
            totalDiscount = 0;
            for (int i = 0; i < productsCount; i++)
            {
                totalDiscount += (ExamOrderList[i].ProductCost - ExamOrderList[i].TotalCost) * ExamOrderList[i].ProductCountInOrder;
                string totalDiscountStr = totalDiscount.HasValue ? totalDiscount.Value.ToString("F2") : "0.00";
                OrderDiscountLabel.Content = $"{totalDiscountStr} руб.";
            }
            if (ExamOrderList.Count == 0)
                OrderDiscountLabel.Content = 0;
        }

        private void UpdateProductsCount()
        {
            int productsCount = ExamOrderList.Count;
            orderInProductsCount = 0;
            for (int i = 0; i < productsCount; i++)
            {
                orderInProductsCount += ExamOrderList[i].ProductCountInOrder;
                CountProductsInOrderLabel.Content = orderInProductsCount.ToString();
            }
            if (ExamOrderList.Count == 0)
                CountProductsInOrderLabel.Content = 0;
        }

        private void UpdateCost()
        {
            int productsCount = ExamOrderList.Count;
            totalCost = 0;
            for (int i = 0; i < productsCount; i++)
            {
                totalCost += ExamOrderList[i].TotalCost * ExamOrderList[i].ProductCountInOrder;
                string totalCostStr = totalCost.HasValue ? totalCost.Value.ToString("F2") : "0.00";
                OrderCostLabel.Content = $"{totalCostStr} руб.";
            }
            if (ExamOrderList.Count == 0)
                OrderCostLabel.Content = 0;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button deleteButton = sender as Button;
            DockPanel countPanel = deleteButton.Parent as DockPanel;
            StackPanel productPanel = countPanel.Parent as StackPanel;
            ExamOrderList.RemoveAt((int)productPanel.Tag);
            if (productPanel.Parent is StackPanel productsInOrderStackPanel)
                productsInOrderStackPanel.Children.Remove(productPanel);
            CreateOrderList();
        }

        private void CountControl_ValueChanged(object sender, RoutedEventArgs e)
        {
            CountControl countControl = sender as CountControl;
            ExamOrderList[Convert.ToInt32(countControl.Tag)].ProductCountInOrder = countControl.Value;
            UpdateProductsCount();
            UpdateCost();
            UpdateDiscount();
            if (countControl.Value == 0)
            {
                DockPanel countPanel = countControl.Parent as DockPanel;
                StackPanel productPanel = countPanel.Parent as StackPanel;
                StackPanel productsInOrderStackPanel = productPanel.Parent as StackPanel;
                ExamOrderList.RemoveAt((int)productPanel.Tag);
                productsInOrderStackPanel?.Children.Remove(productPanel);
                CreateOrderList();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentFrame.CanGoBack)
                App.CurrentFrame.GoBack();
        }

        private void PickupPointsComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            WarnLabel.Content = string.Empty;
        }

        private void GoToYourOrders_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new YourOrdersPage());
        }

        private void GoToAllOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new AllOrdersPage());
        }

        private async void MakeOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExamOrderList.Count != 0)
            {
                if (PickupPointsComboBox.SelectedItem != null)
                {
                    Random rnd = new();
                    int travelDays = rnd.Next(2, 12);
                    DateTime currentDate = DateTime.Now;
                    DateTime deliveryDate = currentDate.AddDays(travelDays);

                    int pickupCode;
                    do
                    {
                        pickupCode = rnd.Next(100, 1000);
                    }
                    while (existingPickupCodes.Contains(pickupCode));

                    await _orderService.AddExamOrderAsync(CurrentUser.UserID, "Новый", currentDate, deliveryDate, examPickupPoints[PickupPointsComboBox.SelectedIndex].OrderPickupPoint, pickupCode);

                    ExamOrder newOrder = new()
                    {
                        OrderId = await _orderService.GetLastOrderIDAsync(),
                        OrderStatus = "Новый",
                        OrderDate = currentDate,
                        OrderDeliveryDate = deliveryDate,
                        OrderPickupPoint = examPickupPoints[PickupPointsComboBox.SelectedIndex].OrderPickupPoint,
                        OrderPickupCode = pickupCode
                    };

                    for (int i = 0; i < ExamOrderList.Count; i++)
                    {
                        await _orderProductService.AddOrderProductAsync(newOrder.OrderId, ExamOrderList[i].ProductArticleNumber, ExamOrderList[i].ProductCountInOrder);
                    }

                    if (CurrentUser.IsGuest)
                    {
                        YourOrdersPage.createdByGuestOrdersList.Add(newOrder);
                    }

                    ExamOrderList.Clear();
                    productsInOrderStackPanel.Children.Clear();
                    App.CurrentFrame.Navigate(new YourOrdersPage());

                }
                else
                {
                    MessageBox.Show("Укажите пункт выдачи");
                    WarnLabel.Content = "*Ошибка";
                }
            }
            else
            {
                MessageBox.Show("Заказ не может быть пустым");
                WarnLabel.Content = "*Ошибка";
            }
        }

        private void CreateOrderList()
        {
            productsInOrderStackPanel.Children.Clear();
            int productsCount = ExamOrderList.Count;
            for (int i = 0; i < productsCount; i++)
            {
                Border productBorder = new()
                {
                    Width = 600,
                    Margin = new Thickness(80, 5, 0, 5),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC6600")),
                    BorderThickness = new(3)
                };

                StackPanel productPanel = new()
                {
                    Tag = i,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCC99"))
                };

                Image productImage = new()
                {
                    Source = new BitmapImage(new Uri(ExamOrderList[i].ProductPhoto)),
                    Width = 200,
                    Height = 200
                };

                productPanel.Children.Add(productImage);

                StackPanel articleNumberPanel = new()
                {
                    Orientation = Orientation.Horizontal
                };

                Label articleNumberLabel = new()
                {
                    Content = "Артикул:"
                };

                Label articleDataLabel = new()
                {
                    Content = ExamOrderList[i].ProductArticleNumber
                };

                articleNumberPanel.Children.Add(articleNumberLabel);
                articleNumberPanel.Children.Add(articleDataLabel);
                productPanel.Children.Add(articleNumberPanel);

                Label nameDataLabel = new()
                {
                    Content = ExamOrderList[i].ProductName
                };

                productPanel.Children.Add(nameDataLabel);

                Label desciptionDataLabel = new()
                {
                    Content = ExamOrderList[i].ProductDescription
                };

                productPanel.Children.Add(desciptionDataLabel);

                StackPanel categoryPanel = new()
                {
                    Orientation = Orientation.Horizontal
                };

                Label categoryLabel = new()
                {
                    Content = "Категория товара:"
                };

                Label categoryDataLabel = new()
                {
                    Content = ExamOrderList[i].ProductCategory
                };

                categoryPanel.Children.Add(categoryLabel);
                categoryPanel.Children.Add(categoryDataLabel);
                productPanel.Children.Add(categoryPanel);

                StackPanel manufacturerPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                Label manufacturerLabel = new()
                {
                    Content = "Производитель товара:"
                };

                Label manufacturerDataLabel = new()
                {
                    Content = ExamOrderList[i].ProductManufacturer
                };

                manufacturerPanel.Children.Add(manufacturerLabel);
                manufacturerPanel.Children.Add(manufacturerDataLabel);
                productPanel.Children.Add(manufacturerPanel);

                DockPanel costDockPanel = new();

                Label costLabel = new()
                {
                    Content = "Цена товара:"
                };

                TextBlock costDataTextBlock = new()
                {
                    Text = ExamOrderList[i].ProductCost.ToString()
                };

                Label discountLabel = new()
                {
                    Content = $"Скидка:",
                    FontSize = 12
                };

                Label discountDataLabel = new()
                {
                    FontSize = 12,
                    Content = ExamOrderList[i].ProductDiscountAmount
                };

                costDockPanel.Children.Add(costLabel);
                costDockPanel.Children.Add(discountDataLabel);
                DockPanel.SetDock(discountDataLabel, Dock.Right);
                costDockPanel.Children.Add(discountLabel);
                DockPanel.SetDock(discountLabel, Dock.Right);
                if (ExamOrderList[i].ProductDiscountAmount > 0)
                {
                    costDataTextBlock.TextDecorations = TextDecorations.Strikethrough;
                    costDataTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    Label costWithDiscountDataLabel = new();
                    decimal resultCost = (decimal)Convert.ToDouble(costDataTextBlock.Text) * (100 - Convert.ToInt32(discountDataLabel.Content)) / 100;
                    costWithDiscountDataLabel.Content = resultCost;
                    costDockPanel.Children.Add(costWithDiscountDataLabel);
                }
                costDockPanel.Children.Add(costDataTextBlock);
                productPanel.Children.Add(costDockPanel);

                StackPanel productStatusPanel = new()
                {
                    Orientation = Orientation.Horizontal
                };

                Label productStatusLabel = new()
                {
                    Content = "Статус:"
                };

                Label productStatusDataLabel = new()
                {
                    Content = ExamOrderList[i].ProductStatus
                };

                productStatusPanel.Children.Add(productStatusLabel);
                productStatusPanel.Children.Add(productStatusDataLabel);
                productPanel.Children.Add(productStatusPanel);

                StackPanel productQuantityInStockPanel = new()
                {
                    Orientation = Orientation.Horizontal
                };

                Label productQuantityInStockLabel = new()
                {
                    Content = "Количество на складе:"
                };

                Label productQuantityInStockDataLabel = new()
                {
                    Content = ExamOrderList[i].ProductQuantityInStock
                };

                productQuantityInStockPanel.Children.Add(productQuantityInStockLabel);
                productQuantityInStockPanel.Children.Add(productQuantityInStockDataLabel);
                productPanel.Children.Add(productQuantityInStockPanel);

                DockPanel countPanel = new();
                CountControl countControl = new()
                {
                    Tag = i,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Value = ExamOrderList[i].ProductCountInOrder,
                    MaxValue = ExamOrderList[i].ProductQuantityInStock
                };

                countControl.countTextBox.Text = ExamOrderList[i].ProductCountInOrder.ToString();
                countControl.ValueChanged += CountControl_ValueChanged;
                Button deleteButton = new();
                deleteButton.Click += DeleteButton_Click;
                Image deleteImage = new();
                deleteButton.Width = 50;
                deleteButton.HorizontalAlignment = HorizontalAlignment.Right;
                deleteImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/delete.png"));
                deleteButton.Content = deleteImage;
                DockPanel.SetDock(deleteButton, Dock.Right);
                countPanel.Children.Add(deleteButton);
                countPanel.Children.Add(countControl);
                productPanel.Children.Add(countPanel);

                productBorder.Child = productPanel;
                productsInOrderStackPanel.Children.Add(productBorder);
            }
            UpdateProductsCount();
            UpdateDiscount();
            UpdateCost();
        }
    }
}
