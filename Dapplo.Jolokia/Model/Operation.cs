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

 using Dapplo.HttpExtensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
		/// Execute the operation
		/// </summary>
		/// <param name="arguments">Array of strings for the arguments, check the arguments for what needs to be passed</param>
		/// <returns>result with string, check the return type for what can be returned</returns>
		public async Task<dynamic> Execute(string[] arguments, CancellationToken token = default(CancellationToken), IHttpSettings httpSettings = null)
		{
			int passedArgumentCount = arguments == null ? 0 : arguments.Length;
			int neededArgumentCount = Arguments == null ? 0 : Arguments.Count;
			if (passedArgumentCount != neededArgumentCount)
			{
				throw new ArgumentException($"Passed arguments for operation {Name} do not match.");
			}
			var execUri = JolokiaBaseUri.AppendSegments("exec", Parent, Name).AppendSegments(arguments);
			var result = await execUri.GetAsJsonAsync(true, token, httpSettings).ConfigureAwait(false);
			return result.value;
		}

		/// <summary>
		/// Set a history for the operation
		/// </summary>
		/// <param name="count">Length of history</param>
		/// <param name="seconds">seconds to keep elements</param>
		/// <param name="token"></param>
		/// <param name="httpSettings"></param>
		public async Task EnableHistory(int count, int seconds, CancellationToken token = default(CancellationToken), IHttpSettings httpSettings = null)
		{
			var historyLimitUri = JolokiaBaseUri.AppendSegments("exec/jolokia:type=Config/setHistoryLimitForOperation", Parent, Name, "[null]", "[null]", count, seconds);
			await historyLimitUri.GetAsJsonAsync(true, token, httpSettings).ConfigureAwait(false);
		}
	}
}
