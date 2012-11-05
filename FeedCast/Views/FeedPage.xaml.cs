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
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Navigation;
using FeedCastLibrary;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FeedCast.Resources;
using G2PO.Resources;

namespace FeedCast.Views
{
    /// <summary>
    /// Page that shows the user all articles within a selected feed.
    /// </summary>
    public partial class FeedPage : PhoneApplicationPage
    {
        /// <summary>
        /// 
        /// </summary>
        private ApplicationBarIconButton _previousButton;

        /// <summary>
        /// 
        /// </summary>
        private ApplicationBarIconButton _nextButton;

        /// <summary>
        /// 
        /// </summary>
        private List<Feed> _allFeeds;

        /// <summary>
        /// 
        /// </summary>
        private List<Article> _allFeedsArticles;

        /// <summary>
        /// The feed selected by the user whose articles to display.
        /// </summary>
        public Feed SelectedFeed
        {
            get { return (Feed)GetValue(SelectedFeedProperty); }
            set { SetValue(SelectedFeedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedFeed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedFeedProperty =
            DependencyProperty.Register(
            "SelectedFeed",
            typeof(Feed),
            typeof(FeedPage),
            new PropertyMetadata(null));

        /// <summary>
        /// The articles belonging to the feed selected by the user.
        /// </summary>
        public ObservableCollection<Article> FeedArticles { get; private set; }

        public FeedPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Retrieve the feed selected by the user and all it's corresponding articles.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string idStr;
            if (NavigationContext.QueryString.TryGetValue("id", out idStr))
            {
                SelectedFeed = App.DataBaseUtility.QueryFeed(Convert.ToInt32(idStr));
                _allFeedsArticles = App.DataBaseUtility.GetFeedArticles(Convert.ToInt32(idStr));
                List<Article> firstSetOfArticles = new List<Article>(_allFeedsArticles);
                if (firstSetOfArticles.Count > 10)
                {
                    firstSetOfArticles.RemoveRange(10, firstSetOfArticles.Count - 10);
                }

                FeedArticles = new ObservableCollection<Article>(firstSetOfArticles);
                DataContext = FeedArticles;
            }

            _allFeeds = App.DataBaseUtility.GetAllFeeds();
            _allFeeds.Sort();
            ApplicationBar appBar = new ApplicationBar { IsMenuEnabled = false };

            int index = _allFeeds.IndexOf(SelectedFeed);

            _previousButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Icons/appbar.back.rest.png", UriKind.Relative),
                Text = AppResources.AppBarButtonPreviousText,
                IsEnabled = (index > 0)
            };

            _previousButton.Click += OnPreviousClick;

            _nextButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Icons/appbar.next.rest.png", UriKind.Relative),
                Text = AppResources.AppBarButtonNextText,
                IsEnabled = (index < _allFeeds.Count - 1)
            };

            _nextButton.Click += OnNextClick;

            appBar.Buttons.Add(_previousButton);
            appBar.Buttons.Add(_nextButton);

            this.ApplicationBar = appBar;
        }

        /// <summary>
        /// User has tapped on an article, navigate to article page and show the tapped article.
        /// </summary>
        private void OnArticleTap(object sender, EventArgs e)
        {
            FrameworkElement container = sender as FrameworkElement;
            if (null != container && null != container.Tag)
            {
                Article tappedArticle = container.Tag as Article;
                if (null != tappedArticle)
                {
                    if (!Convert.ToBoolean(tappedArticle.Read))
                    {
                        tappedArticle.Read = true;
                        App.DataBaseUtility.QueryFeed(Convert.ToInt32(tappedArticle.FeedID)).UnreadCount--;
                        App.DataBaseUtility.QueryFeed(Convert.ToInt32(tappedArticle.FeedID)).ViewCount++;
                        App.DataBaseUtility.SaveChangesToDB();
                    }
                    NavigationService.Navigate(new Uri("/Article/" + tappedArticle.ArticleID + "/Feed", UriKind.Relative));
                }
            }
        }

        /// <summary>
        /// Remove the specified article.
        /// </summary>
        private void OnRemoveClick(object sender, EventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            if (null != sender)
            {
                Article removed = menu.Tag as Article;
                if (null != removed)
                {
                    FeedArticles.Remove(removed);
                    // TODO check if article is removed in the database.
                    App.DataBaseUtility.DeleteArticle(removed);
                }
            }
        }

        /// <summary>
        /// Display the previous feed.
        /// </summary>
        private void OnPreviousClick(object sender, EventArgs e)
        {
            if (null != _allFeeds && _allFeeds.Count > 0)
            {
                int prevFeedIndex = _allFeeds.IndexOf(SelectedFeed) - 1;
                if (prevFeedIndex >= 0)
                {
                    Feed previousFeed = _allFeeds[prevFeedIndex];
                    List<Article> feedArticles = App.DataBaseUtility.GetFeedArticles(previousFeed.FeedID);
                    if (null != feedArticles)
                    {
                        this.SelectedFeed = previousFeed;
                        this.DataContext = feedArticles;
                        _nextButton.IsEnabled = true;
                        _previousButton.IsEnabled = (prevFeedIndex > 0);
                    }
                }
            }
        }

        /// <summary>
        /// Display the next feed.
        /// </summary>
        private void OnNextClick(object sender, EventArgs e)
        {
            if (null != _allFeeds && _allFeeds.Count > 0)
            {
                int nextFeedIndex = _allFeeds.IndexOf(SelectedFeed) + 1;
                if (nextFeedIndex < _allFeeds.Count)
                {
                    Feed previousFeed = _allFeeds[nextFeedIndex];
                    List<Article> feedArticles = App.DataBaseUtility.GetFeedArticles(previousFeed.FeedID);
                    if (null != feedArticles)
                    {
                        this.SelectedFeed = previousFeed;
                        this.DataContext = feedArticles;
                        _previousButton.IsEnabled = true;
                        _nextButton.IsEnabled = (nextFeedIndex < _allFeeds.Count - 1);
                    }
                }
            }
        }

        private void OnBottomReached(object sender, EventArgs e)
        {
            if (FeedArticles.Count != 0 && _allFeedsArticles.Count > FeedArticles.Count)
            {
                List<Article> nextSetOfArticles = new List<Article>(_allFeedsArticles);
                if (null != nextSetOfArticles)
                {
                    Array set;
                    if (FeedArticles.Count + 10 < nextSetOfArticles.Count)
                    {
                        set = nextSetOfArticles.GetRange(FeedArticles.Count, 10).ToArray();
                    }
                    else
                    {
                        set = nextSetOfArticles.GetRange(FeedArticles.Count, nextSetOfArticles.Count - FeedArticles.Count).ToArray();
                    }

                    foreach (Article a in set)
                    {
                        FeedArticles.Add(a);
                    }
                }
            }
        }

    }
}