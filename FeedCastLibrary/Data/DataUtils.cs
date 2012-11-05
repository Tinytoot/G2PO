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
using System.ComponentModel;
using System.Linq;
using System;
using System.Threading;
using System.Windows;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace FeedCastLibrary
{
    public class DataUtils : INotifyPropertyChanged
    {
        //Database!
        private LocalDatabaseDataContext db;


        private Mutex dbMutex = new Mutex(false, "DBControl");
        // note: could also have used a lock rather than a mutex.
        // A lock protects against cross-thread access to the data context.
        // The mutex additionally protects against cross-process access.

        //Contructor to initialize the database
        public DataUtils(string dbConnectionString)
        {
            db = new LocalDatabaseDataContext(dbConnectionString);
        }

        //Used when the database is modified.
        public void SaveChangesToDB()
        {
            dbMutex.WaitOne();

            try
            {
                // Attempt all updates.
                db.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
            catch (ChangeConflictException)
            {
                // For debugging.
                //System.Diagnostics.Debug.WriteLine("Optimistic concurrency error.");
                //System.Diagnostics.Debug.WriteLine(e.Message);
                //foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                //{
                //    MetaTable metatable = db.Mapping.GetTable(occ.Object.GetType());
                //    System.Diagnostics.Debug.WriteLine("Table name: {0}", metatable.TableName);
                //    foreach (MemberChangeConflict mcc in occ.MemberConflicts)
                //    {
                //        object currVal = mcc.CurrentValue;
                //        object origVal = mcc.OriginalValue;
                //        object databaseVal = mcc.DatabaseValue;

                //        System.Diagnostics.Debug.WriteLine("current value: {0}", currVal);
                //        System.Diagnostics.Debug.WriteLine("original value: {0}", origVal);
                //        System.Diagnostics.Debug.WriteLine("database value: {0}", databaseVal);
                //    }
                //}

                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                {
                    // Keep the value that has changed, update the other values with database values.
                    occ.Resolve(RefreshMode.KeepChanges);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("ERROR!!!: " + e);
                return;
            }
            // Minimize wait for other threads.
            finally { dbMutex.ReleaseMutex(); }
        }

        //Notify Silverlight to update the UI 
        // if the database changes
        #region INotifyProperyChangedMembers

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

       
        #region Featured
        /// <summary>
        /// Queries especially for Featured. 
        /// </summary>
        public List<Feed> QueryForUnreadFeeds()
        {
            dbMutex.WaitOne();

            // Query the database for feeds with unread articles.
            var c = from Feed q in db.Feed
                    where q.UnreadCount != 0
                    orderby q.ViewCount descending
                    select q;
            dbMutex.ReleaseMutex();
            return c.ToList();
        }

        public List<Article> QueryForSortedFeed(List<Feed> c, int[] maxThresIndexes, int index)
         {
             dbMutex.WaitOne();

            // Query for the newest article of the chosen feed.
            var a = from Article o in db.Article
                    where o.FeedID == c[maxThresIndexes[index]].FeedID
                    where o.Read == false
                    orderby o.PublishDate descending
                    select o;
            dbMutex.ReleaseMutex();
            return a.ToList();
        }
        #endregion



        //Check if the feed is there before adding
        public bool checkFeed(string feedURI, int catID)
        {
            bool result;
            List<Category_Feed> cat_feed = new List<Category_Feed>();
            var c = from q in db.Feed
                    where q.FeedBaseURI == feedURI
                    select q;

            foreach (Feed f in c)
            {
                var d = from q in db.Category_Feed
                        where q.CategoryID == catID
                        where q.FeedID == f.FeedID
                        select q;
                cat_feed = d.ToList();
            }
            result = (cat_feed.Count() == 0);
            return result;
        }

        //Check if the feed is there before adding
        public bool checkIfFeedExists(string feedURI)
        {
            List<Feed> feed = new List<Feed>();
            var c = from q in db.Feed
                    where q.FeedBaseURI == feedURI
                    select q;
            feed = c.ToList();
            return (feed.Count() == 0);
        }

        //Intial List of What's New articles, without a limiter
        public List<Article> InitialWhatsNewCollection()
        {
            dbMutex.WaitOne();

            List<Article> w = new List<Article>();
            var whatsnew = from Article q in db.Article
                           where (q.PublishDate).Value.CompareTo(DateTime.Now) < 0 
                           where q.PublishDate.Value.AddDays(3).CompareTo(DateTime.Now) > 0
                           orderby q.PublishDate descending
                           select q;
            w = whatsnew.ToList();
            if (w.Count != 0)
            {
                SynFeedParser.latestDate = w[0].PublishDate.Value;
            }
            dbMutex.ReleaseMutex();
            return w;
        }


        //Intial List of What's New articles
        public List<Article> WhatsNewCollection(int initialCount)
        {
            dbMutex.WaitOne();
            List<Article> w = new List<Article>();
            var whatsnew = from Article q in db.Article
                           where (q.PublishDate).Value.CompareTo(DateTime.Now) < 0
                           where q.PublishDate.Value.AddDays(3).CompareTo(DateTime.Now) > 0
                           orderby q.PublishDate descending
                           select q;
            w = whatsnew.ToList();
            if (w.Count != 0)
            {
                if (w.Count >= initialCount)
                {
                    w.RemoveRange(initialCount, w.Count() - initialCount);
                }
                SynFeedParser.latestDate = w[0].PublishDate.Value;
            }
            dbMutex.ReleaseMutex();
            return w;
        }

        //Articles that have been downloaded since the last refresh
        public List<Article> UpdateWhatsNewCollection()
        {
            dbMutex.WaitOne();
            List<Article> w = new List<Article>();
            var whatsnew = from Article q in db.Article
                           where q.PublishDate.Value.ToLocalTime().CompareTo(SynFeedParser.latestDate) > 0
                           orderby q.PublishDate ascending
                           select q;
            try
            {
                w = whatsnew.ToList();
                if (w.Count() != 0 && whatsnew != null)
                {
                    SynFeedParser.latestDate = w[w.Count-1].PublishDate.Value;
                }
            }
            catch 
            {
                return w;    
            }

            finally {dbMutex.ReleaseMutex();}
            return w;
        }

        //When the user hits the bottom, the application is supposed to load more articles
        public List<Article> NextWhatsNewCollection(int count)
        {
            dbMutex.WaitOne();

            List<Article> w = new List<Article>();
            List<Article> whatsNew = new List<Article>();
            var whatsnew = from Article q in db.Article
                           where q.PublishDate.Value.AddDays(3).CompareTo(DateTime.Now) > 0
                           orderby q.PublishDate descending
                           select q;

            w = whatsnew.ToList();
            if (w.Count > 0)
            {
                if (w.Count > count )
                {
                    if (w.Count - count > 10)
                    {
                        whatsNew = w.ToList().GetRange(count, 10).ToList();
                    }
                    else
                    {
                        whatsNew = w.ToList().GetRange(count, w.Count - count).ToList();
                    }

                }
                else
                {
                    whatsNew = null;
                }
            }
            dbMutex.ReleaseMutex();
            return whatsNew;
        }
        
        //Add a category to the category table
        public bool AddCategory(Category newCategory)
        {
            dbMutex.WaitOne();

            var c = from q in db.Category
                    where q.CategoryTitle == newCategory.CategoryTitle
                    select q;
            bool checkCategory = (c.Count() == 0);
            if (checkCategory)
            {
                newCategory.IsPinned = false;
                if (newCategory.CategoryID != 1) 
                {
                    newCategory.IsRemovable = true;
                }
                else
                {
                    newCategory.IsRemovable = false;
                }
                
                db.Category.InsertOnSubmit(newCategory);
                SaveChangesToDB();
            }

            dbMutex.ReleaseMutex();
            return checkCategory;
        }

        //Add a feed to the feed table, and a corresponding entry in Category_Feed Table
        public bool AddFeed(Feed newFeed, Category cat)
        {
            dbMutex.WaitOne();

            bool checkfeed = checkFeed(newFeed.FeedBaseURI, cat.CategoryID);
            bool feedexists = checkIfFeedExists(newFeed.FeedBaseURI);

            //Checks if the feed is in the database
            if (feedexists)
            {
                newFeed.SharedCount = 0;
                newFeed.UnreadCount = 0;
                newFeed.ViewCount = 0;
                newFeed.FavoritedCount = 0;
                newFeed.IsPinned = false;
                db.Feed.InsertOnSubmit(newFeed);
                SaveChangesToDB();
                Category_Feed newCat_Feed = new Category_Feed { CategoryID = cat.CategoryID, FeedID = newFeed.FeedID };
                db.Category_Feed.InsertOnSubmit(newCat_Feed);
                SaveChangesToDB();

            }
            //If it does, but not to the category, add it to the category
            else if (checkfeed)
            {
                Category_Feed newCat_Feed = new Category_Feed { CategoryID = cat.CategoryID, FeedID = newFeed.FeedID };
                db.Category_Feed.InsertOnSubmit(newCat_Feed);
                SaveChangesToDB();
            }
            //Else do nothing.
            dbMutex.ReleaseMutex();
            return (feedexists);
        }

        public void AddCat_Feed(int newFeed, int cat)
        {
            Category_Feed newCat_Feed = new Category_Feed { CategoryID = cat, FeedID = newFeed };
            db.Category_Feed.InsertOnSubmit(newCat_Feed);
            SaveChangesToDB();
        }       

        //Add an article to the article table!
        public void AddArticles(ICollection<Article> newArticles, Feed feed)
        {
            dbMutex.WaitOne();

            int downloadedArticleCount = newArticles.Count;
            int numOfNew = 0;

            // Query local database for existing articles.
            for (int i = 0; i < downloadedArticleCount; i++)
            {
                Article newArticle = newArticles.ElementAt(i);
                var d = from q in db.Article
                        where q.ArticleBaseURI == newArticle.ArticleBaseURI
                        select q;

                List<Article> a = d.ToList();

                //Determine if any articles are already in the database 
                bool alreadyInDB = (d.ToList().Count == 0);
                if (alreadyInDB)
                {
                    newArticle.Read = false;
                    newArticle.Favorite = false;
                    numOfNew++;
                }
                else
                {
                    //If so, remove them from the list
                    newArticles.Remove(newArticle);
                    downloadedArticleCount--;
                    i--;
                }
                
            }
            //Try to submit and update counts. 
            try
            {
                db.Article.InsertAllOnSubmit(newArticles);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        feed.UnreadCount += numOfNew;
                        SaveChangesToDB();
                    });
                SaveChangesToDB();
            }
            //TODO handle errors.
            catch { }
            finally { dbMutex.ReleaseMutex(); }
        }

        //Corner Case (if the feed doesnt belong to any other category)
        public bool checkDelete(int feed)
        {
            var d = from o in db.Category_Feed
                    where o.FeedID == feed
                    select o;
            List<Category_Feed> cat_feed = d.ToList();
            for (int i = 0; i < cat_feed.Count; i++ )
            {
                if (cat_feed[i].CategoryID == 1)
                {
                    cat_feed.RemoveAt(i);
                }
            }
            return (cat_feed.Count == 1);
        }

        //Delete a category.
        public void DeleteCategory(Category deleteCategory)
        {
            //If the category is not favorites
            if (deleteCategory.CategoryID != 1)
            {
                //Get all the pairings of the category
                var q = from o in db.Category_Feed
                        where o.CategoryID == deleteCategory.CategoryID
                        select o;
                //Foreach of the pairings
                foreach (Category_Feed deleteCat_Feed in q)
                {
                    //Check if the feed has any other association 
                    if (checkDelete(Convert.ToInt32(deleteCat_Feed.FeedID)))
                    {
                        //If not, delete it.
                        Feed feed = QueryFeed(Convert.ToInt32(deleteCat_Feed.FeedID));
                        DeleteFeed(feed);
                    }
                }

                //Remove the cat_feeds even if they are associated with Favorites Category. 
                List<Category_Feed> catfeed = q.ToList();
                int max = catfeed.Count;
                for (int i = 0; i < max; i++)
                {
                    if (catfeed[i].CategoryID == 1)
                    {
                        catfeed.RemoveAt(i);
                        max--;
                        i--;
                    }
                }
                db.Category_Feed.DeleteAllOnSubmit(catfeed);
                db.Category.DeleteOnSubmit(deleteCategory);
                SaveChangesToDB();
            }
        }

        //Delete a feed.
        public void DeleteFeed(Feed deleteFeed)
        {
            var q = from o in db.Category_Feed
                    where o.FeedID == deleteFeed.FeedID
                    select o;
            //Delete all the cat_feed pairings 
            db.Category_Feed.DeleteAllOnSubmit(q.ToList());
            var a = from o in db.Article
                    where o.FeedID == deleteFeed.FeedID
                    select o;
            //Delete all the articles in that feed
            DeleteArticles(a.ToList());
            db.Feed.DeleteOnSubmit(deleteFeed);
            SaveChangesToDB();
        }

        //Delete an article.
        public void DeleteArticle(Article deleteArticle)
        {
            db.Article.DeleteOnSubmit(deleteArticle);
            SaveChangesToDB();
        }

        //Delete articles
        public void DeleteArticles(List<Article> deleteArticles)
        {
            int max = deleteArticles.Count;
            for (int i = 0; i < max;i++ )
            {
                //If favorited remove it from the collection to be deleted
                if (Convert.ToBoolean(deleteArticles[i].Favorite))
                {
                    deleteArticles.RemoveAt(i);
                    max--;
                    i--;
                }
            }
            //Delete all the one not favorited. 
            db.Article.DeleteAllOnSubmit(deleteArticles);
            SaveChangesToDB();
        }

       
        //Query for a particular category
        public Category QueryCategory(int category)
        {
            Category result = null;
            var c = from Category q in db.Category
                    where q.CategoryID == category
                    select q;
            if (c.Count() > 0) { result = c.Take(1).Single(); }
            return result;
        }

        //Query for a particular feed
        public Feed QueryFeed(int feed)
        {
            Feed result = null;
            var c = from Feed q in db.Feed
                    where q.FeedID == feed
                    select q;
            if (c.Count() > 0) { result = c.Take(1).Single(); }
            return result;
        }

        //Query for a particular article
        public Article QueryArticle(int articleID)
        {
            Article result = null;
            var c = from Article q in db.Article
                    where q.ArticleID == articleID
                    select q;
            if (c.Count() > 0) { result = c.Take(1).Single(); }
            return result;
        }

     
        //Get all the Categories
        public List<Category> GetAllCategories()
        {
            dbMutex.WaitOne();
            List<Category> cat;
            var c = from q in db.Category
                    select q;
            cat = c.ToList();
            dbMutex.ReleaseMutex();
            return cat;
        }

        //Get all the Feeds
        public List<Feed> GetAllFeeds()
        {
            dbMutex.WaitOne();
            List<Feed> f;
            var c = from q in db.Feed
                    select q;
            f = c.ToList();
            dbMutex.ReleaseMutex();
            return f;
        }

        //Get all the Articles no filter
        public List<Article> GetAllArticles()
        {
            dbMutex.WaitOne();
            List<Article> a;
            var c = from q in db.Article
                    orderby q.PublishDate descending
                    select q;
            a = c.ToList();
            dbMutex.ReleaseMutex();
            return a;
        }

        //Get all the Articles filtered by feed
        public List<Article> GetFeedArticles(int feedID)
        {
            List<Article> a;
            var q = from Article o in db.Article
                    where o.FeedID == feedID
                    orderby o.PublishDate descending
                    select o;

            a = q.ToList();

            return a;
        }

        //Get all the feeds in a category (Used to remove off the UI)
        public List<Feed> GetFeeds(int catID)
        {
            List<Feed> feeds = new List<Feed>();
            var c = from o in db.Category_Feed
                    where o.CategoryID == catID
                    select o;
            foreach (Category_Feed cat in c.ToList())
            {
                if (checkDelete(Convert.ToInt32(cat.FeedID)))
                {
                    feeds.Add(QueryFeed(Convert.ToInt32(cat.FeedID)));
                }
            }
            return feeds;
        }

        //Get all the Articles filtered by Category
        public List<Article> GetCategoryArticles(int categoryID)
        {         
            List<Article> cat = new List<Article>();
            //Favorites?
             if (categoryID == 1)
             {
                 var o = from a in db.Article
                         where a.Favorite == true
                         select a;
                 cat.AddRange(o.ToList());
             }

             else
             {
                 //Get the corresponding Cat_Feeds with the category ID
                 var q = from o in db.Category_Feed
                         where o.CategoryID == categoryID
                         select o;
                 // Not Favorites!
                 List<Article> feedArticles;
                 foreach (Category_Feed f in q)
                 {
                     // For every Cat_Feed get articles
                     feedArticles = GetFeedArticles(Convert.ToInt32(f.FeedID));
                     cat.AddRange(feedArticles);
                 }
             }

             cat.Sort();
             cat.Reverse();
            //Return list.
            return cat;
        }

        public void clearOldArticles()
        {
            dbMutex.WaitOne();
            var o = from a in db.Article
                    where a.PublishDate.Value.AddDays(7).CompareTo(DateTime.Now) < 0
                    where a.Favorite == false
                    where a.Read == true
                    select a;

            var j = from a in db.Article
                    where a.PublishDate.Value.AddDays(14).CompareTo(DateTime.Now) < 0
                    where a.Favorite == false
                    select a;

            if (null != o && o.ToList().Count != 0)
            {
                List<Article> allArticles = o.ToList();
                allArticles.AddRange(j.ToList());

                foreach (Article a in allArticles)
                {
                    Feed feed = QueryFeed(Convert.ToInt32(a.FeedID)); 
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            feed.UnreadCount--;
                            SaveChangesToDB();
                        });
                    int unread = Convert.ToInt32(feed.UnreadCount);
                }
                DeleteArticles(o.ToList());
            }
            dbMutex.ReleaseMutex();
        }

        public void PrintOutput()
        {
            dbMutex.WaitOne();
            List<Article> a;
            List<Feed> f;
            List<Category> c;
            var o = from q in db.Article
                    select q;
            a = o.ToList();
            var l = from q in db.Feed
                    select q;
            f = l.ToList();
            var m = from q in db.Category
                    select q;
            c = m.ToList();

            foreach (Article article in a)
            {
                System.Diagnostics.Debug.WriteLine(article.ArticleTitle);
                System.Diagnostics.Debug.WriteLine(article.ArticleID);
            }

            foreach (Feed feed in f)
            {
                System.Diagnostics.Debug.WriteLine(feed.FeedTitle);
                System.Diagnostics.Debug.WriteLine(feed.FeedID);
            }

            foreach (Category category in c)
            {
                System.Diagnostics.Debug.WriteLine(category.CategoryTitle);
                System.Diagnostics.Debug.WriteLine(category.CategoryID);
            }
        dbMutex.ReleaseMutex();
        }
  

    }
}
