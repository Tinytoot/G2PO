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
using G2PO.Resources;

namespace FeedCast.Views
{
    /// <summary>
    /// Page that shows the user all articles within a selected category.
    /// </summary>
    public partial class CategoryPage : PhoneApplicationPage
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
        private List<Category> _allCategories;

        /// <summary>
        /// 
        /// </summary>
        private List<Article> _allCategoryArticles;

        /// <summary>
        /// The category the user selected.
        /// </summary>
        public Category SelectedCategory
        {
            get { return (Category)GetValue(SelectedCategoryProperty); }
            set { SetValue(SelectedCategoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCategory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCategoryProperty =
            DependencyProperty.Register(
            "SelectedCategory",
            typeof(Category),
            typeof(CategoryPage),
            new PropertyMetadata(null));

        /// <summary>
        /// All the articles belonging to the category selected by the user.
        /// </summary>
        public ObservableCollection<Article> CategoryArticles { get; private set; }

        public CategoryPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Retrieve the category clicked by the user and it's corresponding articles.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string idStr;
            if (NavigationContext.QueryString.TryGetValue("id", out idStr))
            {
                SelectedCategory = App.DataBaseUtility.QueryCategory(Convert.ToInt32(idStr));
                _allCategoryArticles = App.DataBaseUtility.GetCategoryArticles(Convert.ToInt32(idStr));
                List<Article> firstSetOfArticles = new List<Article>(_allCategoryArticles);
                if (firstSetOfArticles.Count > 10)
                {
                    firstSetOfArticles.RemoveRange(10, firstSetOfArticles.Count - 10);
                }
                CategoryArticles = new ObservableCollection<Article>(firstSetOfArticles);
                DataContext = CategoryArticles;
            }

            _allCategories = App.DataBaseUtility.GetAllCategories();
            _allCategories.Sort();

            ApplicationBar appBar = new ApplicationBar();

            int index = _allCategories.IndexOf(SelectedCategory);

            _previousButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Icons/appbar.back.rest.png", UriKind.Relative),
                Text = AppResources.AppBarButtonPreviousText,
                IsEnabled = (index > 0)
            };

            _previousButton.Click += OnPreviousClick;

            ApplicationBarMenuItem addMenu = new ApplicationBarMenuItem(AppResources.AppBarMenuItemAddText);

            addMenu.Click += OnAddMenuClick;

            _nextButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Icons/appbar.next.rest.png", UriKind.Relative),
                Text = AppResources.AppBarButtonNextText,
                IsEnabled = (index < _allCategories.Count - 1)
            };

            _nextButton.Click += OnNextClick;

            appBar.Buttons.Add(_previousButton);
            appBar.Buttons.Add(_nextButton);
            appBar.MenuItems.Add(addMenu);

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
                    if(!Convert.ToBoolean(tappedArticle.Read))
                    {
                    tappedArticle.Read = true;
                    App.DataBaseUtility.QueryFeed(Convert.ToInt32(tappedArticle.FeedID)).UnreadCount--;
                    App.DataBaseUtility.QueryFeed(Convert.ToInt32(tappedArticle.FeedID)).ViewCount++;
                    App.DataBaseUtility.SaveChangesToDB();
                    }
                    NavigationService.Navigate(new Uri("/Article/" + tappedArticle.ArticleID + "/Category/" +
                        SelectedCategory.CategoryID, UriKind.Relative));
                }
            }
        }

        /// <summary>
        /// Remove the selected article.
        /// </summary>
        private void OnRemoveClick(object sender, EventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            if (null != sender)
            {
                Article removed = menu.Tag as Article;
                if (null != removed)
                {

                    if (!Convert.ToBoolean(removed.Read))
                    {
                        Feed feed = App.DataBaseUtility.QueryFeed(Convert.ToInt32(removed.FeedID));
                        feed.UnreadCount--;
                        App.DataBaseUtility.SaveChangesToDB();
                    }

                    CategoryArticles.Remove(removed);
                    // TODO check if article is removed in the database.
                    App.DataBaseUtility.DeleteArticle(removed);
                }
            }
        }

        /// <summary>
        /// Navigate to the previous category.
        /// </summary>
        private void OnPreviousClick(object sender, EventArgs e)
        {
            if (null != _allCategories && _allCategories.Count > 0)
            {
                int prevCatIndex = _allCategories.IndexOf(SelectedCategory) - 1;
                if (prevCatIndex >= 0)
                {
                    Category previousCategory = _allCategories[prevCatIndex];
                    List<Article> categoryArticles = App.DataBaseUtility.GetCategoryArticles(previousCategory.CategoryID);
                    if (null != categoryArticles)
                    {
                        this.SelectedCategory = previousCategory;
                        this.DataContext = categoryArticles;
                        _nextButton.IsEnabled = true;
                        _previousButton.IsEnabled = (prevCatIndex > 0);
                    }
                }
            }
        }

        /// <summary>
        /// Navigate to the next category.
        /// </summary>
        private void OnNextClick(object sender, EventArgs e)
        {
            if (null != _allCategories && _allCategories.Count > 0)
            {
                int nextCatIndex = _allCategories.IndexOf(SelectedCategory) + 1;
                if (nextCatIndex < _allCategories.Count)
                {
                    Category previousCategory = _allCategories[nextCatIndex];
                    List<Article> categoryArticles = App.DataBaseUtility.GetCategoryArticles(previousCategory.CategoryID);
                    if (null != categoryArticles)
                    {
                        this.SelectedCategory = previousCategory;
                        this.DataContext = categoryArticles;
                        _previousButton.IsEnabled = true;
                        _nextButton.IsEnabled = (nextCatIndex < _allCategories.Count - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Handler called when the user presses add in the application bar menu.
        /// </summary>
        public void OnAddMenuClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewFeed/" + SelectedCategory.CategoryID, UriKind.Relative));
        }

        private void OnBottomReached(object sender, EventArgs e)
        {
            if (CategoryArticles.Count != 0 && _allCategoryArticles.Count > CategoryArticles.Count)
            {
                List<Article> nextSetOfArticles = new List<Article>(_allCategoryArticles);
                if (null != nextSetOfArticles)
                {
                    Array set;
                    if (CategoryArticles.Count + 10 < nextSetOfArticles.Count)
                    {
                        set = nextSetOfArticles.GetRange(CategoryArticles.Count, 10).ToArray();
                    }
                    else
                    {
                        set = nextSetOfArticles.GetRange(CategoryArticles.Count, nextSetOfArticles.Count - CategoryArticles.Count).ToArray();
                    }
                    foreach (Article a in set)
                    {
                        CategoryArticles.Add(a);
                    }
                }
            }
        }

    }
}