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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FeedCast.Models;
using FeedCastLibrary;
using System.Net;
using FeedCast.Views;
using System.Windows;
using FeedCast.Resources;
using G2PO.Resources;

namespace FeedCast.ViewModels
{
    /// <summary>
    /// This is the ViewModel for the Initial Launch of the application.
    /// The user will be presented with the Loaded Initial Catgories to choose from.
    /// </summary>
    public class LaunchPageViewModel : ObservableCollection<InitialCategory>
    {
        public ContentLoader LoadContent { get; set; }

        /// <summary>
        /// Creates a new instance of LaunchPageViewModel with the given NavigationService.
        /// </summary>
        /// <param name="navService">The NavigationService used </param>
        /// <param name="FinishedUri">The Uri to navigate to when this ViewModel
        /// has finished downloading and storing all the initial categories.</param>
        public LaunchPageViewModel()
        {
            LoadContent = new ContentLoader();
            LoadInitialCategories();
        }

        /// <summary>
        /// Loads the defined InitialCategories to memory.
        /// </summary>
        public void LoadInitialCategories()
        {
            // Add InitialCategories here with Feeds (Must contain valid FeedBaseURI's). 
            Add(new InitialCategory(
                AppResources.BusinessCategoryTitleText,
                new Feed() { FeedTitle = "CNN Money", FeedBaseURI = "http://rss.cnn.com/rss/money_topstories.rss" },
                new Feed() { FeedTitle = "Fox Business", FeedBaseURI = "http://feeds.foxbusiness.com/foxbusiness/latest" }));
            Add(new InitialCategory(
                AppResources.EntertainmentCategoryTitleText,
                new Feed() { FeedTitle = "Hollywood Reporter", FeedBaseURI = "http://feeds.feedburner.com/thr/news" },
                new Feed() { FeedTitle = "Huffington Post Entertainment", FeedBaseURI = "http://www.huffingtonpost.com/feeds/verticals/entertainment/index.xml" }));
            Add(new InitialCategory(
                AppResources.FashionCategoryTitleText,
                new Feed() { FeedTitle = "Style", FeedBaseURI = "http://www.style.com/homepage/rss" },
                new Feed() { FeedTitle = "Us Magazine", FeedBaseURI = "http://www.usmagazine.com/celebrity_news/rss" }));
            Add(new InitialCategory(
                AppResources.HealthCategoryTitleText,
                new Feed() { FeedTitle = "Huffington Post Health", FeedBaseURI = "http://www.huffingtonpost.com/feeds/verticals/health/index.xml" },
                new Feed() { FeedTitle = "WebMD", FeedBaseURI = "http://rssfeeds.webmd.com/rss/rss.aspx?RSSSource=RSS_PUBLIC" }));
            Add(new InitialCategory(
                AppResources.MusicCategoryTitleText,
                new Feed() { FeedTitle = "VH1", FeedBaseURI = "http://www.bestweekever.tv/feed/" },
                new Feed() { FeedTitle = "MTV", FeedBaseURI = "http://www.mtv.com/rss/news/news_full.jhtml" }));
            Add(new InitialCategory(
                AppResources.NewsCategoryTitleText,
                new Feed() { FeedTitle = "BBC", FeedBaseURI = "http://feeds.bbci.co.uk/news/rss.xml" },
                new Feed() { FeedTitle = "CNN", FeedBaseURI = "http://rss.cnn.com/rss/cnn_topstories.rss" }));
            Add(new InitialCategory(
                AppResources.PoliticsCategoryTitleText,
                new Feed() { FeedTitle = "NY Times", FeedBaseURI = "http://www.nytimes.com/services/xml/rss/nyt/GlobalHome.xml" },
                new Feed() { FeedTitle = "Reuters", FeedBaseURI = "http://www.reuters.com/rssFeed/topNews" }));

            Add(new InitialCategory(
                AppResources.SportsCategoryTitleText,
                new Feed() { FeedTitle = "ESPN", FeedBaseURI = "http://sports.espn.go.com/espn/rss/news" },
                new Feed() { FeedTitle = "Yahoo Sports", FeedBaseURI = "http://sports.yahoo.com/sow/rss.xml" }));
            Add(new InitialCategory(
                AppResources.TechnologyCategoryTitleText,
                new Feed() { FeedTitle = "Engadget", FeedBaseURI = "http://www.engadget.com/rss.xml" },
                new Feed() { FeedTitle = "Wired", FeedBaseURI = "http://feeds.wired.com/wired/index" }));
        }

        /// <summary>
        /// Downloads and stores the provided list of categories into the database.
        /// </summary>
        /// <param name="selection">An IList of categories selected by the user.</param>
        public void LoadSelection(IList selection)
        {
            if (null != selection)
            {
                foreach (InitialCategory initCat in selection)
                {
                    if (null != initCat)
                    {
                        // Creating a new category with the provided InitialCategory's title
                        // (Note: Casting it to a category yields a strange database error).
                        Category cat = new Category { CategoryTitle = initCat.CategoryTitle };

                        // Add each Category to the database firstly so they will get their CategoryId's assigned.
                        App.DataBaseUtility.AddCategory(cat);

                        if (null != initCat.Feeds)
                        {
                            List<Feed> feedList = initCat.Feeds as List<Feed>;
                            if (null != feedList)
                            {
                                foreach (Feed feed in feedList)
                                {
                                    // Add each Initial Category's feeds to the database secondly so they will have their FeedId's assigned.
                                    App.DataBaseUtility.AddFeed(feed, cat);

                                }
                            }
                        }
                        else
                        {
                            throw new ArgumentNullException("The provided Initial Categories must contain valid Feeds.");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("One or more Initial Categories provided were null.");
                    }
                }

                List<Feed> f = App.DataBaseUtility.GetAllFeeds();
                LoadContent.DownloadFeeds(f);

            }
            else
            {
                throw new ArgumentNullException("Selection must not be null.", "selection");
            }
        }

      
    }
}