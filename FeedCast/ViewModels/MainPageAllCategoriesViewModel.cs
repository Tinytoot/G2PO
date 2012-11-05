/* 
    Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see the MSDN article about this app, visit http://go.microsoft.com/fwlink/?LinkId=247592 
  
*/
using System.Collections.ObjectModel;
using FeedCast.Models;
using FeedCastLibrary;
using System.Collections.Generic;

namespace FeedCast.ViewModels
{
    public class MainPageAllCategoriesViewModel : ObservableCollection<Category>
    {
        public MainPageAllCategoriesViewModel()
        {
            foreach (Category c in App.DataBaseUtility.GetAllCategories())
            {
                Add(c);
            }
        }
    }
}
