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

using System.Runtime.Serialization;

namespace Dapplo.Jolokia.Ui.Entities
{
	[DataContract]
	public class MemoryUsage
	{
		[DataMember(Name = "max")]
		public long Max { get; set; }
		[DataMember(Name = "committed")]
		public long Committed { get; set; }
		[DataMember(Name = "init")]
		public long Init { get; set; }
		[DataMember(Name = "used")]
		public long Used { get; set; }
	}
}
