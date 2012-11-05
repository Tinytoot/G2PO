// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using FeedCast.Models;
using System.Collections.ObjectModel;

namespace FeedCast.ViewModels
{
    public static class MainPageAllFeedsViewModel : ObservableCollection<FeedsInCategory>
    {
        private static readonly string Groups = "#abcdefghijklmnopqrstuvwxyz";

        public MainPageAllFeedsViewModel()
        {
            //Sorted list of feeds
            List<Feed> feedList = new List<Feed>()
            {
                new Feed { FeedTitle = "CNN Money" },
                new Feed { FeedTitle = "Fox Business" },
                new Feed { FeedTitle = "Huffington Post Health" },
                new Feed { FeedTitle = "Huffington Post Entertainment" },
                new Feed { FeedTitle = "Hollywood Reporter" },
                new Feed { FeedTitle = "Us Magazine Style" },
                new Feed { FeedTitle = "Style.com" },
                new Feed { FeedTitle = "MTV" },
                new Feed { FeedTitle = "VH1" },
                new Feed { FeedTitle = "BBC" },
                new Feed { FeedTitle = "CNN" },
                new Feed { FeedTitle = "Reuters" },
                new Feed { FeedTitle = "Engadget" },
                new Feed { FeedTitle = "Wired" },
                new Feed { FeedTitle = "NY Times" },
                new Feed { FeedTitle = "ESPN" }
            };

            Dictionary<string, FeedsInCategory> groups = new Dictionary<string, FeedsInCategory>();

            foreach (char c in Groups)
            {
                FeedsInCategory group = new FeedsInCategory(c.ToString());
                this.Add(group);
                groups[c.ToString()] = group;
            }

            foreach (Feed f in feedList)
            {
                string s = App.DataBaseUtility.GetFeedNameKey(f);
                groups[s].Add(f);
            }
        }
    }
}
