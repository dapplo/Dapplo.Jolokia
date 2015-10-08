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
using Dapplo.Jolokia.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dapplo.Jolokia
{
	/// <summary>
	/// A simple implementation of a Jolokia API for .NET
	/// </summary>
	public class Jolokia
	{
		private readonly Uri _baseUri;
		public IHttpSettings HttpSettings
		{
			get;
			set;
		}
		/// <summary>
		/// Create a Jolokia API object
		/// </summary>
		/// <param name="hostname"></param>
		/// <param name="port"></param>
		/// <param name="schema"></param>
		/// <returns>Jolokia</returns>
		public static async Task<Jolokia> Create(string hostname, int port, string schema = "http", string username = null, string password = null, CancellationToken token = default(CancellationToken), IHttpSettings httpSettings = null)
		{
			var uriBuilder = new UriBuilder(schema, hostname, port, "/jolokia");
			uriBuilder.UserName = username;
			uriBuilder.Password = password;
			return await Create(uriBuilder.Uri, token, httpSettings).ConfigureAwait(false);
		}

		/// <summary>
		/// Create a Jolokia API object
		/// </summary>
		/// <param name="jolokiaUri">Predefined base-uri to the jolokia REST API (normally ends with jolokia)</param>
		/// <returns>Jolokia</returns>
		public static async Task<Jolokia> Create(Uri jolokiaUri, CancellationToken token = default(CancellationToken), IHttpSettings httpSettings = null)
		{
			var jolokia = new Jolokia(jolokiaUri);
			jolokia.HttpSettings = httpSettings;
			jolokia.AgentVersion = await jolokia.GetVersionAsync(token).ConfigureAwait(false);
			return jolokia;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="baseUri"></param>
		private Jolokia(Uri baseUri)
		{
			_baseUri = baseUri;
		}

		/// <summary>
		/// Returns the Jolokia Agent-Version
		/// </summary>
		public string AgentVersion
		{
			get;
			private set;
		}

		/// <summary>
		/// Get all the domains, with their mbeans
		/// </summary>
		public IDictionary<string, IDictionary<string, MBean>> Domains
		{
			get;
			private set;
		}

		/// <summary>
		/// Load all the JMX information
		/// </summary>
		/// <returns></returns>
		public async Task RefreshAsync(CancellationToken token = default(CancellationToken))
		{
			AgentVersion = await GetVersionAsync(token).ConfigureAwait(false);
			Domains = await ListAsync(token).ConfigureAwait(false);
			return;
		}

		/// <summary>
		/// Load the list output from Jolokia, and parse it to MBeans and Operations
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task<IDictionary<string, IDictionary<string, MBean>>> ListAsync(CancellationToken token = default(CancellationToken))
		{
			IDictionary<string, IDictionary<string, MBean>> result = new Dictionary<string, IDictionary<string, MBean>>();

			var listUri = _baseUri.AppendSegments("list");
			var jmxInfo = await listUri.GetAsJsonAsync(token: token).ConfigureAwait(false);
			if (jmxInfo.status != 200)
			{
				throw new InvalidOperationException("Status != 200");
			}
			var baseExecUri = _baseUri.AppendSegments("exec");
			var baseReadAttributeUri = _baseUri.AppendSegments("read");
			var baseWriteAttributeUri = _baseUri.AppendSegments("write");
			foreach (var domainItem in (IDictionary<string, dynamic>)jmxInfo.value)
			{
				string domainName = domainItem.Key;
				var mbeans = new Dictionary<string, MBean>();
				result.Add(domainName, mbeans);
				foreach (var mbeanItem in (IDictionary<string, dynamic>)domainItem.Value)
				{
					var mbean = new MBean
					{
						Name = mbeanItem.Key,
						Domain = domainName,
						Description = mbeanItem.Value.desc
					};
					var mbeanBaseExecUri = baseExecUri.AppendSegments(mbean.FullyqualifiedName);
					var mbeanBaseReadAttributeUri = baseReadAttributeUri.AppendSegments(mbean.FullyqualifiedName);
					var mbeanBaseWriteAttributeUri = baseWriteAttributeUri.AppendSegments(mbean.FullyqualifiedName);

					// Build attributes
					var attributes = (IDictionary<string, dynamic>)mbeanItem.Value.attr;
					if (attributes != null)
					{
						foreach (var attributeItem in attributes)
						{
							var attribute = new Attr
							{
								Name = attributeItem.Key,
								Description = attributeItem.Value.desc,
								Type = attributeItem.Value.desc,
								IsReadonly = attributeItem.Value.rw == false,
								ReadUri = mbeanBaseReadAttributeUri.AppendSegments(attributeItem.Key),
								WriteUri = mbeanBaseWriteAttributeUri.AppendSegments(attributeItem.Key)
							};
                            mbean.Attributes.Add(attribute.Name, attribute);
						}
					}

					// Build operations
					var operations = (IDictionary<string, dynamic>)mbeanItem.Value.op;
					if (operations != null)
					{
						foreach (var operationItem in operations)
						{
							if (operationItem.Value.GetType() == typeof(JsonArray))
							{
								// Skip overloaded for now
								continue;
							}
							var operation = new Operation
							{
								Name = operationItem.Key,
								Description = operationItem.Value.desc,
								ReturnType = operationItem.Value.ret,
								ExecuteUri = mbeanBaseExecUri.AppendSegments(operationItem.Key)
							};
							// Arguments
							foreach (var argumentItem in (ICollection<dynamic>)operationItem.Value.args)
							{
								operation.Arguments.Add(new Argument
								{
									Name = argumentItem.name,
									Description = argumentItem.desc,
									Type = argumentItem.type
								});
							}
							mbean.Operations.Add(operation.Name, operation);
						}
					}
					mbeans.Add(mbean.FullyqualifiedName, mbean);
				}
			}
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task<string> GetVersionAsync(CancellationToken token = default(CancellationToken))
		{
			var versionUri = _baseUri.AppendSegments("version");
			var versionResult = await versionUri.GetAsJsonAsync(token: token).ConfigureAwait(false);
			return versionResult.value.agent;
		}
	}
}
