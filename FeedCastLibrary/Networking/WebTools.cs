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
using System.Net;
using System.Windows;
using System.Xml;
using Microsoft.Phone.Net.NetworkInformation;
using System.Threading;

namespace FeedCastLibrary
{
    /// <summary>
    /// Handles downloading of feeds and search results.
    /// </summary>
    public sealed class WebTools
    {
        /// <summary>
        /// Dictionary that stores all the downloaded feeds to a list of their respective articles.
        /// </summary>
        public IDictionary<Feed, ICollection<Article>> Downloads { get; private set; }

        /// <summary>
        /// Dictionary storing all standing web requests.
        /// </summary>
        private IDictionary<Feed, WebRequest> _allRequests;

        /// <summary>
        /// Parser used to parse SyndicationItems into Articles.
        /// </summary>
        private IXmlFeedParser _parser;

        /// <summary>
        /// Timer used to count down until web requests are timed out.
        /// </summary>
        private Timer _timeout;

        /// <summary>
        /// Object used to lock an action.
        /// </summary>
        private object _lockObject;

        /// <summary>
        /// Number of requests remaining to return.
        /// </summary>
        private static int _numOfRequests;

        /// <summary>
        /// Whether the application has been set to use only wifi connections.
        /// </summary>
        public bool WifiOnly
        {
            get
            {
                return Settings.WifionlySetting;
            }
            set
            {
                Settings.WifionlySetting = value;
            }
        }

        /// <summary>
        /// Specifies if this WebTools Object is currently downloading.
        /// This could have changed by the time you checked.
        /// </summary>
        public bool IsDownloading { get; set; }


        /// <summary>
        /// Property for the DateTime of the last time Downloading was finished.
        /// </summary>
        public static readonly DependencyProperty LastDownloadProperty =
            DependencyProperty.Register(
            "LastDownload",
            typeof(DateTime),
            typeof(WebTools),
            new PropertyMetadata(null));


        /// <summary>
        /// Event raised after all feed downloads have finished.
        /// </summary>
        public event EventHandler<AllDownloadsFinishedEventArgs> AllDownloadsFinished;

        /// <summary>
        /// Event raised after each feed download has finished.
        /// </summary>
        public event EventHandler<SingleDownloadFinishedEventArgs> SingleDownloadFinished;

        public WebTools(IXmlFeedParser parser)
        {
            if (null != parser)
            {
                _parser = parser;
            }
            else
            {
                throw new ArgumentNullException("No parser");
            }

            // Set all fields and properties.
            _lockObject = new object();
            _allRequests = new Dictionary<Feed, WebRequest>();
            Downloads = new Dictionary<Feed, ICollection<Article>>();
        }

        /// <summary>
        /// Downloads the provided Feed.
        /// You cannot call download while downloads are in progress.
        /// </summary>
        /// <param name="feed">feed to download.</param>
        public void Download(Feed feed)
        {
            if (null != feed)
            {
                Download(new List<Feed> { feed });
            }
        }

