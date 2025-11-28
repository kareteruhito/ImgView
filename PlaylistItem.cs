using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;

namespace ImgView
{
    public class PlaylistItem
    {
        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FullName);
            }
        }
        public Guid Id {get; set;}

        public string FullName {get; set;}

        public PlaylistItem()
        {
            Id = Guid.NewGuid();
        }
        public PlaylistItem(string fullname) : this()
        {
            FullName = fullname;
        }
        static public void Save(IEnumerable<PlaylistItem> itmes, string filename)
        {
            using(var sw = new StreamWriter(filename))
            {
                foreach (var item in itmes)
                {
                    sw.WriteLine(item.FullName);
                }
            }
        }
        static public IEnumerable<PlaylistItem> Load(string filename)
        {
            List<PlaylistItem> items = new List<PlaylistItem>();

            using(var sr = new StreamReader(filename))
            {
                while (sr.Peek() >= 0)
                {
                    string fullname = sr.ReadLine();

                    items.Add(new PlaylistItem(fullname));
                }
            }

            return items;
        }
    }
}