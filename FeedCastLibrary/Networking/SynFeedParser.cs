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
using System.Collections.ObjectModel;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace FeedCastLibrary
{
    public class SynFeedParser : IXmlFeedParser
    {
        /// <summary>
        /// Moderates thread operations.
        /// </summary>
        private static object _lockObject = new object();

        /// <summary>
        /// Maintains the date of the latest article.
        /// </summary>
        public static DateTime latestDate;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SynFeedParser() { }

        /// <summary>
        /// Remove all HTTP tags and trim extra whitespace.
        /// </summary>
        /// <param name="text">The string from which HTML will be removed</param>
        /// <returns>The HTML-removed string</returns>
        private static string HTMLParser(string text)
        {
            string parsedText = Regex.Replace(text, @"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>",
                    string.Empty);
            parsedText = Regex.Replace(parsedText, "<!--.*-->", string.Empty); 
            parsedText = HttpUtility.HtmlDecode(parsedText);
            parsedText = parsedText.Trim();
            return parsedText;
        }

        /// <summary>
        /// Function to parse the item of a feed and obtain useful data.
        /// </summary>
        /// <param name="feed">The Feed the article being parsed belongs to.</param>
        /// <param name="synFeed">Syndication Feed to be parsed</param>
        /// <param name="item">SyndicationItem that will be added to the database</param>
        public Collection<Article> ParseItems(XmlReader reader, Feed parentFeed)
        {
            // Collection to store all parsed articles
            Collection<Article> parsedArticles = new Collection<Article>();
            SyndicationFeed synFeed;

            // Create a syndicationFeed from the provided reader
            try
            {
                synFeed = SyndicationFeed.Load(reader);
            }
            catch
            {
                return null;
            }
            if (null != synFeed)
            {
                // First, obtain an image for the feed if it does not have one.
                if (!ImageGrabber.IfImageExists(parentFeed))
                {
                    ImageGrabber.GetImage(parentFeed, synFeed);
                    latestDate = new DateTime();
                }

                // Parse the xml file, getting each article.
                foreach (SyndicationItem item in synFeed.Items)
                {
                    // Get the necessary details from the SyndicationItem.
                    string title = (item.Title != null) ? item.Title.Text : String.Empty;
                    title = HTMLParser(title); // parse the title for html leftovers
                    string itemURI = (item.Links[0].Uri.ToString() != null) ? item.Links[0].Uri.ToString() : String.Empty;
                    DateTimeOffset date = item.PublishDate;

                    // Special case for the text/preview of article.
                    StringBuilder text = new StringBuilder();
                    if (item.Summary != null)
                    {
                        text.Append(item.Summary.Text);
                    }
                    // If the article instead writes to the content:encoded portion of the xml.
                    else
                    {
                        foreach (SyndicationElementExtension extension in item.ElementExtensions)
                        {
                            XElement ele = extension.GetObject<XElement>();
                            if (ele.Name.LocalName == "encoded"
                                && ele.Name.Namespace.ToString().Contains("content"))
                            {
                                text.Append(ele.Value + "<br/>");
                            }
                        }
                    }
                    string parsedText = HTMLParser(text.ToString());
                    if (parsedText.Length >= 3500)
                    {
                        parsedText = parsedText.Substring(0, 2499);
                    }

                    if (null == latestDate)
                    {
                        lock (_lockObject)
                        {
                            latestDate = new DateTime();
                        }
                    }

                    bool val;
                    lock (_lockObject)
                    {
                        val = latestDate.CompareTo(date.DateTime) < 0;
                    }
                    if (val)
                    {
                        // Initialize new article, then add it to the database.
                        Article newArticle = new Article
                        {
                            ArticleTitle = title,
                            Authors = parentFeed.FeedTitle,
                            PublishDate = date.DateTime.ToLocalTime(),
                            Summary = parsedText,
                            LastUpdatedTime = DateTime.Now,
                            ArticleBaseURI = itemURI,
                            FeedID = parentFeed.FeedID,
                            ImageURL = parentFeed.ImageURL
                        };

                        parsedArticles.Add(newArticle);
                    }
                }
            }
            return parsedArticles;
        }
    }
}
