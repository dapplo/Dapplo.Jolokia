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

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dapplo.Jolokia.Entities
{
    /// <summary>
    /// Details of an operation
    /// </summary>
    [DataContract]
    public class MBeanOperation
    {
        /// <summary>
        /// Name of the operation
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Description of the Operation
        /// </summary>
        [DataMember(Name = "desc")]
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Arguments for the operation
        /// </summary>
        [DataMember(Name = "args")]
        public ICollection<Argument> Arguments
        {
            get;
            set;
        } = new List<Argument>();

        /// <summary>
        /// The returntype of the operation
        /// </summary>
        [DataMember(Name = "ret")]
        public string ReturnType
        {
            get;
            set;
        } = "java.lang.String";

        /// <summary>
        /// MBean parent with it's fully qualified name
        /// </summary>
        public string Parent
        {
            get;
            set;
        }
    }
}
