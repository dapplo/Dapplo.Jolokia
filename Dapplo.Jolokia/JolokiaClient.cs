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

using Dapplo.HttpExtensions;
using Dapplo.Jolokia.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.HttpExtensions.JsonSimple;

namespace Dapplo.Jolokia
{
    /// <summary>
    /// A simple implementation of a Jolokia API for .NET
    /// </summary>
    public class JolokiaClient
    {
        private readonly Uri _baseUri;

        /// <summary>
        /// The IHttpBehaviour
        /// </summary>
        public IHttpBehaviour Behaviour { get; }

        /// <summary>
        /// Create a Jolokia API object
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="schema"></param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Jolokia</returns>
        public static async Task<JolokiaClient> CreateAsync(string hostname, int port, string schema = "http", string username = null, string password = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var uriBuilder = new UriBuilder(schema, hostname, port, "/jolokia");
            if (username != null)
            {
                uriBuilder.UserName = username;
            }
            if (password != null)
            {
                uriBuilder.Password = password;
            }
            return await CreateAsync(uriBuilder.Uri, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a Jolokia API object
        /// </summary>
        /// <param name="jolokiaUri">Predefined base-uri to the jolokia REST API (normally ends with jolokia)</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Jolokia</returns>
        public static async Task<JolokiaClient> CreateAsync(Uri jolokiaUri, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Make sure Jolokia answers with json, see Release 1.0.2 – 2012-01-06: https://jolokia.org/changes-report.html
            jolokiaUri = jolokiaUri.ExtendQuery("mimeType", "application/json");

            var jolokia = new JolokiaClient(jolokiaUri);
            await jolokia.LoadVersionAsync(cancellationToken).ConfigureAwait(false);
            return jolokia;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUri"></param>
        private JolokiaClient(Uri baseUri)
        {
            _baseUri = baseUri;
            Behaviour = new HttpBehaviour
            {
                JsonSerializer = new SimpleJsonSerializer()
            };
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
        } = new Dictionary<string, IDictionary<string, MBean>>();

        /// <summary>
        /// Load all the JMX information
        /// </summary>
        public async Task RefreshAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await LoadVersionAsync(cancellationToken).ConfigureAwait(false);
            await LoadListAsync(null, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Process the MBean information
        /// </summary>
        /// <param name="domainPath"></param>
        /// <param name="mbeans"></param>
        private void ProcessMBeans(string domainPath, IDictionary<string, MBean> mbeans)
        {
            foreach (var mbeanKey in mbeans.Keys)
            {
                var mbean = mbeans[mbeanKey];
                mbean.Name = mbeanKey;
                mbean.Domain = domainPath;

                // Correct attributes
                if (mbean.Attributes != null)
                {
                    foreach (var attibuteKey in mbean.Attributes.Keys)
                    {
                        var attribute = mbean.Attributes[attibuteKey];
                        attribute.Name = attibuteKey;
                        attribute.Parent = mbean.FullyqualifiedName;
                    }
                }

                // Build operations
                if (mbean.Operations != null)
                {
                    foreach (var openrationKey in mbean.Operations.Keys)
                    {
                        var operation = mbean.Operations[openrationKey];
                        operation.Name = openrationKey;
                        operation.Parent = mbean.FullyqualifiedName;
                    }
                }
            }
        }
        /// <summary>
        /// Load the list output from Jolokia, and parse it to MBeans and Operations
        /// </summary>
        /// <param name="domainPath">domain to load, null if all</param>
        /// <param name="mbeanPath">Mbean to load, null if all</param>
        /// <param name="cancellationToken"></param>
        public async Task LoadListAsync(string domainPath = null, string mbeanPath = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listUri = _baseUri.AppendSegments("list", domainPath, mbeanPath);

            if (!string.IsNullOrEmpty(domainPath))
            {
                // No path means we handle a result with domains
                var jmxResponseDomains = await listUri.GetAsAsync<ValueContainer<IDictionary<string, IDictionary<string, MBean>>>>(cancellationToken).ConfigureAwait(false);
                if (jmxResponseDomains.Status != 200)
                {
                    throw new InvalidOperationException("Status != 200");
                }
                foreach (var domainKey in jmxResponseDomains.Value.Keys)
                {
                    IDictionary<string, MBean> mbeans;
                    if (!Domains.TryGetValue(domainKey, out mbeans))
                    {
                        mbeans = new Dictionary<string, MBean>();
                        Domains.Add(domainKey, mbeans);
                    }
                    ProcessMBeans(domainKey, jmxResponseDomains.Value[domainKey]);
                }
            }
            else
            {
                var jmxResponseMBeans = await listUri.GetAsAsync<ValueContainer<IDictionary<string, MBean>>>(cancellationToken).ConfigureAwait(false);
                if (jmxResponseMBeans.Status != 200)
                {
                    throw new InvalidOperationException("Status != 200");
                }
                ProcessMBeans(domainPath, jmxResponseMBeans.Value);
                Domains[domainPath] = jmxResponseMBeans.Value;
            }
        }

        /// <summary>
        /// Load (or reload) the Jolokia Agent Version
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task<string> LoadVersionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Behaviour.MakeCurrent();
            var versionUri = _baseUri.AppendSegments("version");
            var versionResult = await versionUri.GetAsAsync<ValueContainer<AgentInfo>>(cancellationToken).ConfigureAwait(false);
            AgentVersion = versionResult.Value.Agent;
            return AgentVersion;
        }


        /// <summary>
        /// Reset and turn of history
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task ResetHistoryEntriesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Behaviour.MakeCurrent();
            var resetHistoryUri = _baseUri.AppendSegments("exec/jolokia:type=Config/resetHistoryEntries");
            await resetHistoryUri.GetAsAsync<dynamic>(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Execute the operation
        /// </summary>
        /// <param name="operation">Operation to execute</param>
        /// <param name="arguments">Array of strings for the arguments, check the arguments for what needs to be passed</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>result with string, check the return type for what can be returned</returns>
        public async Task<dynamic> ExecuteAsync(MBeanOperation operation, string[] arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            Behaviour.MakeCurrent();
            int passedArgumentCount = arguments?.Length ?? 0;
            int neededArgumentCount = operation.Arguments?.Count ?? 0;
            if (passedArgumentCount != neededArgumentCount)
            {
                throw new ArgumentException($"Passed arguments for operation {operation.Name} do not match.");
            }
            var execUri = _baseUri.AppendSegments("exec", operation.Parent, operation.Name).AppendSegments(arguments);
            var result = await execUri.GetAsAsync<dynamic>(cancellationToken).ConfigureAwait(false);
            return result.value;
        }

        /// <summary>
        /// Set a history for the operation
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="count">Length of history</param>
        /// <param name="seconds">seconds to keep elements</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public async Task EnableHistoryAsync(MBeanOperation operation, int count, int seconds, CancellationToken cancellationToken = default(CancellationToken))
        {
            Behaviour.MakeCurrent();
            var historyLimitUri = _baseUri.AppendSegments("exec/jolokia:type=Config/setHistoryLimitForOperation", operation.Parent, operation.Name, "[null]", "[null]", count, seconds);
            await historyLimitUri.GetAsAsync<dynamic>(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Read the attribute
        /// </summary>
        /// <typeparam name="TValue">the type to which the value of the attribute is deserialized to. Could be dynamic if you don't know what it is</typeparam>
        /// <param name="attribute">Attr</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>TValue</returns>
        public async Task<TValue> ReadAsync<TValue>(MBeanAttribute attribute, CancellationToken cancellationToken = default(CancellationToken))
        {
            Behaviour.MakeCurrent();

            var readUri = _baseUri.AppendSegments("read", attribute.Parent, attribute.Name);
            var result = await readUri.GetAsAsync<ValueContainer<TValue>>(cancellationToken).ConfigureAwait(false);
            return result.Value;
        }

        /// <summary>
        /// Write the attribute
        /// </summary>
        /// <param name="attribute">Attr</param>
        /// <param name="value">string with the </param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>dynamic, check the Type for what it is</returns>
        public async Task<dynamic> WriteAsync(MBeanAttribute attribute, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Behaviour.MakeCurrent();
            var writeUri = _baseUri.AppendSegments("write", attribute.Parent, attribute.Name).AppendSegments(value);
            var result = await writeUri.GetAsAsync<dynamic>(cancellationToken).ConfigureAwait(false);
            return result.value;
        }

        /// <summary>
        /// Set a history for the attribute
        /// </summary>
        /// <param name="attribute">Attr</param>
        /// <param name="count">Length of history</param>
        /// <param name="seconds">seconds to keep elements</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public async Task EnableHistoryAsync(MBeanAttribute attribute, int count, int seconds, CancellationToken cancellationToken = default(CancellationToken))
        {
            Behaviour.MakeCurrent();
            var historyLimitUri = _baseUri.AppendSegments("exec/jolokia:type=Config/setHistoryLimitForAttribute", attribute.Parent, attribute.Name, "[null]", "[null]", count, seconds);
            await historyLimitUri.GetAsAsync<dynamic>(cancellationToken).ConfigureAwait(false);
        }
    }
}
