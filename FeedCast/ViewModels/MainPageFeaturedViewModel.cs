/* 
    Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see the MSDN article about this app, visit http://go.microsoft.com/fwlink/?LinkId=247592 
  
*/
using System.Windows;
using FeedCast.Models;
using FeedCastLibrary;
using System;

namespace FeedCast.ViewModels
{
    public class MainPageFeaturedViewModel : DependencyObject
    {

        #region HubTile Article Properties
        public Article HubTileArticle1
        {
            get { return (Article)GetValue(HubTileArticle1Property); }
            set { SetValue(HubTileArticle1Property, value); }
        }

        public static readonly DependencyProperty HubTileArticle1Property =
            DependencyProperty.Register(
            "HubTileArticle1",
            typeof(Article),
            typeof(MainPageFeaturedViewModel),
            new PropertyMetadata(null));

        public Article HubTileArticle2
        {
            get { return (Article)GetValue(HubTileArticle2Property); }
            set { SetValue(HubTileArticle2Property, value); }
        }

        public static readonly DependencyProperty HubTileArticle2Property =
            DependencyProperty.Register(
            "HubTileArticle2",
            typeof(Article),
            typeof(MainPageFeaturedViewModel),
            new PropertyMetadata(null));

        public Article HubTileArticle3
        {
            get { return (Article)GetValue(HubTileArticle3Property); }
            set { SetValue(HubTileArticle3Property, value); }
        }

        public static readonly DependencyProperty HubTileArticle3Property =
            DependencyProperty.Register(
            "HubTileArticle3",
            typeof(Article),
            typeof(MainPageFeaturedViewModel),
            new PropertyMetadata(null));

        public Article HubTileArticle4
        {
            get { return (Article)GetValue(HubTileArticle4Property); }
            set { SetValue(HubTileArticle4Property, value); }
        }

        public static readonly DependencyProperty HubTileArticle4Property =
            DependencyProperty.Register(
            "HubTileArticle4",
            typeof(Article),
            typeof(MainPageFeaturedViewModel),
            new PropertyMetadata(null));

        public Article HubTileArticle5
        {
            get { return (Article)GetValue(HubTileArticle5Property); }
            set { SetValue(HubTileArticle5Property, value); }
        }

        public static readonly DependencyProperty HubTileArticle5Property =
            DependencyProperty.Register(
            "HubTileArticle5",
            typeof(Article),
            typeof(MainPageFeaturedViewModel),
            new PropertyMetadata(null));

        public Article HubTileArticle6
        {
            get { return (Article)GetValue(HubTileArticle6Property); }
            set { SetValue(HubTileArticle6Property, value); }
        }

        public static readonly DependencyProperty HubTileArticle6Property =
            DependencyProperty.Register(
            "HubTileArticle6",
            typeof(Article),
            typeof(MainPageFeaturedViewModel),
            new PropertyMetadata(null));
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MainPageFeaturedViewModel()
        {
            GetFeatured();
        }

        /// <summary>
        /// Obtains the latest featured articles and sets them up to be displayed.
        /// </summary>
        public void GetFeatured()
        {
            // Gather the latest featured articles.
            // If there are no new ones, then old ones will be used.
            FeaturedAlgorithm.UpdateFeatured();

            // Set the articles to be displayed.
            // Gather each article from the database to ensure that it actually exists.
            if (Settings.FeaturedArticles != null)
            {
                HubTileArticle1 = App.DataBaseUtility.QueryArticle(Convert.ToInt32(Settings.FeaturedArticles[0].ArticleID));
                HubTileArticle2 = App.DataBaseUtility.QueryArticle(Convert.ToInt32(Settings.FeaturedArticles[1].ArticleID));
                HubTileArticle3 = App.DataBaseUtility.QueryArticle(Convert.ToInt32(Settings.FeaturedArticles[2].ArticleID));
                HubTileArticle4 = App.DataBaseUtility.QueryArticle(Convert.ToInt32(Settings.FeaturedArticles[3].ArticleID));
                HubTileArticle5 = App.DataBaseUtility.QueryArticle(Convert.ToInt32(Settings.FeaturedArticles[4].ArticleID));
                HubTileArticle6 = App.DataBaseUtility.QueryArticle(Convert.ToInt32(Settings.FeaturedArticles[5].ArticleID));
            }
        }

        /// <summary>
        /// Updates the Featured section if an article inside was removed.
        /// </summary>
        public void RedoFeatured()
        {
            bool articlesExist = true;

            // Check to see if all the featured articles still exist.
            foreach (Article article in Settings.FeaturedArticles)
            {
                if (null == App.DataBaseUtility.QueryArticle(Convert.ToInt32(article.ArticleID)))
                {
                    articlesExist = false;
                    break;
                }
            }

            // If they don't, get new ones.
            if (!articlesExist)
            {
                GetFeatured();
            }
        }
    }
}
