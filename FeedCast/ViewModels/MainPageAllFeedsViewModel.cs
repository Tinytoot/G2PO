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
using FeedCast.Models;
using FeedCastLibrary;

namespace FeedCast.ViewModels
{
    public class MainPageAllFeedsViewModel : ObservableCollection<FeedsInGroup>
    {
        /// <summary>
        /// All the groups for the long list selector.
        /// </summary>
        private static readonly string Groups = "#abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// A dictionary of group letter to feeds in such group.
        /// </summary>
        private Dictionary<string, FeedsInGroup> _groupings;

        public MainPageAllFeedsViewModel()
        {
            _groupings = new Dictionary<string, FeedsInGroup>();

            foreach (char c in Groups)
            {
                FeedsInGroup group = new FeedsInGroup(c.ToString());
                this.Add(group);
                _groupings[c.ToString()] = group;
            }

            List<Feed> feeds = App.DataBaseUtility.GetAllFeeds();
            foreach (Feed f in feeds)
            {
                AddFeed(f);
            }
        }

        public void AddFeed(Feed feed)
        {
            _groupings[feed.GetNameKey()].Add(feed);
        }

        public void RemoveFeed(Feed feed)
        {
            _groupings[feed.GetNameKey()].Remove(feed);
        }

        public bool ContainsFeed(Feed feed)
        {
            return _groupings[feed.GetNameKey()].Contains(feed);
        }
    }
}
