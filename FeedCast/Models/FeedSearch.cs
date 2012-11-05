using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Xml;
using FeedCastLibrary;

namespace FeedCast.Models
{
    public class FeedSearch
    {
        /// <summary>
        /// Number of results to show.
        /// </summary>
        private static readonly int _numOfResults = 10; 

        /// <summary>
        /// The name of the feed to be searched for.
        /// </summary>
        private string _query;

        /// <summary>
        /// The uri for the search.
        /// </summary>
        private string _searchString;              
        public Collection<Article> results;
        public AutoResetEvent waitHandler;
        private Object _lockObject;
        public Action<int> Callback { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name to be searched for</param>
        public FeedSearch(string query, Action<int> cb)
        {
            this._query = query;
            this.Callback = cb;
            results = new Collection<Article>();
            waitHandler = new AutoResetEvent(false);
            _lockObject = new Object();

            // Format the search string.
            _searchString = "http://api.bing.com/rss.aspx?query=feed:" + query +
                "&source=web&web.count=" + _numOfResults.ToString() + "&web.filetype=feed&market=en-us";
        }

        /// <summary>
        /// Sends an asynchronous request to download Bing's search results.
        /// </summary>
        public void DownloadResults()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(_searchString));
                request.BeginGetResponse(ResponseHandler, request);
            }
            catch
            {
                // Error in sending a web request; exit.
                return;
            }
        }

        /// <summary>
        /// Takes xml files and takes the search results, then returns them.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ResponseHandler(IAsyncResult asyncResult)
        {
            HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResult))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {

                        // Use an xml reader to grab the xml, then parse it.
                        using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                        {
                            // Due to an error with SyndicationLoad and DateTime, we use a custom parser.
                            ParseResults(reader);
                        }

                        // Return the search results.
                        waitHandler.Set();
                    }
                }
            }
            catch (WebException) 
            {
                // Error in the web request; exit.
                return;
            }
        }

        /// <summary>
        /// Parse the results of the Bing search
        /// </summary>
        /// <param name="reader">The xml reader containing the search results</param>
        private void ParseResults(XmlReader reader)
        {
            reader.ReadToFollowing("item");
            do
            {
                if (reader.ReadToFollowing("title"))
                {
                    string name = reader.ReadElementContentAsString();

                    if (reader.ReadToFollowing("link"))
                    {
                        string uri = reader.ReadElementContentAsString();
                        Article newResult = new Article
                        {
                            ArticleTitle = name,
                            ArticleBaseURI = uri
                        };
                        // Safely add the search result to the collection.
                        lock (_lockObject)
                        {
                            results.Add(newResult);
                        }
                    }
                }
            } while (reader.ReadToFollowing("item"));
        }

        /// <summary>
        /// Returns the search results.
        /// </summary>
        /// <returns>The search results</returns>
        public Collection<Article> ReturnResults()
        {
            return results;
        }
    }
}
