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
using Microsoft.Phone.Tasks;

namespace FeedCast.ViewModels
{
    public static class ShareUtility
    {
        // The body for each message.
        private static readonly string _messageTitle = "Cool article";
        private static readonly string _messageBody = "Hey, check out this article: ";

        public static void ShareLink(string link)
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();
            shareLinkTask.Title = _messageTitle;
            shareLinkTask.Message = _messageBody + link;
            shareLinkTask.LinkUri = new Uri(link, UriKind.Absolute);
            shareLinkTask.Show();
        }

        public static void ShareSMS(string link)
        {
            SmsComposeTask smsComposeTask = new SmsComposeTask();
            smsComposeTask.Body = _messageBody + link;
            smsComposeTask.Show();
        }

        public static void ShareEmail(string link)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.Subject = _messageTitle;
            emailComposeTask.Body = _messageBody + link;
            emailComposeTask.Show();
        }

        public static void LaunchBrowser(string link)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(link, UriKind.Absolute);
            webBrowserTask.Show();
        }
    }
}
