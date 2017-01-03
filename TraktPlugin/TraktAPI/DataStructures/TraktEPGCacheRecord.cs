using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TraktPlugin.TraktAPI.DataStructures
{
    [DataContract]
    public class TraktEPGCacheRecord
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "show")]
        public TraktShowSummary Show { get; set; }

        [DataMember(Name = "movie")]
        public TraktMovieSummary Movie { get; set; }
    }
}