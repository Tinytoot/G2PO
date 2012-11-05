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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using FeedCast.ViewModels;
using FeedCastLibrary;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using G2PO.Resources;

namespace FeedCast.Views
{
    public partial class ArticlePage : PhoneApplicationPage
    {
        /// <summary>
        /// How much time (in ms.) the title takes to expand when tapped.
        /// </summary>
        private static readonly double ExpansionAnimationDuration = 200D;

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
        private List<Article> _allArticles;

        /// <summary>
        /// The article this page reflects.
        /// </summary>
        public Article PageArticle { get; private set; }

        #region TitleMaxHeight Dependency Property
        /// <summary>
        /// The default maximum amount of title content to show.
        /// Defaults to 130.
        /// </summary>
        public double TitleMaxHeight
        {
            get { return (double)GetValue(TitleMaxHeightProperty); }
            set { SetValue(TitleMaxHeightProperty, value); }
        }

        /// <summary>
        /// The dependency property for the default maximum amount of title content to show.
        /// Defaults to 130.
        /// </summary>
        public static readonly DependencyProperty TitleMaxHeightProperty =
            DependencyProperty.Register(
            "MaxHeight",
            typeof(double),
            typeof(ArticlePage),
            new PropertyMetadata(130D));
        #endregion

        /// <summary>
        /// Whether the title is currently expanded.
        /// </summary>
        private bool _titleExpanded;

        public ArticlePage()
        {
            InitializeComponent();

            _titleExpanded = false;
        }

        /// <summary>
        /// Called when this page is navigated to; retrieve the article to show.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Retrieve the article ID to retrive and populate the page with such article.
            string articleIDStr;
            string catIDStr = "";
            if (NavigationContext.QueryString.TryGetValue("id", out articleIDStr))
            {
                // Filter the article string.
                string articleSource = "";
                if (articleIDStr.Contains("/"))
                {
                    // Check to see if category id is contained. If so, user came from Category view.
                    if (articleIDStr.Contains("/Category/"))
                    {
                        articleSource = "category";
                        catIDStr = articleIDStr.Substring(articleIDStr.LastIndexOf("/") + 1);
                    }

                    // Check to see if feed id is contained. If so, user came from Feed view.
                    else if (articleIDStr.Contains("/Feed"))
                    {
                        articleSource = "feed";
                    }
                    // Check to see if true is contained. If so, user came from Featured.
                    else if (articleIDStr.Contains("true"))
                    {
                        articleSource = "featured";
                    }

                    articleIDStr = articleIDStr.Remove(articleIDStr.IndexOf("/"));

                }

                PageArticle = App.DataBaseUtility.QueryArticle(int.Parse(articleIDStr));

                if (articleSource == "featured")
                {
                        _allArticles = new List<Article>(Settings.FeaturedArticles);
                }
                else if (articleSource == "feed")
                {
                    _allArticles = new List<Article>(App.DataBaseUtility.GetFeedArticles(Convert.ToInt32(PageArticle.FeedID)));
                }
                else if (articleSource == "category")
                {
                    _allArticles = new List<Article>(App.DataBaseUtility.GetCategoryArticles(Convert.ToInt32(catIDStr)));
                }
                else
                {
                    _allArticles = new List<Article>(App.DataBaseUtility.InitialWhatsNewCollection());
                }

                this.DataContext = PageArticle;
            }

            ApplicationBar appBar = new ApplicationBar();

            int index = _allArticles.IndexOf(PageArticle);

            _previousButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Icons/appbar.back.rest.png", UriKind.Relative),
                Text = AppResources.AppBarButtonPreviousText,
                IsEnabled = (index > 0)
            };

            _previousButton.Click += OnPreviousClick;

