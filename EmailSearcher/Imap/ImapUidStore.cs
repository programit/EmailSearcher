using System.Collections.Generic;
using System.IO;

namespace EmailSearcher.Imap
{
    public class IMapUidStore
    {
        private const string IndexedUidStorePath = @"D:\EmailSearcher\IndexedUids.txt";

        private readonly HashSet<uint> cachedUids = new HashSet<uint>();

        public IMapUidStore()
        {
            this.ReadUidsIntoCache();
        }

        public void AddUid(uint uid)
        {
            if (this.cachedUids.Contains(uid))
            {
                return;
            }

            this.cachedUids.Add(uid);
            File.AppendAllText(IMapUidStore.IndexedUidStorePath, $"{uid}\n");
        }

        public bool IsUidStored(uint uid)
        {
            return this.cachedUids.Contains(uid);
        } 

        private void ReadUidsIntoCache()
        {
            if (!File.Exists(IMapUidStore.IndexedUidStorePath))
            {
                return;
            }

            // TODO: This is probably a bad idea... A better solution is to read one line at a time so we don't allocate this huge array
            string[] lines = File.ReadAllLines(IMapUidStore.IndexedUidStorePath);

            foreach (string line in lines)
            {
                uint uid;
                if (uint.TryParse(line, out uid))
                {
                    this.cachedUids.Add(uid);
                }
            }
        }
    }
}