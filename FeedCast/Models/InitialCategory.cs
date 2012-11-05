/* 
    Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see the MSDN article about this app, visit http://go.microsoft.com/fwlink/?LinkId=247592 
  
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Threading;
using FeedCastLibrary;

namespace FeedCast.Models
{
    /// <summary>
    /// Represents a category in the launch page used to initially populate the feed reader.
    /// </summary>
    public class InitialCategory : Category
    {
        /// <summary>
        /// A comma-separated string of the initial feeds to be loaded with this InitialCategory.
        /// </summary>
        public string AssociatedFeeds { get; private set; }

        /// <summary>
        /// A list of the feeds to be loaded with this InitialCategory.
        /// </summary>
        public IList<Feed> Feeds { get; private set; }

        /// <summary>
        /// Constructs an InitialCategory with the provided parameters.
        /// </summary>
        /// <param name="title">The title of this InitialCategory.</param>
        /// <param name="feeds">One or more feeds to load when this InitialCategory is loaded.</param>
        public InitialCategory(string title, params Feed[] feeds) : base()
        {
            CategoryTitle = title;
            Feeds = new List<Feed>(feeds);

            // Building up AssociatedFeeds.
            StringBuilder sb = new StringBuilder();
            string separator = ", ";
            foreach (Feed f in feeds)
            {
                sb.Append(f.FeedTitle).Append(separator);
            }
            sb.Length -= separator.Length;
            AssociatedFeeds = sb.ToString();
        }
    }
}
