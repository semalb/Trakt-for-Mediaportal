﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TraktPlugin.TraktAPI.DataStructures
{
    [DataContract]
    public class TraktSeason
    {
        [DataMember(Name = "number")]
        public int Number { get; set; }

        [DataMember(Name = "ids")]
        public TraktSeasonId Ids { get; set; }
    }
}
