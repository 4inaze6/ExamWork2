using ExamWork.Pages;
using ServiceLayer;
using System.Windows;

namespace ExamWork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CurrentUser.IsGuest = true;
            CurrentUser.RoleID = 2;
            App.CurrentFrame = MainFrame;
            MainFrame.Navigate(new ShopPage());
        }
    }
}