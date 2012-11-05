/* 
    Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see the MSDN article about this app, visit http://go.microsoft.com/fwlink/?LinkId=247592 
  
*/
using FeedCastLibrary;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Net;

namespace FeedCast.ViewModels
{
    public class ContentLoader
    {
        private static WebTools Downloader = new WebTools(new SynFeedParser());
        private object _lock = new object();
        public Action _callback;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ContentLoader() { }

        /// <summary>
        /// Constructor with a callback action
        /// </summary>
        /// <param name="callback"></param>
        public ContentLoader(Action callback)
        {
            _callback = callback;
        }

        /// <summary>
        /// Event raised after all feed downloads have finished.
        /// </summary>
        public event EventHandler<LoadingFinishedEventArgs> LoadingFinished;

        public void DownloadFeeds(IList<Feed> feeds)
        {
            if (feeds.Count != 0)
            {
                if (!Downloader.IsDownloading)
                {
                    Downloader.AllDownloadsFinished += SaveToDB;
                    Downloader.Download(feeds);
                }
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _callback();
                    });
            }
        }

        private void SaveToDB(object sender, AllDownloadsFinishedEventArgs e)
        {
            // For every collection in the dictionary returned.
            foreach (Feed feed in e.Downloads.Keys)
            {
                // Make sure its not null!!
                if (null != feed)
                {
                    ICollection<Article> articles = e.Downloads[feed];
                    if (null != articles && articles.Count > 0)
                    {
                        // Add it to the database.
                        App.DataBaseUtility.AddArticles(articles, feed);
                    }
                }
            }

            App.DataBaseUtility.clearOldArticles();

            if (null != LoadingFinished)
            {
                Settings.LastUpdatedTime = DateTime.Now.ToString();
                LoadingFinished(this, new LoadingFinishedEventArgs());
            }
        }

        /// <summary>
        /// Event arguments passed when all Feed downloads have finished.
        /// </summary>
        public class LoadingFinishedEventArgs : EventArgs
        {
            /// <summary>
            /// Creates a new instance of LoadingFinishedEventArgs.
            /// </summary>
            public LoadingFinishedEventArgs()
                : base() { }

        }
    }
}
