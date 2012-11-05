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
using System.Windows.Input;
using System.Windows.Navigation;
using FeedCastLibrary;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FeedCast.Resources;
using G2PO.Resources;

namespace FeedCast.Views
{
    public partial class NewCategoryPage : PhoneApplicationPage
    {
        /// <summary>
        /// Application bar save button
        /// </summary>
        private ApplicationBarIconButton _saveButton;

        public NewCategoryPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when the application is navigated to.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Setting up applicationbar for proper loading and localization.
            this.ApplicationBar = new ApplicationBar();
            _saveButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Icons/appbar.save.rest.png", UriKind.Relative),
                Text = AppResources.NewCategoryAppBarSaveText,
                IsEnabled = false
            };
            _saveButton.Click += OnSaveClick;
            this.ApplicationBar.Buttons.Add(_saveButton);

            // Setting DataContext.
            FeedPicker.DataContext = App.DataBaseUtility.GetAllFeeds();
        }

        /// <summary>
        /// User has clicked save. Category is added to database along with it's associated feeds.
        /// </summary>
        private void OnSaveClick(object sender, EventArgs e)
        {
            string text = CategoryNameTextBox.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                Category newCat = new Category() { CategoryTitle = text };
                App.DataBaseUtility.AddCategory(newCat);

                App.DataBaseUtility.SaveChangesToDB();

                foreach (Feed f in FeedPicker.SelectedItems)
                {
                    App.DataBaseUtility.AddFeed(f, newCat);
                }

                App.DataBaseUtility.SaveChangesToDB();

                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
        }

        /// <summary>
        /// Check if the user presses enter to save the category.
        /// </summary>
        private void OnTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
            {
                _saveButton.IsEnabled = true;
                if (null != e && e.Key == Key.Enter)
                {
                    OnSaveClick(sender, e);
                }
            }
            else
            {
                _saveButton.IsEnabled = false;
            }
        }
    }
}