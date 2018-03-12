//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.Jolokia
// 
//  Dapplo.Jolokia is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.Jolokia is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.Jolokia. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

using System;
using System.Runtime.Serialization;

namespace Dapplo.Jolokia.Entities
{
    /// <summary>
    /// Value container for reading values
    /// </summary>
    [DataContract]
    public class ValueContainer<TValue>
    {
        /// <summary>
        /// Timestamp for when the value was retrieved
        /// </summary>
        public DateTimeOffset Timestamp
        {
            get
            {
#if NET46
                return DateTimeOffset.FromUnixTimeSeconds(Epoch);
#else
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(Epoch);
#endif
            }
        }

        /// <summary>
        /// Timestamp with seconds since 1.1.1970, for when the value was retrieved
        /// </summary>
        [DataMember(Name = "timestamp")]
        public long Epoch { get; set; }

        /// <summary>
        /// Status of the retrieval
        /// </summary>
        [DataMember(Name = "status")]
        public int Status { get; set; }

        /// <summary>
        /// The retrieved value
        /// </summary>
        [DataMember(Name = "value")]
        public TValue Value { get; set; }
    }
}
