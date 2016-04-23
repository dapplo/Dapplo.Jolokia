/*
	Dapplo - building blocks for desktop applications
	Copyright (C) 2015-2016 Dapplo

	For more information see: http://dapplo.net/
	Dapplo repositories are hosted on GitHub: https://github.com/dapplo

	This file is part of Dapplo.Jolokia.

	Dapplo.Jolokia is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	Dapplo.Jolokia is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with Dapplo.Jolokia. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;

namespace Dapplo.Jolokia.Model
{
	public class MBean
	{
		public string FullyqualifiedName => $"{Domain}:{Name}";

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
