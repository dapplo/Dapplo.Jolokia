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


namespace Dapplo.Jolokia.Model
{
	/// <summary>
	/// Contains the attribute information belonging to an MBean
	/// </summary>
	public class Attr
	{
		/// <summary>
		/// Name of the attribute
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Description of the attribute
		/// </summary>
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		/// Type for the attribute
		/// </summary>
		public string Type
		{
			get;
			set;
		} = "java.lang.String";

		public bool IsReadonly
		{
			get;
			set;
		} = true;

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
