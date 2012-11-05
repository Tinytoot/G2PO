// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using FeedCastLibrary;
using FeedCast.Models;

namespace FeedCast.ViewModels
{
    public class MainPageSettingsViewModel
    {
        public Settings AppSettings { get; set; }

        public MainPageSettingsViewModel()
        {
            AppSettings = new Settings();
        }
    }
}
