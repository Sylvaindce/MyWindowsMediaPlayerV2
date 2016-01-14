using MyWindowsMediaPlayerV2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyWindowsMediaPlayerV2.Utilities
{
    public class Search
    {
        internal static int itExist(string str, Library lib)
        {
            for (int i = 0; i < lib.songs.Count(); i++)
            {
                if (str == lib.songs[i].title)
                    return i;
            }
            return -1;
        }
        internal static int itExist(string str, List<Album> album)
        {
            for (int i = 0; i < album.Count(); i++)
            {
                if (album[i].name == str)
                    return i;
            }
            return -1;
        }

        internal static int itExist(string str, List<Artist> artist)
        {
            for (int i = 0; i < artist.Count(); i++)
            {
                if (artist[i].name == str)
                    return i;
            }
            return -1;
        }

        internal static int itExist(string str, List<string> array)
        {
            for (int i = 0; i < array.Count(); i++)
            {
                if (array[i] == str)
                    return i;
            }
            return -1;
        }
    }
}