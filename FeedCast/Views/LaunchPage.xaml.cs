/* 
    Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see the MSDN article about this app, visit http://go.microsoft.com/fwlink/?LinkId=247592 
  
*/
using System;
using System.Collections;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FeedCast.ViewModels;
using FeedCastLibrary;
using Microsoft.Phone.Controls;
using G2PO.Resources;

namespace FeedCast.Views
{
    /// <summary>
    /// The initial application page, that only shows once, where the user
    /// decides what categories he would initially like to populate their reader with.
    /// </summary>
    public partial class LaunchPage : PhoneApplicationPage
    {
        /// <summary>
        /// Collection of initial categories displayed to the user.
        /// </summary>
        private LaunchPageViewModel _initialCategories;

        #region Fade Effect
        /// <summary>
        /// 
        /// </summary>
        private Panel _indicators;

        /// <summary>
        /// 
        /// </summary>
        private TextBlock _errorText;

        /// <summary>
        /// 
        /// </summary>
        private TextBlock _loadingText;

        /// <summary>
        /// 
        /// </summary>
        private PerformanceProgressBar _progressBar;

        /// <summary>
        /// Storyboard for tresizing the background fading effect.
        /// </summary>
        private Storyboard _backgroundResizeStoryboard;

        /// <summary>
        /// Popup used to display the Fade Effect.
        /// </summary>
        private Popup _popup;

        /// <summary>
        /// Overlay containing layers for the Fade Effect.
        /// </summary>
        private Panel _overlay;
        #endregion

        /// <summary>
        /// Constructor called when the page is initialized.
        /// </summary>
        public LaunchPage()
        {
            InitializeComponent();

            // Load all initial categories into memory.
            _initialCategories = new LaunchPageViewModel();

            // Hook into the DownloadsFinished event for all initial categories.
            _initialCategories.LoadContent.LoadingFinished += OnAllDownloadsFinished;
        }

        #region Navigation Overrides
        /// <summary>
        /// Called when the page is first navigated to.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Set the datacontext only once when the page is navigated to.
            CategorySelection.DataContext = _initialCategories;
        }