            ApplicationBarIconButton browserButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Icons/appbar.gotoaddressbar.rest.png", UriKind.Relative),
                Text = AppResources.AppBarButtonOpenInBrowserText,
            };

            browserButton.Click += OnBrowserClick;

            _nextButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Icons/appbar.next.rest.png", UriKind.Relative),
                Text = AppResources.AppBarButtonNextText,
                IsEnabled = (index < _allArticles.Count - 1)
            };

            _nextButton.Click += OnNextClick;

            ApplicationBarMenuItem shareMenuItem = new ApplicationBarMenuItem(AppResources.ShareText);

            shareMenuItem.Click += OnShareClick;

            ApplicationBarMenuItem delMenuItem = new ApplicationBarMenuItem(AppResources.ContextMenuRemoveText);

            delMenuItem.Click += OnDeleteClick;

            ApplicationBarMenuItem favMenuItem = new ApplicationBarMenuItem(AppResources.AddToFavoritesText);

            favMenuItem.Click += OnFavoriteClick;

            appBar.Buttons.Add(_previousButton);
            appBar.Buttons.Add(browserButton);
            appBar.Buttons.Add(_nextButton);
            appBar.MenuItems.Add(shareMenuItem);
            appBar.MenuItems.Add(delMenuItem);
            appBar.MenuItems.Add(favMenuItem);

            this.ApplicationBar = appBar;
        }

        /// <summary>
        /// User has clicked browser; launch the browser to this article's corresponding adress.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBrowserClick(object sender, EventArgs e)
        {
            if (null != PageArticle)
            {
                ShareUtility.LaunchBrowser(PageArticle.ArticleBaseURI);
            }
        }

        /// <summary>
        /// User has clicked previous; retrieve the previous article to show the user.
        /// </summary>
        private void OnPreviousClick(object sender, EventArgs e)
        {
            if (null != _allArticles && _allArticles.Count > 0)
            {
                int index = _allArticles.IndexOf(PageArticle) - 1;
                if (index >= 0)
                {
                    PageArticle = _allArticles[index];
                    if (!Convert.ToBoolean(PageArticle.Read))
                    {
                        PageArticle.Read = true;
                        Feed feed = App.DataBaseUtility.QueryFeed(Convert.ToInt32(PageArticle.FeedID));
                        feed.UnreadCount--;
                        feed.ViewCount++;
                        App.DataBaseUtility.SaveChangesToDB();
                    }

                    this.DataContext = PageArticle;
                    _nextButton.IsEnabled = true;
                    _previousButton.IsEnabled = (index > 0);
                }
            }
        }

        /// <summary>
        /// User has clicked on next; retrieve the next article to show the user.
        /// </summary>
        private void OnNextClick(object sender, EventArgs e)
        {
            if (null != _allArticles && _allArticles.Count > 0)
            {
                int index = _allArticles.IndexOf(PageArticle) + 1;
                if (index < _allArticles.Count)
                {
                    PageArticle = _allArticles[index];
                    if (!Convert.ToBoolean(PageArticle.Read))
                    {
                        PageArticle.Read = true;
                        Feed feed = App.DataBaseUtility.QueryFeed(Convert.ToInt32(PageArticle.FeedID));
                        feed.UnreadCount--;
                        feed.ViewCount++;
                        App.DataBaseUtility.SaveChangesToDB();
                    }

                    this.DataContext = PageArticle;
                    _previousButton.IsEnabled = true;
                    _nextButton.IsEnabled = (index < _allArticles.Count - 1);
                }
            }
        }

        /// <summary>
        /// User has clicked on share; navigate to share options.
        /// </summary>
        private void OnShareClick(object sender, EventArgs e)
        {
            if (null != PageArticle)
            {
                App.DataBaseUtility.QueryFeed(Convert.ToInt32(PageArticle.FeedID)).SharedCount++;
                NavigationService.Navigate(new Uri("/Share/" + PageArticle.ArticleBaseURI, UriKind.Relative));
            }
        }

        /// <summary>
        /// User has clicked on delete; Article is deleted from the Database and UI.
        /// </summary>
        private void OnDeleteClick(object sender, EventArgs e)
        {
            // Determine the deleted article/index
            int deletedIndex = _allArticles.IndexOf(PageArticle);
            Article deletedArticle = PageArticle;

            if (!Convert.ToBoolean(deletedArticle.Read))
            {
                deletedArticle.Read = true;
                Feed feed = App.DataBaseUtility.QueryFeed(Convert.ToInt32(deletedArticle.FeedID));
                feed.UnreadCount--;
                App.DataBaseUtility.SaveChangesToDB();
            }


            // Remove it from the database/ui
            App.DataBaseUtility.DeleteArticle(deletedArticle);
            _allArticles.Remove(deletedArticle);
            MainPage._featuredArticles.RedoFeatured();


            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }

            //// Determine where to move.
            //Article movedArticle = _allArticles[deletedIndex];

            //// Add the moved article to the ui.
            //PageArticle = movedArticle;
            //PageArticle.Read = true;
            //App.DataBaseUtility.SaveChangesToDB();
            //this.DataContext = PageArticle;

            //// Determine trhe status of the buttons.
            //_nextButton.IsEnabled = (deletedIndex < _allArticles.Count - 1);
            //_previousButton.IsEnabled = (deletedIndex > 0);
        }

        /// <summary>
        /// User has clicked on Add to Favorites; article is added to favorites.
        /// </summary>
        private void OnFavoriteClick(object sender, EventArgs e)
        {
            PageArticle.Favorite = true;
            App.DataBaseUtility.AddCat_Feed(Convert.ToInt32(PageArticle.FeedID), 1);
            App.DataBaseUtility.SaveChangesToDB();
        }

        /// <summary>
        /// Animate the title to show the rest of it's content.
        /// </summary>
        private void OnTitleTap(object sender, EventArgs e)
        {
            TextBlock title = sender as TextBlock;
            if (null != title && (_titleExpanded || (title.ActualHeight > title.MaxHeight)))
            {
                Duration duration = new Duration(TimeSpan.FromMilliseconds(ExpansionAnimationDuration));

                DoubleAnimation heightAnim = new DoubleAnimation()
                {
                    To = _titleExpanded ? TitleMaxHeight : title.ActualHeight,
                    Duration = duration
                };

                Storyboard sb = new Storyboard() { Duration = duration };
                Storyboard.SetTargetProperty(heightAnim, new PropertyPath(TextBlock.MaxHeightProperty));
                Storyboard.SetTarget(heightAnim, title);
                sb.Children.Add(heightAnim);
                sb.Begin();

                _titleExpanded = !_titleExpanded;
            }
        }
    }
}