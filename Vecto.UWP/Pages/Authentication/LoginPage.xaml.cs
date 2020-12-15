﻿using Refit;
using System;
using System.Net.Http;
using Vecto.Application.Login;
using Vecto.UWP.Services;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Vecto.UWP.Pages.Authentication
{
    public sealed partial class LoginPage : Page
    {
        private readonly IApiService _service;
        private readonly PasswordVault _passwordVault;

        public LoginPage()
        {
            _service = CustomRefitService.For<IApiService>();
            _passwordVault = new PasswordVault();
            InitializeComponent();

            /* 
             * if password was stored fill in fields automatically
             * probably want to change this to automatically log in when we have a sign out button
             */
            var credential = GetCredential();
            if (credential != null)
            {
                credential.RetrievePassword();

                EmailTextBox.Text = credential.UserName;
                PasswordBox.Password = credential.Password;
                RememberMe.IsChecked = true;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginProgressRing.Visibility = Visibility.Visible;

            var model = new LoginDTO() { Email = EmailTextBox.Text, Password = PasswordBox.Password };
            bool rememberMe = RememberMe.IsChecked ?? false;

            // attempt login
            string token = "";
            try
            {
                token = await _service.Login(model);

                if (rememberMe) StorePassword(model);
                else ClearPassword();

                StoreToken(token);

                Frame.Navigate(typeof(MainPage));
            }
            catch (ApiException)
            {
                ErrorTextBlock.Text = "Username or password incorrect";
            }
            catch (HttpRequestException)
            {
                ErrorTextBlock.Text = "Connection timed out";
            }
            finally
            {
                LoginProgressRing.Visibility = Visibility.Collapsed;
            }
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegisterPage), null, new SuppressNavigationTransitionInfo());
        }

        private void StorePassword(LoginDTO model)
        {
            _passwordVault.Add(new PasswordCredential("Vecto", model.Email, model.Password));
        }

        private void ClearPassword()
        {
            _passwordVault.Remove(GetCredential());
        }

        private void StoreToken(string token)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["token"] = token.Replace(@"""", "");
        }

        private PasswordCredential GetCredential()
        {
            try
            {
                var credentialEnumerator = _passwordVault.FindAllByResource("Vecto").GetEnumerator();
                credentialEnumerator.MoveNext();
                var credential = credentialEnumerator.Current;
                return credential;
            }
            catch
            {
                return null;
            }
        }
    }
}
