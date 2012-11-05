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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using FeedCast.Resources;
using FeedCast.ViewModels;
using FeedCastLibrary;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using G2PO.Resources;

namespace FeedCast.Views
{
    /// <summary>
    /// Page a user navigates to when they would like to add a new feed to their reader.
    /// </summary>
    public partial class NewFeedPage : PhoneApplicationPage
    {
        /// <summary>
        /// Collection of search results that gets populated once the user performs a search.
        /// </summary>
        private NewFeedPageViewModel _search;

        public NewFeedPage()
        {
            InitializeComponent();

            _search = new NewFeedPageViewModel();
        }

        /// <summary>
        /// User has navigated to this page from the AddMenu; they have decided to add a new feed.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Loading application bar form code behind for propert timing and localization.
            this.ApplicationBar = new ApplicationBar();
            ApplicationBarIconButton saveButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Icons/appbar.save.rest.png", UriKind.Relative),
                Text = AppResources.NewCategoryAppBarSaveText,
                IsEnabled = false
            };
            saveButton.Click += OnSaveClick;
            this.ApplicationBar.Buttons.Add(saveButton);

            ApplicationBarIconButton searchButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Icons/appbar.feature.search.rest.png", UriKind.Relative),
                Text = AppResources.NewFeedAppBarSearchText
            };
            searchButton.Click += OnSearchClick;
            this.ApplicationBar.Buttons.Add(searchButton);

            // Setting page datacontext.
            SearchResults.DataContext = _search;
            List<Category> categories = App.DataBaseUtility.GetAllCategories();
            categories.Remove(App.Favorites);
            CategoryPicker.DataContext = categories;

            string idStr;
            if (NavigationContext.QueryString.TryGetValue("selectedCatID", out idStr))
            {
                int catID = Convert.ToInt32(idStr);
                Category selectedCat = App.DataBaseUtility.QueryCategory(catID);
                if (null != selectedCat && CategoryPicker.Items.Contains(selectedCat))
                {
                    CategoryPicker.SelectedItem = selectedCat;
                }
            }
        }

        /// <summary>
        /// User clicks the search button. Search results are populated and the user
        /// is provided with a checkbox appbar button to finalize their selection.
        /// </summary>
        private void OnSearchClick(object sender, EventArgs e)
        {
            // Check to make sure the user had not initiated a download.
            if (!_search.IsDownloading)
            {
                // Dismiss focus on the keyboard (hide keyboard).
                this.Focus();

                // Search for the user's query.
                string query = SearchBox.Text;
                if (!string.IsNullOrWhiteSpace(query))
                {
                    // Remove the no results text if present.
                    NoResultsText.Visibility = Visibility.Collapsed;

                    // Show progress bar.
                    LoadingProgressBar.IsIndeterminate = true;

                    // Search for results.
                    _search.GetResults(query, (count) =>
                    {
                        // Stop showing progress bar.
                        LoadingProgressBar.IsIndeterminate = false;

                        // If there are no search results show NoResultsText, else hide it.
                        NoResultsText.Visibility = (count <= 0) ? Visibility.Visible : Visibility.Collapsed;
                    });
                }
                else
                {
                    // User has entered an invalid query.
                    NoResultsText.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Check if the user intended to begin the search by hitting the enter key.
        /// </summary>
        private void OnTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            // Check if the user pressed the enter key to begin the search.
            if (null != e && e.Key == Key.Enter)
            {
                OnSearchClick(sender, e);
            }
        }

        /// <summary>
        /// User has selected a category.
        /// </summary>
        private void OnSaveClick(object sender, EventArgs e)
        {
            IList selectedFeeds = SearchResults.SelectedItems;
            if (null != selectedFeeds && selectedFeeds.Count > 0)
            {
                Category selectedCategory = CategoryPicker.SelectedItem as Category;
                if (null != selectedCategory)
                {
                    Collection<Article> selectedItems = new Collection<Article>();

                    foreach (Article a in SearchResults.SelectedItems)
                    {
                        selectedItems.Add(a);
                    }

                    if (null != selectedItems)
                    {
                        _search.ResultToFeed(selectedCategory, new Collection<Article>(selectedItems));
                    }

                    if (NavigationService.CanGoBack)
                    {
                        NavigationService.GoBack();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSearchResultsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationBarIconButton saveButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;

            if (e.AddedItems.Count > 0)
            {
                if (null != saveButton)
                {
                    saveButton.IsEnabled = true;
                }
            }
            else
            {
                saveButton.IsEnabled = false;
            }
        }
    }
}