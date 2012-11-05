using System;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FeedCastLibrary;

namespace FeedCast.ViewModels
{
    public static class AddTile
    {
        /// <summary>
        /// Create a secondary tile linking to an existing feed.
        /// </summary>
        /// <param name="feed">The feed to be linked to on the tile</param>
        public static void AddLiveTile(Feed feed)
        {
            // Look to see whether the Tile already exists; if so, don't try to create it again.
            ShellTile tileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("/Feed/" +
                feed.FeedID.ToString()));

            // Create the Tile if we didn't find that it already exists.
            if (tileToFind == null)
            {
                // Create the Tile object and set some initial properties for the Tile.
                StandardTileData newTileData = new StandardTileData
                {
                    BackgroundImage = new Uri(feed.ImageURL, UriKind.RelativeOrAbsolute),
                    Title = feed.FeedTitle,
                    Count = 0,
                    BackTitle = feed.FeedTitle,
                    BackContent = "Read the latest in " + feed.FeedTitle + "!",
                };

                // Create the Tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
                ShellTile.Create(new Uri("/Feed/" + feed.FeedID, UriKind.Relative), newTileData);
                // note: Tile URI could also have listed the full path to the feed page. For example --> /Views/FeedPage.xaml?id=

                feed.IsPinned = true;
                App.DataBaseUtility.SaveChangesToDB();
            }
            
        }

        public static void AddLiveTile(Category cat)
        {
            // Look to see whether the Tile already exists; if so, don't try to create it again.
            ShellTile tileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("/Category/" +
                cat.CategoryID.ToString()));

            // Create the Tile if we didn't find that it already exists.
            if (tileToFind == null)
            {
                // Create an image for the category if there isnt one. 
                if (cat.ImageURL == null || cat.ImageURL == String.Empty)
                {
                    cat.ImageURL = ImageGrabber.GetDefaultImage();                
                }

                // Create the Tile object and set some initial properties for the Tile.
                StandardTileData newTileData = new StandardTileData
                {
                    BackgroundImage = new Uri(cat.ImageURL, UriKind.RelativeOrAbsolute),
                    Title = cat.CategoryTitle,
                    Count = 0,
                    BackTitle = cat.CategoryTitle,
                    BackContent = "Read the latest in " + cat.CategoryTitle + "!",
                };

                // Create the Tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
                ShellTile.Create(new Uri("/Category/" + cat.CategoryID, UriKind.Relative), newTileData);
                // note: Tile URI could have listed the full path to the Category page. For example --> /Views/CategoryPage.xaml?id=

                cat.IsPinned = true;
                App.DataBaseUtility.SaveChangesToDB();
            }

        }
    }
}
