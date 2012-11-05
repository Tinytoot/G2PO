/* 
    Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see the MSDN article about this app, visit http://go.microsoft.com/fwlink/?LinkId=247592 
  
*/

//#define DEBUG_AGENT

using System.Collections.Generic;
using System.Net;
using System.Windows;
using FeedCastLibrary;
using Microsoft.Phone.Scheduler;

namespace FeedCastAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <summary>
        /// Tracks whether the agent is initialized or not
        /// </summary>
        private static volatile bool _classInitialized;
        
        /// <summary>
        /// Stores the location of the database
        /// </summary>
        private static readonly string DBLocation = "Data Source=isostore:/LocalDatabase.sdf";
        
        /// <summary>
        /// DataBaseUtility allowing the agent use with the database
        /// </summary>
        protected static DataUtils DataBaseTools { get; private set; }

        /// <summary>
        /// Allows the background agent access into isolated storage
        /// </summary>
        private Settings AgentSettings;

        /// <summary>
        /// Holds the number of remaining downloads
        /// </summary>
        private int _remainingDownloads;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }

            // Initialize the database utilities and storage settings.
            LocalDatabaseDataContext db = new LocalDatabaseDataContext(DBLocation);
            DataBaseTools = new DataUtils(DBLocation);
            AgentSettings = new Settings();
        }
        

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            // Run the periodic task.
            List<Feed> allFeeds = DataBaseTools.GetAllFeeds();
            _remainingDownloads = allFeeds.Count;
            if (_remainingDownloads > 0)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        WebTools downloader = new WebTools(new SynFeedParser());
                        downloader.SingleDownloadFinished += SendToDatabase;
                        try
                        {
                            downloader.Download(allFeeds);
                        }
                        //TODO handle errors.
                        catch { }
                    });
            }

            // Used to quickly invoke task when debugging.
            //#if DEBUG_AGENT
            //            ScheduledActionService.LaunchForTest(task.Name, System.TimeSpan.FromSeconds(600));
            //#endif
            
        }

        /// <summary>
        /// Send the articles that the background agent has downloaded to the database.
        /// </summary>
        /// <param name="sender">The object that calls the event</param>
        /// <param name="e">The value returned from the event raiser</param>
        private void SendToDatabase(object sender, SingleDownloadFinishedEventArgs e)
        {
            // Make sure download is not null!
            if (e.DownloadedArticles != null)
            {
                DataBaseTools.AddArticles(e.DownloadedArticles, e.ParentFeed);
                _remainingDownloads--;
               // System.Diagnostics.Debug.WriteLine("BGAgent downloaded " + e.ParentFeed.FeedTitle);
            }

            // If there are no remaining downloads, 
            // tell the scheduler that the background agent is done.
            if (_remainingDownloads <= 0)
            {
                NotifyComplete();
            }
        }
    }
}