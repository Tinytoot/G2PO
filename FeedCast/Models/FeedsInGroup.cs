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
using FeedCastLibrary;

namespace FeedCast.Models
{
    public class FeedsInGroup : ObservableCollection<Feed>, IComparable<FeedsInGroup>
    {
        public FeedsInGroup(string category)
        {
            Key = category;
        }

        public string Key { get; set; }

        public bool HasItems { get { return Count > 0; } }

        /// <summary>
        /// Compares this instance with the specified FeedsInGroup and indicates
        /// whether this instance precedes, follows, or appears in the 
        /// same position in the sort order as the specified FeedsInGroup.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Less than zero if this instance precedes "other".
        /// Zero if this instance has the same position in the sort order as "other".
        /// Greater than 0 if This instance follows value or value is null.</returns>
        public int CompareTo(FeedsInGroup other)
        {
            return this.Key[0].CompareTo(other.Key[0]);
        }
    }
}
