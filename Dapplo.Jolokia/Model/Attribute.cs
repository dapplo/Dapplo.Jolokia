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

	Dapplo.HttpExtensions is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
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
		/// Base Jolokia Uri http(s)://host:port/jolokia where this attribute was found
		/// </summary>
		public Uri JolokiaBaseUri
		{
			get;
			set;
		}

		/// <summary>
		/// MBean parent with it's fully qualified name
		/// </summary>
		public string Parent
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
			var readUri = JolokiaBaseUri.AppendSegments("read", Parent, Name);
			var result = await readUri.GetAsJsonAsync(true, token, httpSettings).ConfigureAwait(false);
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
			var writeUri = JolokiaBaseUri.AppendSegments("write", Parent, Name).AppendSegments(argument);
			var result = await writeUri.GetAsJsonAsync(true, token, httpSettings).ConfigureAwait(false);
			return result.value;
		}

		/// <summary>
		/// Set a history for the attribute
		/// </summary>
		/// <param name="count">Length of history</param>
		/// <param name="seconds">seconds to keep elements</param>
		/// <param name="token"></param>
		/// <param name="httpSettings"></param>
		public async Task EnableHistory(int count, int seconds, CancellationToken token = default(CancellationToken), IHttpSettings httpSettings = null)
		{
			var historyLimitUri = JolokiaBaseUri.AppendSegments("exec/jolokia:type=Config/setHistoryLimitForAttribute", Parent, Name, "[null]", "[null]", count, seconds);
			await historyLimitUri.GetAsJsonAsync(true, token, httpSettings).ConfigureAwait(false);
		}
	}
}
