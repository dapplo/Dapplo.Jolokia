/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) Dapplo 2015-2016
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;

namespace Dapplo.Jolokia.Model
{
	public class MBean
	{
		public string FullyqualifiedName
		{
			get
			{
				return string.Format("{0}:{1}", Domain, Name);
			}
		}

		public string Name
		{
			get;
			set;
		}

		public string Domain
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public IDictionary<string, Attr> Attributes
		{
			get;
			set;
		} = new Dictionary<string, Attr>();

		public IDictionary<string, Operation> Operations
		{
			get;
			set;
		} = new Dictionary<string, Operation>();
	}
}
