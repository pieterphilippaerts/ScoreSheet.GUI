using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Information;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Keyboard = System.Windows.Input.Keyboard;

namespace PieterP.ScoreSheet.GUI.Views.Controls
{
    public class BindablePasswordBox : Decorator
    {
        /// <summary>
        /// The password dependency property.
        /// </summary>
        public static readonly DependencyProperty PasswordProperty;

        private bool _isPreventCallback;
        private RoutedEventHandler _savedCallback;

        /// <summary>
        /// Static constructor to initialize the dependency properties.
        /// </summary>
        static BindablePasswordBox()
        {
            PasswordProperty = DependencyProperty.Register(
                "Password",
                typeof(string),
                typeof(BindablePasswordBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnPasswordPropertyChanged))
            );
        }

        /// <summary>
        /// Saves the password changed callback and sets the child element to the password box.
        /// </summary>
        public BindablePasswordBox()
        {
            _savedCallback = HandlePasswordChanged;

            Child = GenerateChildLayout();
        }

        /// <summary>
        /// The password dependency property.
        /// </summary>
        public string Password
        {
            get { return GetValue(PasswordProperty) as string; }
            set { SetValue(PasswordProperty, value); }
        }

        /// <summary>
        /// The password box where the user enters its password.
        /// </summary>
        internal PasswordBox PasswordBox { get; set; }

        /// <summary>
        /// Notification panel containing the message and image to be displayed when the user has CAPS-LOCK activated.
        /// </summary>
        private StackPanel CapsNotificationPanel { get; set; }

        /// <summary>
        /// Handles changes to the password dependency property.
        /// </summary>
        /// <param name="d">the dependency object</param>
        /// <param name="eventArgs">the event args</param>
        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs eventArgs)
        {
            BindablePasswordBox bindablePasswordBox = (BindablePasswordBox)d;
            if (bindablePasswordBox._isPreventCallback)
            {
                return;
            }

            bindablePasswordBox.PasswordBox.PasswordChanged -= bindablePasswordBox._savedCallback;
            bindablePasswordBox.PasswordBox.Password = (eventArgs.NewValue != null) ? eventArgs.NewValue.ToString() : string.Empty;
            bindablePasswordBox.PasswordBox.PasswordChanged += bindablePasswordBox._savedCallback;
        }

        /// <summary>
        /// Handles the password changed event.
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="eventArgs">the event args</param>
        private void HandlePasswordChanged(object sender, RoutedEventArgs eventArgs)
        {
            PasswordBox passwordBox = (PasswordBox)sender;

            _isPreventCallback = true;
            Password = passwordBox.Password;
            _isPreventCallback = false;
        }

        /// <summary>
        /// Detects whether CAPS-LOCK is activated. If it is activated, the notification panel will be shown.
        /// Accepts an optional parameter of <see cref="RoutedEventArgs">event arguments</see> so that the check can be skipped when a key is pressed that is not the CAPS-LOCK key.
        /// </summary>
        /// <param name="eventArgs">Optional event arguments to determine whether the CAPS-LOCK key itself was pressed.</param>
        private void DetectCapsLock(RoutedEventArgs eventArgs = null)
        {
            // When a key event is passed and it's not based on the caps lock key, it means this function has been called for any other key.
            // The toggled state of the caps lock key will not have changed when a different key is pressed.
            if (eventArgs is KeyEventArgs keyEventArgs && keyEventArgs.Key != Key.CapsLock)
                return;

            if (CapsNotificationPanel != null)
                CapsNotificationPanel.Visibility = Keyboard.IsKeyToggled(Key.CapsLock) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Generates the child layout to be rendered.
        /// Contains a password box, image and text box. The image and text box are used as notification
        /// </summary>
        /// <returns>The UIElement to be used as the layout.</returns>
        private UIElement GenerateChildLayout()
        {
            if (PasswordBox == null)
            {
                PasswordBox = new PasswordBox();
                PasswordBox.PasswordChanged += HandlePasswordChanged;
                PasswordBox.KeyDown += (_, e) => DetectCapsLock(e);
                PasswordBox.GotFocus += (_, __) => DetectCapsLock();
                PasswordBox.LostFocus += (_, __) => CapsNotificationPanel.Visibility = Visibility.Collapsed;
            }

            if (CapsNotificationPanel == null)
            {
                var notificationImage = new Image();
                notificationImage.Margin = new Thickness(5, 0, 5, 0);
                notificationImage.Source = new BitmapImage(new Uri("pack://application:,,,/images/warning_48.png"));
                notificationImage.Height = 16;
                notificationImage.Width = 16;

                var notificationText = new TextBlock();
                notificationText.Text = Strings.CapsLock_On;

                CapsNotificationPanel = new StackPanel();
                CapsNotificationPanel.Margin = new Thickness(5);
                CapsNotificationPanel.Orientation = Orientation.Horizontal;
                CapsNotificationPanel.Children.Add(notificationImage);
                CapsNotificationPanel.Children.Add(notificationText);
                DetectCapsLock();
            }

            var fullLayout = new StackPanel();
            fullLayout.Orientation = Orientation.Vertical;
            fullLayout.Children.Add(PasswordBox);
            fullLayout.Children.Add(CapsNotificationPanel);

            return fullLayout;
        }
    }
}
