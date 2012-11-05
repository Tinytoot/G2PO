/* 
    Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see the MSDN article about this app, visit http://go.microsoft.com/fwlink/?LinkId=247592 
  
*/
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System;
using System.Collections.Generic;
using FeedCastLibrary;
using System.Net;

namespace FeedCast.ViewModels
{
    public class NewFeedPageViewModel : ObservableCollection<Article>
    {
        /// <summary>
        /// The number of results to display.
        /// </summary>
        private int _numOfResults;

        /// <summary>
        /// Object to moderate threaded access.
        /// </summary>
        private readonly object _lockObject;

        /// <summary>
        /// The WebTools object used to download the articles of a feed.
        /// </summary>
        private WebTools _feedSearch { get; set; }

        /// <summary>
        /// Returns whether FeedSearch is currently downloading results.
        /// </summary>
        public bool IsDownloading
        {
            get
            {
                if (null != _feedSearch)
                {
                    return _feedSearch.IsDownloading;
                }
                return false;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public NewFeedPageViewModel()
        {
            _numOfResults = 10;
            _lockObject = new object();
            _feedSearch = new WebTools(new SearchResultParser());
            Clear();
        }

        private string GetSearchString(string query)
        {
            // Format the search string.
            string search = "http://api.bing.com/rss.aspx?query=feed:" + query +
                "&source=web&web.count=" + _numOfResults.ToString() + "&web.filetype=feed&market=en-us";
            return search;
        }

        /// <summary>
        /// Performs a search and adds the results to the observable collection.
        /// </summary>
        /// <param name="query">The query to search by</param>
        public void GetResults(string query, Action<int> Callback)
        {
            // Clear the page ViewModel.
            Clear();

            // Get the search string and put it into a feed. 
            Feed feed = new Feed { FeedBaseURI = GetSearchString(query) };

            // Lambda expression to add results to page VM after the download completes.
            // _feedSearch is a WebTools object.
            _feedSearch.AllDownloadsFinished +=
                (sender, e) =>
                {
                    // See if the search returned any results.
                    if (e.Downloads.Count > 0)
                    {
                        // Add the search results to the page ViewModel.
                        foreach (Collection<Article> result in e.Downloads.Values)
                        {
                            if (null != result)
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    foreach (Article a in result)
                                    {
                                        lock (_lockObject)
                                        {
                                            // Add to the page VM.
                                            Add(a);
                                        }
                                    }
                                    Callback(Count);
                                });
                            }
                        }
                    }
                    else 
                    {   // If no search results were returned.
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Callback(0);
                            });
                    }
                };

            // Initiate the download (a Bing search).
            _feedSearch.Download(feed);
        }


        /// <summary>
        /// Turns SearchResults into Feeds and adds them to the database.
        /// </summary>
        /// <param name="category">The category the feed(s) will be added to</param>
        /// <param name="results">The collection of SearchResults</param>
        public void ResultToFeed(Category category, Collection<Article> results)
        {
            List<Feed> feedsAdded = new List<Feed>();
            foreach (Article result in results)
            {
                Feed feed = new Feed
                {
                    FeedTitle = result.ArticleTitle,
                    FeedBaseURI = result.ArticleBaseURI
                };
                if (App.DataBaseUtility.AddFeed(feed, category))
                {
                    feedsAdded.Add(feed);
                }
            }

            // Load the articles for the new feeds.
            ContentLoader loadContent = new ContentLoader();
            loadContent.DownloadFeeds(feedsAdded);
           // loadContent.LoadingFinished += MainPage._whatsNewArticles.DisplayFeeds;
        }
    }
}
