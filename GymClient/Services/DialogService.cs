using GymClient.Interfaces;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GymClient.Services
{
    public class DialogService : IDialogService
    {
        private readonly SemaphoreSlim _dialogLock = new(1, 1);

        public async Task ShowMessage(string message)
        {
            await _dialogLock.WaitAsync(); 
            try
            {
                var stack = new StackPanel { Margin = new Thickness(16) };

                stack.Children.Add(new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap
                });

                var okButton = new System.Windows.Controls.Button
                {
                    Content = "OK",
                    Margin = new Thickness(0, 16, 0, 0),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right
                };
                stack.Children.Add(okButton);

                okButton.Click += (_, __) =>
                    DialogHost.CloseDialogCommand.Execute(null, okButton);

                await DialogHost.Show(stack);
            }
            finally
            {
                _dialogLock.Release();         
            }
        }

        public async Task<bool> ShowConfirmation(string message)
        {
            await _dialogLock.WaitAsync();    
            try
            {
                var stack = new StackPanel { Margin = new Thickness(16) };

                stack.Children.Add(new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap
                });

                var buttonPanel = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Margin = new Thickness(0, 16, 0, 0)
                };

                var cancelButton = new System.Windows.Controls.Button { Content = "Отмена", Margin = new Thickness(0, 0, 8, 0) };
                var okButton = new System.Windows.Controls.Button { Content = "Удалить" };

                buttonPanel.Children.Add(cancelButton);
                buttonPanel.Children.Add(okButton);
                stack.Children.Add(buttonPanel);

                bool result = false;

                okButton.Click += (_, __) =>
                {
                    result = true;
                    DialogHost.CloseDialogCommand.Execute(null, okButton);
                };

                cancelButton.Click += (_, __) =>
                {
                    result = false;
                    DialogHost.CloseDialogCommand.Execute(null, cancelButton);
                };

                await DialogHost.Show(stack);
                return result;
            }
            finally
            {
                _dialogLock.Release();
            }
        }

        public async Task ShowMessageWithColoredEmail(string message, string email, System.Windows.Media.Brush? emailColor)
        {
            await _dialogLock.WaitAsync();
            try
            {
                var stack = new StackPanel { Margin = new Thickness(16) };

                var textBlock = new TextBlock { TextWrapping = TextWrapping.Wrap };

                textBlock.Inlines.Add(message + " ");

                var run = new Run(email)
                {
                    Foreground = emailColor ?? System.Windows.Media.Brushes.Red,
                    FontWeight = FontWeights.Bold
                };
                textBlock.Inlines.Add(run);

                stack.Children.Add(textBlock);

                var okButton = new System.Windows.Controls.Button
                {
                    Content = "OK",
                    Margin = new Thickness(0, 16, 0, 0),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right
                };
                stack.Children.Add(okButton);

                okButton.Click += (_, __) =>
                    DialogHost.CloseDialogCommand.Execute(null, okButton);

                await DialogHost.Show(stack);
            }
            finally
            {
                _dialogLock.Release();
            }
        }
    }
}
