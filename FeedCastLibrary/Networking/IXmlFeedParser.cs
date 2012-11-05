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
    public interface IXmlFeedParser
    {
        /// <summary>
        /// Parses the provided XmlReader into a collection of Articles,
        /// taking into account properties from parentFeed.
        /// </summary>
        /// <param name="reader">The XmlReader to use to parse the article items</param>
        /// <param name="ownerFeed">The feed owner of the articles to parse</param>
        /// <returns></returns>
        Collection<Article> ParseItems(XmlReader reader, Feed ownerFeed);
    }
}
