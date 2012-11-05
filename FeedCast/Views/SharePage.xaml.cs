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
using FeedCast.ViewModels;
using Microsoft.Phone.Controls;

namespace RSS_Reader_Mockup
{
    public partial class SharePage : PhoneApplicationPage
    {
        private string URL;

        public SharePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Retrieve the url to send.
            if (!NavigationContext.QueryString.TryGetValue("url", out URL))
                throw new ArgumentException("Could not get 'URL' query string.");
        }

        private void OnMessagingTap(object sender, EventArgs e)
        {
            if(null != URL)
                ShareUtility.ShareSMS(URL);
        }

        private void OnEmailTap(object sender, EventArgs e)
        {
            if(null != URL)
                ShareUtility.ShareEmail(URL);
        }

        private void OnSocialTap(object sender, EventArgs e)
        {
            if(null != URL)
                ShareUtility.ShareLink(URL);
        }
    }
}