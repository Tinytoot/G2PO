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
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace FeedCast.Views
{
    /// <summary>
    /// Intermediate page where the user decides if they want to add a new feed or category.
    /// </summary>
    public partial class AddMenu : PhoneApplicationPage
    {
        public AddMenu()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                NavigationService.RemoveBackEntry();
            }
        }

        /// <summary>
        /// Navigate to the NewFeed page, where the user can add new feeds.
        /// </summary>
        private void OnNewFeedTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewFeed", UriKind.Relative));
        }

        /// <summary>
        /// Navigate to the NewCategory page, where the user can add new categories.
        /// </summary>
        private void OnNewCategoryTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewCategory", UriKind.Relative));
        }
    }
}