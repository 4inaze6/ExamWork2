using ExamWork.Pages;
using ServiceLayer;
using ServiceLayer.Services;
using System.Windows;
using System.Windows.Controls;

namespace ExamWork
{
    public partial class AuthorizationPage : Page
    {
        public static readonly ExamUserService _userService = new();
        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private async void AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            var user = await _userService.GetUserByLoginAndPasswordAsync(authorizationLoginTextBox.Text, authorizationPasswordTextBox.Password);
            if (user != null)
            {
                CurrentUser.IsGuest = false;
                CurrentUser.UserID = user.UserId;
                CurrentUser.RoleID = user.RoleId;
                CurrentUser.UserSurname = user.UserSurname;
                CurrentUser.UserName = user.UserName;
                CurrentUser.UserPatronymic = user.UserPatronymic;
                CurrentUser.UserLogin = user.UserLogin;
                CurrentUser.UserPassword = user.UserPassword;
                App.CurrentFrame.Navigate(new ShopPage());
            }
            else
                IncorrectDataLabel.Visibility = Visibility.Visible;
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.IsGuest = true;
            CurrentUser.RoleID = 2;
            App.CurrentFrame.Navigate(new ShopPage());
        }

        private void AuthorizationPasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            IncorrectDataLabel.Visibility = Visibility.Hidden;
        }

        private void AuthorizationLoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            IncorrectDataLabel.Visibility = Visibility.Hidden;
        }
    }
}
