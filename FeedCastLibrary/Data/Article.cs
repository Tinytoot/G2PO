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

namespace FeedCastLibrary
{
    public partial class Article : IComparable<Article>, IEquatable<Article>
    {
        /// <summary>
        /// Compares this instance with the specified article and indicates
        /// whether this instance precedes, follows, or appears in the 
        /// same position in the sort order as the specified article.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Less than zero if this instance precedes "other".
        /// Zero if this instance has the same position in the sort order as "other".
        /// Greater than 0 if This instance follows value or value is null.</returns>
        public int CompareTo(Article other)
        {
            return ((DateTime)this._PublishDate).CompareTo((DateTime)other.PublishDate);
        }

        /// <summary>
        /// Determines whether two article objects have the same value.
        /// </summary>
        /// <param name="other"> The article to compare this article to.</param>
        /// <returns>True if this article has the same value as "other" article,
        /// otherwise false.</returns>
        public bool Equals(Article other)
        {
            return this.ArticleBaseURI.Equals(other.ArticleBaseURI);
        }

        /// <summary>
        /// Returns a string that represents this article.
        /// </summary>
        /// <returns>A string representation of this article.</returns>
        public override string ToString()
        {
            return this.ArticleTitle;
        }
    }
}
