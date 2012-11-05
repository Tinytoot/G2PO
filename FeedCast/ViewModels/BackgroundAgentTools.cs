/* 
    Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see the MSDN article about this app, visit http://go.microsoft.com/fwlink/?LinkId=247592 
  
*/

// Enables user-specified timing of the background agent for debugging.
//#define DEBUG_AGENT

using System;
using Microsoft.Phone.Scheduler;

namespace FeedCast.ViewModels
{
    public class BackgroundAgentTools
    {
        private PeriodicTask periodicDownload;
        private string periodicTaskName;

        /// <summary>
        /// Constructor
        /// </summary>
        public BackgroundAgentTools()
        {
            periodicTaskName = "FeedCastAgent";
        }

        /// <summary>
        /// Creates or renews the periodic agent on the scheduler.
        /// </summary>
        /// <returns>Whether the periodic agent is created or not</returns>
        public bool StartPeriodicAgent()
        {
            periodicDownload = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;
            bool wasAdded = true;

            // If the task already exists and the IsEnabled property is false, then background
            // agents have been disabled by the user.
            if (periodicDownload != null && !periodicDownload.IsEnabled)
            {
                // Can't add the agent. Return false!
                wasAdded = false;
            }

            // If the task already exists and background agents are enabled for the
            // application, then remove the agent and add again to update the scheduler.
            if (periodicDownload != null && periodicDownload.IsEnabled)
            {
                ScheduledActionService.Remove(periodicTaskName);
            }

            periodicDownload = new PeriodicTask(periodicTaskName);
            periodicDownload.Description = "Allows FeedCast to download new articles on a regular schedule.";

            // Scheduling the agent may not be allowed because maximum number 
            // of agents has been reached or the phone is a 256-MB device.
            try
            {
                ScheduledActionService.Add(periodicDownload);
            }
            catch (SchedulerServiceException) { }

            // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
            //#if(DEBUG_AGENT)
            //            ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(60));
            //#endif
            return wasAdded;
        }
    }
}
