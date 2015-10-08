/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
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

using Dapplo.HttpExtensions;
using System;
using System.Threading;
using System.Threading.Tasks;

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
		/// The read Uri for the attribute
		/// </summary>
		public Uri ReadUri
		{
			get;
			set;
		}

		/// <summary>
		/// The write Uri for the attribute
		/// </summary>
		public Uri WriteUri
		{
			get;
			set;
		}

		/// <summary>
		/// Read the attribute
		/// </summary>
		/// <param name="token"></param>
		/// <param name="httpSettings"></param>
		/// <returns>dynamic, check the Type for what it is</returns>
		public async Task<dynamic> Read(CancellationToken token = default(CancellationToken), IHttpSettings httpSettings = null)
		{
			var result = await ReadUri.GetAsJsonAsync(true, token, httpSettings).ConfigureAwait(false);
			return result.value;
		}

		/// <summary>
		/// Write the attribute
		/// </summary>
		/// <param name="argument"></param>
		/// <param name="token"></param>
		/// <param name="httpSettings"></param>
		/// <returns>dynamic, check the Type for what it is</returns>
		public async Task<dynamic> Write(string argument, CancellationToken token = default(CancellationToken), IHttpSettings httpSettings = null)
		{
			var writeUriWithParam = WriteUri.AppendSegments(argument);
			var result = await writeUriWithParam.GetAsJsonAsync(true, token, httpSettings).ConfigureAwait(false);
			return result.value;
		}
	}
}
