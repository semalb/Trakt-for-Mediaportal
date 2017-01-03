using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MediaPortal.Configuration;
using TraktPlugin.Extensions;
using TraktPlugin.TraktAPI.DataStructures;
using TraktPlugin.TraktAPI.Extensions;

namespace TraktPlugin.Cache
{
    enum EPG_CACHE_CODES
    {

    }
    public static class EPGCache
    {
        private static Dictionary<string, string> EPGCacheDictionary;
        private static List<string> newRecords = new List<string>();
        private static string cacheFile = Path.Combine(Config.GetFolder(Config.Dir.Config), string.Format(@"Trakt\TmdbCache\EPGCache.txt"));
        //public const string nullRecord = "nullRecord";
        public static readonly TraktEPGCacheRecord nullShow = new TraktEPGCacheRecord {Type="nullRecord"};

        public static void loadCache()
        {
            if (!File.Exists(cacheFile))
            {
                StreamWriter fs = new StreamWriter(cacheFile);
                fs.WriteLine("#LocalizedTitle|#JSONOriginalRecord");
                fs.Close();
            }
            TraktLogger.Info("Loading EPG cache");
            StreamReader showsEPGCacheFile = new StreamReader(cacheFile);
            //showsEPGCacheFile.ReadLine(); //the first line is the header, skip.
            EPGCacheDictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            TraktLogger.Info("reading first line {0}",showsEPGCacheFile.ReadLine());
            string line;
            char separator = '|';
            int count = 0;
            while ((line = showsEPGCacheFile.ReadLine()) != null)
            {
                string[] substrings = line.Split(separator);
                EPGCacheDictionary.Add(substrings[0], substrings[1]);
                count++;
            }
            TraktLogger.Info("Loaded '{0}' items", count);
            showsEPGCacheFile.Close();
        }
        public static void saveCache()
        {
            TraktLogger.Info("Saving EPG Cache to disk");
            StreamWriter fs = new StreamWriter(cacheFile,true);
            foreach (string record in newRecords)
            {
#if DEBUG
                TraktLogger.Info("Writing {0} to cachefile", record);
#endif
                fs.WriteLine(record);
            }
            fs.Close();
            EPGCacheDictionary.Clear();
            newRecords.Clear();
        }

        //Returns true if localizedTitle is on cache
        public static bool searchOnCache(string localizedTitle)
        {
#if DEBUG
            TraktLogger.Info("Checking '{0}' on cache ", localizedTitle);
#endif
            
            string dataOut;
            lock (EPGCacheDictionary)
            {
                //if (EPGCacheDictionary.TryGetValue(localizedTitle, out dataOut))
                //{
                //    return true;
                //}
                //else return false;
                TraktLogger.Info("Status of {0} in cache: {1}:", localizedTitle, EPGCacheDictionary.TryGetValue(localizedTitle, out dataOut));
                return EPGCacheDictionary.TryGetValue(localizedTitle, out dataOut);
            }
        }
        public static bool searchOnCache(string localizedTitle, out TraktEPGCacheRecord data)
        {
#if DEBUG
            TraktLogger.Info("Checking '{0}' on cache ", localizedTitle);
#endif
            lock (EPGCacheDictionary)
            {
                string record;
                bool found =  EPGCacheDictionary.TryGetValue(localizedTitle, out record );
                data = record.FromJSON<TraktEPGCacheRecord>();
                return found;
            }
        }

        public static TraktEPGCacheRecord createData(TraktMovieSummary data)
        {
            return new TraktEPGCacheRecord
            {
                Type = "movie",
                Movie = data
            };
        }
        public static TraktEPGCacheRecord createData(TraktShowSummary data)
        {
            return new TraktEPGCacheRecord
            {
                Type = "show",
                Show = data
            };
        }
        
        // Returns true if record is successfully added on cache
        public static bool addOnCache(string localizedTitle, TraktEPGCacheRecord data)
        {

            if (searchOnCache(localizedTitle)) return false;
            else
            {
                lock (EPGCacheDictionary)
                {
                    EPGCacheDictionary.Add(localizedTitle, data.ToJSON());
                    newRecords.Add(string.Format("{0}|{1}", localizedTitle, data.ToJSON()));
                    return true;
                }
            }
        }
        public static bool addOnCache(string localizedTitle)
        {
            return addOnCache(localizedTitle, nullShow);
        }




        //This method is deprecated as public.
        //        private static bool searchOnCache(string localizedTitle, out string data)
        //        {
        //#if DEBUG
        //            TraktLogger.Info("Checking '{0}' on cache ", localizedTitle);
        //#endif
        //            lock (EPGCacheDictionary)
        //            {
        //                return EPGCacheDictionary.TryGetValue(localizedTitle, out data);
        //            }
        //        }
    }
}