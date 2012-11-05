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
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using FeedCastLibrary;
using FeedCast.Models;
using System.Net;

namespace FeedCast.ViewModels
{
    /// <summary>
    /// ViewModel for the mainpage's what's new panorama item.
    /// Add Articles here to populate that section of the UI.
    /// </summary>
    public class MainPageWhatsNewViewModel : ObservableCollection<Article>
    {
        /// <summary>
        /// ViewModel for What's New panel, handles what articles are shown & what happens when refresh is tapped. 
        /// </summary>
        public MainPageWhatsNewViewModel()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    List<Article> articleList = App.DataBaseUtility.WhatsNewCollection(20);

                    foreach (Article a in articleList)
                    {
                        Add(a);
                    }
                });

            //Update!!
            if (!Settings.InitialLaunchSetting)
            {
                //LoadContent();
            }

        }

        /// <summary>
        /// Gets all the Feeds from the Database
        /// </summary>
        public void LoadContent(Action callback)
        {
            ContentLoader loadContent = new ContentLoader(callback);
            loadContent.LoadingFinished += DisplayFeeds;
            loadContent.DownloadFeeds(App.DataBaseUtility.GetAllFeeds());
        }



        /// <summary>
        /// Saves Articles to the Database 
        /// Displays the 20 most recent. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DisplayFeeds(object sender, ContentLoader.LoadingFinishedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                //Pop on some on top, remove from the bottom
                if (this.Count > 0)
                {
                    List<Article> newArticles = App.DataBaseUtility.UpdateWhatsNewCollection();
                    for (int i = 0; i < newArticles.Count; i++)
                    {
                        Insert(0, newArticles[i]);
                        if (newArticles.Count >= 20 && i >= 20)
                        {
                            RemoveAt(10);
                        }
                    }
                }
                //If empty then just pull the 20 most recent.
                // In this scenario, the user removes all the feeds, adds some, and taps refresh. 
                else
                {
                    foreach (Article a in App.DataBaseUtility.WhatsNewCollection(20))
                    {
                        Add(a);
                    }
                }

                (sender as ContentLoader)._callback();
            });
        }
    }
}