        /// <summary>
        /// Downloads the provided List of Feeds.
        /// You cannot call download while downloads are in progress.
        /// </summary>
        /// <param name="feeds">List of feeds to download.</param>
        public void Download(IList<Feed> feeds)
        {
            // Clear any leftover stored articles/feeds in Downloads and _allRequests.
            Downloads.Clear();
            _allRequests.Clear();

            // Set the timer for timeouts if parsing syndication feeds.
            if (_parser is SynFeedParser)
            {
                int timeLimit;
                if (Settings.InitialLaunchSetting)
                {
                    timeLimit = 20000; // 20 second limit for initial launch.
                }
                else
                {
                    timeLimit = 7500; // Otherwise, 7.5 second time limit.
                }
                TimerCallback tc = new TimerCallback(TimeoutConnections);
                _timeout = new Timer(tc, this, timeLimit, Timeout.Infinite);
            }

            if (null != feeds && feeds.Count > 0)
            {
                lock (_lockObject)
                {
                    _numOfRequests += feeds.Count;
                }
                IsDownloading = true;

                // Download each separate feed.
                foreach (Feed feed in feeds)
                {
                    string feedURI = feed.FeedBaseURI;
                    if (null != feedURI)
                    {
                        HttpWebRequest feedRequest = HttpWebRequest.Create(feedURI) as HttpWebRequest;

                        // Check if the application has been set to wifi-only or not.
                        if (WifiOnly)
                        {
                            feedRequest.SetNetworkRequirement(NetworkSelectionCharacteristics.NonCellular);
                        }
                        if (null != feedRequest)
                        {
                            RequestState feedState = new RequestState()
                            {
                                // Change the owner to the parent HTTPWebRequest.
                                Request = feedRequest,
                                // Change the argument to be the current feed to be downloaded.
                                Argument = feed,
                            };

                            // Begin download.
                            feedRequest.BeginGetResponse(ResponseCallback, feedState);

                            // Keep track of the download.
                            _allRequests.Add(feed, feedRequest);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Callback method called when the "Download" method returns from an HTTPWebRequest.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        private void ResponseCallback(IAsyncResult result)
        {
            RequestState state = result.AsyncState as RequestState;
            Feed parentFeed = state.Argument as Feed;

            if (null != parentFeed)
            {
                HttpWebRequest request = state.Request as HttpWebRequest;

                // Progress only if this download has not been timed out.

                if (null != request)
                {
                    // Retrieve response.
                    try
                    {
                        using (HttpWebResponse response = request.EndGetResponse(result) as HttpWebResponse)
                        {
                            if (null != response && response.StatusCode == HttpStatusCode.OK)
                            {
                                using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                                {
                                    // Collection to store all articles
                                    Collection<Article> parsedArticles = _parser.ParseItems(reader, parentFeed);

                                    // Raise event for a single feed downloaded.
                                    if (null != SingleDownloadFinished)
                                    {
                                        SingleDownloadFinished(this, new SingleDownloadFinishedEventArgs(parentFeed, parsedArticles));
                                    }

                                    // Add to all downloads dictionary and raise AllDownloadsFinished if all async requests have finished.
                                    Downloads.Add(parentFeed, parsedArticles);
                                    CheckIfDone();
                                }
                            }
                        }
                    }
                    catch (WebException we)
                    {
                        if (we.Status == WebExceptionStatus.RequestCanceled)
                        {
                            // The web request was timed out. Let debug know it failed.
                            System.Diagnostics.Debug.WriteLine("ERROR: Feed \"" + parentFeed.FeedTitle + "\" was timed out.");
                            CheckIfDone();
                            return;
                        }
                        else if (we.Message == "The remote server returned an error: NotFound.")
                        {
                            // The web site did not respond. This means one of two things: Either the web site is bad, or you have no internet.
                            // If error code NotFound is received, then there is no connection. Throw the exception.
                            if ((we.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                            {
                                // WebTools throws an exception if without connection. Warn the user.
                                CheckIfDone();
                                if (!IsDownloading)
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        MessageBox.Show("FeedCast is unable to reach a connection. Please check your network connectivity.");
                                    });
                                }
                            }
                            // If error code InternalServerError is received, then it is a faulty site.
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("ERROR: Feed \"" + parentFeed.FeedTitle + "\" is faulty.");
                                CheckIfDone();
                                return;
                            }
                        }
                        else
                        {
                            // Unexpected web exception.
                            throw we;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Decrements the number of remaining downloads and checks to see if all downloads are done.
        /// </summary>
        /// <returns>If all downloads are finished</returns>
        private void CheckIfDone()
        {
            lock (_lockObject)
            {
                IsDownloading = ((--_numOfRequests) > 0);
                if (_numOfRequests <= 0)
                {
                    if (null != AllDownloadsFinished)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            // Dispose of the timer if using one.
                            if (_parser is SynFeedParser)
                            {
                                _timeout.Dispose();
                            }
                            AllDownloadsFinished(this, new AllDownloadsFinishedEventArgs(Downloads));
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Aborts any web requests that are taking a suspiciously long amount of time.
        /// </summary>
        /// <param name="allRequests">Object containing the current state</param>
        private void TimeoutConnections(object state)
        {
            if (IsDownloading)
            {
                System.Diagnostics.Debug.WriteLine("TIMEOUT BEGINNING.");

                foreach (Feed feed in _allRequests.Keys)
                {
                    // If the feed has not yet been downloaded, then cancel its web request.
                    if (!Downloads.ContainsKey(feed))
                    {
                        _allRequests[feed].Abort();
                    }
                }

                System.Diagnostics.Debug.WriteLine("TIMEOUT ENDING.");
            }
        }
    }

    /// <summary>
    /// Class that passes data across asynchronous calls.
    /// </summary>
    public class RequestState
    {
        /// <summary>
        /// The owner of this request state.
        /// </summary>
        public WebRequest Request { get; set; }

        /// <summary>
        /// The argument for this request.
        /// </summary>
        public object Argument { get; set; }

        /// <summary>
        /// Creates a new, empty request state with default porperty values.
        /// Request = null
        /// Argument = null
        /// AddToDatabase = null
        /// AddToUI = null
        /// </summary>
        public RequestState()
        {
            this.Request = null;
            this.Argument = null;
        }

        /// <summary>
        /// Creates an instance of the webrequest class with given parameters.
        /// </summary>
        /// <param name="argument">The argument for this request.</param>
        /// <param name="addToDatabase">Boolean denoting whether to add the argument to the database.</param>
        /// <param name="addToUI">Boolean denoting whether to add the argument to the UI.</param>
        public RequestState(object argument)
        {
            this.Argument = argument;
        }

        /// <summary>
        /// Creates an instance of the webrequest class with given parameters.
        /// </summary>
        /// <param name="owner">The owner of this request state.</param>
        /// <param name="argument">The argument for this request.</param>
        /// <param name="addToDatabase">Boolean denoting whether to add the argument to the database.</param>
        /// <param name="addToUI">Boolean denoting whether to add the argument to the UI.</param>
        public RequestState(WebRequest owner, object argument)
            : this(argument)
        {
            this.Request = owner;
        }
    }

    /// <summary>
    /// Event arguments passed when all Feed downloads have finished.
    /// </summary>
    public class AllDownloadsFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// A dictionary of Downloaded feeds to a list of its respective downloaded articles.
        /// </summary>
        public IDictionary<Feed, ICollection<Article>> Downloads { get; set; }

        /// <summary>
        /// Creates a new instance of DownloadFinishedEventArgs which
        /// provides arguments relevant to the download performed.
        /// </summary>
        public AllDownloadsFinishedEventArgs() : this(null) { }

        /// <summary>
        /// Creates a new instance of DownloadFinishedEventArgs which
        /// provides arguments relevant to the download performed.
        /// </summary>
        /// <param name="downloads">A dictionary of Downloaded feeds 
        /// to a list of its respective downloaded articles.</param>
        public AllDownloadsFinishedEventArgs(IDictionary<Feed, ICollection<Article>> downloads)
            : base()
        {
            this.Downloads = downloads;
        }
    }

    /// <summary>
    /// Event argument passed when a single Feed download has finished.
    /// Warning, this event is raised many times (once per feed downloaded).
    /// </summary>
    public class SingleDownloadFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// The feed whose articles were downloaded.
        /// </summary>
        public Feed ParentFeed { get; set; }

        /// <summary>
        /// Articles corresponding to ParentFeed that were downloaded.
        /// </summary>
        public IList<Article> DownloadedArticles { get; set; }

        /// <summary>
        /// Creates a new instance of SingleDownloadFinishedEventArgs which
        /// provides arguments relevant to the download performed.
        /// </summary>
        public SingleDownloadFinishedEventArgs() : this(null, null) { }

        /// <summary>
        /// Creates a new instance of SingleDownloadFinishedEventArgs which
        /// provides arguments relevant to the download performed.
        /// </summary>
        /// <param name="downloadedFeeds">The feed whose articles were downloaded.</param>
        /// <param name="downloadedArticles">Articles corresponding to ParentFeed that were downloaded.</param>
        public SingleDownloadFinishedEventArgs(Feed parentFeed, IList<Article> downloadedArticles)
        {
            this.ParentFeed = parentFeed;
            this.DownloadedArticles = downloadedArticles;
        }
    }
}


