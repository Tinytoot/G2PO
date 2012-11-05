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
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System;

namespace FeedCast.Models
{
    /// <summary>
    /// A ListBox with Header and SubHeader scrolling content.
    /// </summary>
    [TemplatePartAttribute(Name = HeaderName, Type = typeof(TextBlock))]
    [TemplatePartAttribute(Name = LastUpdatedName, Type = typeof(TextBlock))]
    public class HeaderedListBox : ListBox
    {
        #region TemplatePart Names
        /// <summary>
        /// Template Part name for Header TextBlock.
        /// </summary>
        private const string HeaderName = "Header";

        /// <summary>
        /// Template Part name for SubHeader TextBlock.
        /// </summary>
        private const string LastUpdatedName = "LastUpdated";
        #endregion

        #region Private fields
        /// <summary>
        /// Header TextBlock.
        /// </summary>
        private TextBlock _header;

        /// <summary>
        /// SubHeader TextBlock.
        /// </summary>
        private TextBlock _lastUpdatedText;
        #endregion

        #region Dependency Properties
        #region HeaderText
        /// <summary>
        /// The Header text for this HeaderedListBox.
        /// </summary>
        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set
            {
                _header.Visibility = (string.IsNullOrEmpty(value) && null != _header)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                SetValue(HeaderTextProperty, value);
            }
        }

        /// <summary>
        /// Property for the Header text for this HeaderedListBox.
        /// </summary>
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register(
            "HeaderText",
            typeof(string),
            typeof(HeaderedListBox),
            new PropertyMetadata(null));
        #endregion

        #region LastUpdatedText
        /// <summary>
        /// The SubHeader text for this HeaderedListBox.
        /// </summary>
        public string LastUpdatedText
        {
            get { return (string)GetValue(LastUpdatedTextProperty); }
            set
            {
                _lastUpdatedText.Visibility = (string.IsNullOrEmpty(value) && null != _lastUpdatedText)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                SetValue(LastUpdatedTextProperty, value);
            }
        }

        /// <summary>
        /// Property for the SubHeader text for this HeaderedListBox.
        /// </summary>
        public static readonly DependencyProperty LastUpdatedTextProperty =
            DependencyProperty.Register(
            "LastUpdatedText",
            typeof(string),
            typeof(HeaderedListBox),
            new PropertyMetadata("Last updated 1 minute ago."));
        #endregion
        #endregion

        /// <summary>
        /// Instantiates a new HeaderedListBox.
        /// </summary>
        public HeaderedListBox()
            : base()
        {
            this.DefaultStyleKey = typeof(HeaderedListBox);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _header = GetTemplateChild(HeaderName) as TextBlock;
            _lastUpdatedText = GetTemplateChild(LastUpdatedName) as TextBlock;
        }
    }
}