        /// <summary>
        /// Called when the page is no longer the active page in the root frame. 
        /// </summary>
        /// <param name="e">The Navigation Event Arguments provided.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Remove back entry to prevent the user from accessing this page again.
            if (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }
        }
        #endregion

        /// <summary>
        /// Event handler called when the user cliks the Finished button.
        /// </summary>
        /// <param name="sender">The object that fired this event.</param>
        /// <param name="e">The Event Arguments associated with this event.</param>
        private void OnFinishedClicked(object sender, EventArgs e)
        {
            // Begin the fade navigation to indicate loading.
            OpenPopup();

            // Retrieve the InitialCategories selected by the user.
            IList selectedItems = CategorySelection.SelectedItems as IList;
            if (selectedItems.Count > 0)
            {
                // Begin loading the Initial Categories selected by the user.
                try
                {
                    _initialCategories.LoadSelection(selectedItems);
                }
                catch (WebException)
                {
                    _errorText.Visibility = Visibility.Visible;

                    DispatcherTimer dt = new DispatcherTimer();
                    dt.Interval = TimeSpan.FromSeconds(3D);
                    dt.Tick +=
                        (s, a) =>
                        {
                            ClosePopup();
                            dt.Stop();
                        };
                    dt.Start();
                }
            }
            else
            {
                OnAllDownloadsFinished(sender, null);
            }
        }

        /// <summary>
        /// Event handler called when all downloads have successfully finished.
        /// </summary>
        /// <param name="sender">The sender of this event handler.</param>
        /// <param name="e">The EventArgs for this handler.</param>
        private void OnAllDownloadsFinished(object sender, ContentLoader.LoadingFinishedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                () =>
                {
                    NavigationService.Navigate(new Uri("/Main", UriKind.Relative));
                    ClosePopup();
                });
        }

        /// <summary>
        /// Opens the ProgressBar Popup.
        /// </summary>
        private void OpenPopup()
        {
            // Grab the frame for measurements.
            PhoneApplicationFrame rootVisual = Application.Current.RootVisual as PhoneApplicationFrame;

            // Panel that contains overlay layers
            _overlay = new Canvas { Background = new SolidColorBrush(Colors.Transparent) };

            double width = rootVisual.ActualWidth;
            double height = rootVisual.ActualHeight;

            // Create a layer for the background brush (Prevents edge seeping).
            UIElement backgroundLayer = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = (Brush)Application.Current.Resources["PhoneBackgroundBrush"],
                CacheMode = new BitmapCache(),
            };
            _overlay.Children.Insert(0, backgroundLayer);

            // Create a layer for the page content
            WriteableBitmap writeableBitmap = new WriteableBitmap((int)width, (int)height);
            writeableBitmap.Render(rootVisual, null);
            writeableBitmap.Invalidate();
            ScaleTransform scaleTransform = new ScaleTransform
            {
                CenterX = width / 2,
                CenterY = height / 2
            };
            UIElement contentLayer = new Image
            {
                Source = writeableBitmap,
                RenderTransform = scaleTransform,
                CacheMode = new BitmapCache(),
            };
            _overlay.Children.Insert(1, contentLayer);

            // Create a layer for the background brush
            UIElement backgroundFadeLayer = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = (Brush)Application.Current.Resources["PhoneBackgroundBrush"],
                Opacity = 0,
                CacheMode = new BitmapCache(),
            };
            _overlay.Children.Insert(2, backgroundFadeLayer);

            // Prepare for scale animation
            double from = 1;
            double to = 0.94;
            TimeSpan timespan = TimeSpan.FromSeconds(0.42);
            IEasingFunction easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut };
            _backgroundResizeStoryboard = new Storyboard();

            // Create an animation for the X scale
            DoubleAnimation animationX = new DoubleAnimation { From = from, To = to, Duration = timespan, EasingFunction = easingFunction };
            Storyboard.SetTarget(animationX, scaleTransform);
            Storyboard.SetTargetProperty(animationX, new PropertyPath(ScaleTransform.ScaleXProperty));
            _backgroundResizeStoryboard.Children.Add(animationX);

            // Create an animation for the Y scale
            DoubleAnimation animationY = new DoubleAnimation { From = from, To = to, Duration = timespan, EasingFunction = easingFunction };
            Storyboard.SetTarget(animationY, scaleTransform);
            Storyboard.SetTargetProperty(animationY, new PropertyPath(ScaleTransform.ScaleYProperty));
            _backgroundResizeStoryboard.Children.Add(animationY);

            DoubleAnimation animationFade = new DoubleAnimation { From = 0, To = .8, Duration = timespan, EasingFunction = easingFunction };
            Storyboard.SetTarget(animationFade, backgroundFadeLayer);
            Storyboard.SetTargetProperty(animationFade, new PropertyPath(Rectangle.OpacityProperty));
            _backgroundResizeStoryboard.Children.Add(animationFade);

            _indicators = new StackPanel();

            _progressBar = new PerformanceProgressBar()
            {
                Width = width,
            };

            _indicators.Children.Add(_progressBar);

            _loadingText = new TextBlock
            {
                Text = AppResources.InitialLoadingText,
                Style = (Style)Application.Current.Resources["PhoneTextTitle3Style"],
                TextAlignment = TextAlignment.Center,
                Width = width
            };

            _indicators.Children.Add(_loadingText);

            _errorText = new TextBlock
            {
                Text = AppResources.InitialLoadingFailedText,
                Style = (Style)Application.Current.Resources["PhoneTextAccentStyle"],
                TextAlignment = TextAlignment.Center,
                Width = width,
                Visibility = Visibility.Collapsed
            };

            _indicators.Children.Add(_errorText);

            _overlay.Children.Add(_indicators);

            Canvas.SetLeft(_indicators, 0);
            Canvas.SetTop(_indicators, ((height / 2) - (_indicators.ActualHeight / 2)));

            _popup = new Popup { Child = _overlay };

            _popup.Opened +=
                (s, e) =>
                {
                    _progressBar.IsIndeterminate = true;
                    _backgroundResizeStoryboard.Begin();
                };

            _popup.Closed +=
                (s, e) =>
                {
                    _progressBar.IsIndeterminate = false;
                    _progressBar.Visibility = Visibility.Collapsed;

                    _loadingText.Visibility = Visibility.Collapsed;
                };

            _popup.IsOpen = true;
        }

        /// <summary>
        /// Closes the Popup.
        /// </summary>
        private void ClosePopup()
        {
            _indicators.Visibility = Visibility.Collapsed;
            if (null != _backgroundResizeStoryboard)
            {
                // Swap all the From/To values to reverse the animation
                foreach (DoubleAnimation animation in _backgroundResizeStoryboard.Children)
                {
                    double temp = animation.From.Value;
                    animation.From = animation.To;
                    animation.To = temp;
                }

                // Capture member variables for delegate closure
                Popup popup = _popup;
                Panel overlay = _overlay;
                _backgroundResizeStoryboard.Completed += delegate
                {
                    // Clear/close popup and overlay
                    if (null != popup)
                    {
                        popup.IsOpen = false;
                        popup.Child = null;
                    }
                    if (null != overlay)
                    {
                        overlay.Children.Clear();
                    }
                };

                // Begin the reverse animation
                _backgroundResizeStoryboard.Begin();

                // Reset member variables
                _backgroundResizeStoryboard = null;
                _popup = null;
                _overlay = null;
            }
            else
            {
                if (null != _popup)
                {
                    _popup.IsOpen = false;
                    _popup.Child = null;
                    _popup = null;
                }
                if (null != _overlay)
                {
                    _overlay.Children.Clear();
                    _overlay = null;
                }
            }
        }
    }
}