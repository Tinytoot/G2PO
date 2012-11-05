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
using System.Linq;
using FeedCastLibrary;

namespace FeedCast.Models
{
    public static class FeaturedAlgorithm
    {
        /// <summary>
        /// Determines which articles will be placed into the featured section. Updates the stored featured articles.
        /// </summary>
        public static void UpdateFeatured()
        {
            Collection<Article> articles = new Collection<Article>();
            int favConst = 10;
            int shareConst = 30;

            // Query the database for feeds with unread articles.
            List<Feed> unreadFeeds = App.DataBaseUtility.QueryForUnreadFeeds();

            int[] thresholds = new int[unreadFeeds.Count()];

            // Calculate the threshold value for each of the feeds. Top threshold feeds are chosen.
            for (int i = 0; i < unreadFeeds.Count(); i++)
            {
                // c.Skip(i).Take(1).Single() is essentially the equivalent of c.ElementAt(i).
                // c.ElementAt(i) is not an allowed operation for QueriableItem c.
                Feed feed = unreadFeeds[i];

                // Give threshold a minimum value of 1 to allow it to work well with MaxIndex().
                thresholds[i] = 1 + feed.ViewCount.Value + (feed.FavoritedCount.Value * favConst) +
                    (feed.SharedCount.Value * shareConst);
            }

            // Take the maximum 6 threshold indexes, which will be used to obtain the articles.
            int[] maxThresIndexes = new int[6];
            int articleCount = 0;   // 6 articles are necessary for the Featured section.
            int numOfFeeds = 0;     // Number of feeds it takes to obtain 6 articles.
            while (numOfFeeds < 6)
            {
                maxThresIndexes[numOfFeeds] = MaxIndex(thresholds);

                // In case there are no more unread feeds:
                if (maxThresIndexes[numOfFeeds] == -1)
                {
                    break;
                }

                articleCount += unreadFeeds[maxThresIndexes[numOfFeeds]].UnreadCount.Value;
                numOfFeeds++;
            }

            if (articleCount >= 6)
            {
                // There are enough new articles; update.
                // Article getting process.
                int index = 0;
                while (articles.Count < 6)
                {
                    if (index >= numOfFeeds)
                    {
                        index = 0;
                    }

                    // Query for the newest article of the chosen feed.
                    List<Article> sortedFeed = App.DataBaseUtility.QueryForSortedFeed(unreadFeeds, maxThresIndexes, index);

                    // Add the article, and temporarily change it to read so it won't be added again.
                    // Check to ensure that the query was successful, and unread articles do exist.
                    if (sortedFeed.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        Article newestArticle = sortedFeed[0];
                        articles.Add(newestArticle);
                        newestArticle.Read = true;
                        App.DataBaseUtility.SaveChangesToDB();
                    }

                    index++;
                }
            }

            // Return the read values in the articles to unread.
            ReturnArticlesToUnread(articles);

            // Update the featured articles if successful (as in, 6 articles have been added).
            if (articles.Count == 6)
            {
                Settings.FeaturedArticles = articles;
            }
            return;
        }

        /// <summary>
        /// Takes the index of the maximum int value in the array.
        /// </summary>
        /// <param name="numbers">The array of numbers to be looked at</param>
        /// <returns>The index of the maximum int value in the array</returns>
        private static int MaxIndex(int[] numbers)
        {
            // Empty case.
            if (numbers.Length == 0)
            {
                return -1;
            }
            int max = 0;
            for (int i = 0; i < numbers.Length; i++)
            {
                if (numbers[i] > numbers[max])
                {
                    max = i;
                }
            }

            // Check for the no-max case.
            if (numbers[max] == -1)
            {
                return -1;
            }
            else
            {
                numbers[max] = -1;
                return max;
            }
        }

        /// <summary>
        /// Return the iterated-over articles to unread.
        /// </summary>
        /// <param name="articles">The collection of articles to be changed</param>
        private static void ReturnArticlesToUnread(Collection<Article> articles)
        {
            for (int i = 0; i < articles.Count; i++)
            {
                articles[i].Read = false;
            }
            App.DataBaseUtility.SaveChangesToDB();
        }
    }
}
