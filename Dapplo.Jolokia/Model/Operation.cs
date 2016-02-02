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
	/// <summary>
	/// Details of an operation
	/// </summary>
	public class Operation
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
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		/// Arguments for the operation
		/// </summary>
		public ICollection<Argument> Arguments
		{
			get;
			set;
		} = new List<Argument>();

		/// <summary>
		/// The returntype of the operation
		/// </summary>
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
