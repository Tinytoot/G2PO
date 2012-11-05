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
using System.Xml;

namespace FeedCastLibrary
{
    public class SearchResultParser : IXmlFeedParser
    {
        /// <summary>
        /// Object to moderate thread access to collection adding.
        /// </summary>
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Default constructor
        /// </summary>
        public SearchResultParser() { }

        /// <summary>
        /// Parse the results of the Bing search
        /// </summary>
        /// <param name="reader">The xml reader containing the search results</param>
        public Collection<Article> ParseItems(XmlReader reader, Feed ownerFeed)
        {
            Collection<Article> results = new Collection<Article>();
            reader.ReadToFollowing("item");
            do
            {
                if (reader.ReadToFollowing("title"))
                {
                    string name = reader.ReadElementContentAsString();

                    if (reader.ReadToFollowing("link"))
                    {
                        string uri = reader.ReadElementContentAsString();

                        // Assign feed information to Article object.
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
            return results;
        }
    }
}
