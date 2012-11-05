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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using FeedCast.ViewModels;
using FeedCastLibrary;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FeedCast.Resources;
using System.Windows.Data;
using FeedCast.Models;
using G2PO.Resources;

namespace FeedCast
{
    /// <summary>
    /// The mainpage of the application. Allows the user to view recent articles,
    /// add and view new feeds, categories, and offers "featured" suggestions.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// Categories in the "All" PanoramaItem.
        /// </summary>
        public MainPageAllCategoriesViewModel _allCategories;

        /// <summary>
        /// Feeds in the "All" PanoramaItem.
        /// </summary>
        public MainPageAllFeedsViewModel _allFeeds;

        /// <summary>
        /// Articles in the "What's New" PanoramaItem.
        /// </summary>
        public MainPageWhatsNewViewModel _whatsNewArticles;

        /// <summary>
        /// Articles in the Featured Panorama Item
        /// </summary>
        public static MainPageFeaturedViewModel _featuredArticles;


        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Settings.InitialLaunchSetting)
            {
                App.Favorites = new Category { CategoryTitle = AppResources.FavoritesCategoryTitleText };
                App.DataBaseUtility.AddCategory(App.Favorites);
                App.DataBaseUtility.SaveChangesToDB();

                // App has initialized for the first time.
                Settings.InitialLaunchSetting = false;
            }

            if (e.NavigationMode == NavigationMode.New)
            {
                ApplicationBar appBar = new ApplicationBar
                {
                    Mode = ApplicationBarMode.Minimized,
                };

                ApplicationBarMenuItem refreshMenu = new ApplicationBarMenuItem(AppResources.MainPageAppBarRefreshText);

                refreshMenu.Click += OnRefreshClick;

                appBar.MenuItems.Add(refreshMenu);

                this.ApplicationBar = appBar;
            }

            _allCategories = new MainPageAllCategoriesViewModel();

            _allFeeds = new MainPageAllFeedsViewModel();

            _whatsNewArticles = new MainPageWhatsNewViewModel();

            _featuredArticles = new MainPageFeaturedViewModel();


            // Loading categories in the database to the "All" panoramaitem's categories listbox.
            AllCategoryItems.DataContext = _allCategories;

            // Loading feeds in the database to the "All" panoramaitem's feeds longlistselector.
            AllFeedItems.DataContext = _allFeeds;

            // Loading articles in the database to the "What's new" panoramaitem's article listbox.
            WhatsNewArticleItems.DataContext = _whatsNewArticles;


            // Loading featured tiles
            FeaturedArticleItems.DataContext = new MainPageFeaturedViewModel();

            HubTile1.Tap += OnHubTileTapped;
            HubTile2.Tap += OnHubTileTapped;
            HubTile3.Tap += OnHubTileTapped;
            HubTile4.Tap += OnHubTileTapped;
            HubTile5.Tap += OnHubTileTapped;
            HubTile6.Tap += OnHubTileTapped;

            // Loading settings
            SettingsPanel.DataContext = App.ApplicationSettings;
        }

        /// <summary>
        /// Called when navigating away from this page.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            // Disable the loading bar.
            if (LoadingProgressBar.IsIndeterminate)
            {
                LoadingProgressBar.IsIndeterminate = false;
            }
        }

        /// <summary>
        /// Called when the user navigates between panoramas; changes the appbar to show different possibilities per panorama.
        /// </summary>
        private void OnPanoramaSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((Panorama)sender).SelectedIndex)
            {
                // What's New PanoramaItem
                case 0:
                    HubTileService.FreezeGroup("featured");
                    break;
                // Featured PanoramaItem
                case 1:
                    ApplicationBar.Mode = ApplicationBarMode.Minimized;
                    ApplicationBar.Buttons.Clear();
                    HubTileService.UnfreezeGroup("featured");
                    break;
                // All Categories/Feeds PanoramaItem
                case 2:
                    ApplicationBar.Mode = ApplicationBarMode.Default;
                    ApplicationBarIconButton addButton = new ApplicationBarIconButton
                        {
                            Text = AppResources.MainPageAppBarAddText,
                            IconUri = new Uri("/Icons/appbar.add.rest.png", UriKind.Relative)
                        };
                    addButton.Click += OnAddClick;
                    ApplicationBar.Buttons.Add(addButton);
                    HubTileService.FreezeGroup("featured");
                    break;
                // Settings PanoramaItem
                case 3:
                    ApplicationBar.Mode = ApplicationBarMode.Minimized;
                    ApplicationBar.Buttons.Clear();
                    HubTileService.FreezeGroup("featured");
                    break;
            }
        }

        /// <summary>
        /// Called when the user clicks the refresh appbar menu item; refreshes the application's feeds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRefreshClick(object sender, EventArgs e)
        {
            _whatsNewArticles.LoadContent(() =>
                    {
                        // Stop showing progress bar.
                        LoadingProgressBar.IsIndeterminate = false;
                    });
            if (!LoadingProgressBar.IsIndeterminate)
            {
                LoadingProgressBar.IsIndeterminate = true;
            }
        }

        /// <summary>
        /// Called when the user clicks add; Navigates to AddMenu.
        /// </summary>
        private void OnAddClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Add", UriKind.Relative));
        }

        /// <summary>
        /// User has tapped on an article, navigate to article page and show the tapped article.
        /// </summary>
        private void OnArticleTap(object sender, EventArgs e)
        {
            Article tappedArticle = GetTagAs<Article>(sender);
            // note: We recommend using the sender DataContext instead of GetTagAs. 
            // See the article for more information: http://go.microsoft.com/fwlink/?LinkId=247592 
            
            if (null != tappedArticle)
            {
                if (!Convert.ToBoolean(tappedArticle.Read))
                {
                    tappedArticle.Read = true;
                    Feed feed = App.DataBaseUtility.QueryFeed(Convert.ToInt32(tappedArticle.FeedID));
                    if (null != feed)
                    {
                        feed.UnreadCount--;
                        feed.ViewCount++;
                    }
                    App.DataBaseUtility.SaveChangesToDB();
                }
                NavigationService.Navigate(new Uri("/Article/" + tappedArticle.ArticleID, UriKind.Relative));
            }
        }

        /// <summary>
        /// Called when the user selects remove from the contextmenu.
        /// </summary>
        private void OnRemoveArticleClick(object sender, RoutedEventArgs e)
        {
            Article selectedArticle = GetTagAs<Article>(sender);
            // note: We recommend using the sender DataContext instead of GetTagAs. 
            // See the article for more information: http://go.microsoft.com/fwlink/?LinkId=247592

            if (null != selectedArticle)
            {

                if (!Convert.ToBoolean(selectedArticle.Read))
                {
                    Feed feed = App.DataBaseUtility.QueryFeed(Convert.ToInt32(selectedArticle.FeedID));
                    feed.UnreadCount--;
                    App.DataBaseUtility.SaveChangesToDB();
                }

                // Delete the selected article from the database and remove it from view.
                App.DataBaseUtility.DeleteArticle(selectedArticle);
                _whatsNewArticles.Remove(selectedArticle);
            }

            // Reload featured articles in case one of them was deleted.
            _featuredArticles.RedoFeatured();

        }

        /// <summary>
        /// Called when the user selects remove on a category context menu.
        /// </summary>
        private void OnCategoryRemoved(object sender, RoutedEventArgs e)
        {
            Category selectedCategory = GetTagAs<Category>(sender);
            // note: We recommend using the sender DataContext instead of GetTagAs. 
            // See the article for more information: http://go.microsoft.com/fwlink/?LinkId=247592

            if (null != selectedCategory && 1 != selectedCategory.CategoryID)
            {
                // Delete the selected article from the database and remove it from view.
                App.DataBaseUtility.DeleteCategory(selectedCategory);
                _allCategories.Remove(selectedCategory);
            }
            // Reload featured articles in case one of them was deleted.
            _featuredArticles.RedoFeatured();

        }

        /// <summary>
        /// Called when the user selects remove on a feed context menu.
        /// </summary>
        private void OnFeedRemoved(object sender, RoutedEventArgs e)
        {
            Feed selectedFeed = GetTagAs<Feed>(sender);
            // note: We recommend using the sender DataContext instead of GetTagAs. 
            // See the article for more information: http://go.microsoft.com/fwlink/?LinkId=247592

            if (null != selectedFeed)
            {
                // Delete the selected article from the database.
                App.DataBaseUtility.DeleteFeed(selectedFeed);
                _allFeeds.RemoveFeed(selectedFeed);
            }
            // Reload featured articles in case one of them was deleted.
            _featuredArticles.RedoFeatured();

        }

        /// <summary>
        /// User has tapped on a category, navigate to categorypage and show the tapped category with it's corresponding articles.
        /// </summary>
        private void OnCategoryTap(object sender, EventArgs e)
        {
            Category tappedCategory = GetTagAs<Category>(sender);
            // note: We recommend using the sender DataContext instead of GetTagAs. 
            // See the article for more information: http://go.microsoft.com/fwlink/?LinkId=247592

            if (null != tappedCategory)
            {
                NavigationService.Navigate(new Uri("/Category/" + tappedCategory.CategoryID, UriKind.Relative));
            }
        }

        /// <summary>
        /// User has tapped on a feed, navigate to feedpage and show the tapped feed with it's corresponding articles.
        /// </summary>
        private void OnFeedTap(object sender, EventArgs e)
        {
            Feed tappedFeed = GetTagAs<Feed>(sender);
            // note: We recommend using the sender DataContext instead of GetTagAs. 
            // See the article for more information: http://go.microsoft.com/fwlink/?LinkId=247592

            if (null != tappedFeed)
            {
                NavigationService.Navigate(new Uri("/Feed/" + tappedFeed.FeedID, UriKind.Relative));
            }
        }

        /// <summary>
        /// Called when the user reaches the bottom of the list; load more articles.
        /// </summary>
        private void OnBottomReached(object sender, EventArgs e)
        {
            if (this._whatsNewArticles.Count != 0)
            {
                List<Article> articles = App.DataBaseUtility.NextWhatsNewCollection(_whatsNewArticles.Count);
                if (null != articles)
                {
                    foreach (Article a in articles)
                    {
                        _whatsNewArticles.Add(a);
                    }
                }
            }
        }

        /// <summary>
        /// Method called when a HubTile is tapped.
        /// </summary>
        private void OnHubTileTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Article tappedArticle = GetTagAs<Article>(sender);
            // note: In this case, the sender DataContext is bound to MainPageFeaturedViewModel 
            // rather than an Article. GetTagAs helps distinguish which Article was tapped.
            // See the article for more information: http://go.microsoft.com/fwlink/?LinkId=247592

            if (null != tappedArticle)
            {
                NavigationService.Navigate(new Uri("/Article/" + tappedArticle.ArticleID + "/true", UriKind.Relative));
            }
        }

        /// <summary>
        /// Handler called when a user selects the pin to start context menu option on a category.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event arguments for this event.</param>
        private void OnCategoryPinned(object sender, EventArgs e)
        {
            Category tappedCategory = GetTagAs<Category>(sender);
            // note: We recommend using the sender DataContext instead of GetTagAs. 
            // See the article for more information: http://go.microsoft.com/fwlink/?LinkId=247592

            if (null != tappedCategory)
            {
                AddTile.AddLiveTile(tappedCategory);
            }

        }

        /// <summary>
        /// Handler called when a user selects the pin to start context menu option on a feed.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event arguments for this event.</param>
        private void OnFeedPinned(object sender, EventArgs e)
        {
            Feed tappedFeed = GetTagAs<Feed>(sender);
            // note: We recommend using the sender DataContext instead of GetTagAs. 
            // See the article for more information: http://go.microsoft.com/fwlink/?LinkId=247592

            if (null != tappedFeed)
            {
                AddTile.AddLiveTile(tappedFeed);
            }
        }

        /// <summary>
        /// Gets the tag property of the given container, given that the container is a FrameWorkElement.
        /// </summary>
        /// <typeparam name="T">The type to return the tag property as.</typeparam>
        /// <param name="container">The FramworkElement (as an object) to get the tag from.</param>
        /// <returns></returns>
        public T GetTagAs<T>(object container) where T : class
        {
            if (null != container)
            {
                FrameworkElement element = container as FrameworkElement;
                if (null != element)
                {
                    T tag = element.Tag as T;
                    if (null != tag)
                    {
                        return tag;
                    }
                }
            }
            return null;
        }
    }
}