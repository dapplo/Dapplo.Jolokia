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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.HttpExtensions.JsonSimple;

namespace Dapplo.Jolokia
{
    /// <summary>
    /// A simple implementation of a Jolokia API for .NET
    /// </summary>
    public class JolokiaClient : IJolokiaClient
    {
        private string _password;
        private string _user;

        /// <inheritdoc />
        public Uri BaseUri { get; }

        /// <summary>
        /// The IHttpBehaviour
        /// </summary>
        public IHttpBehaviour Behaviour { get; }

        /// <summary>
        /// Create a Jolokia client
        /// </summary>
        /// <param name="jolokiaUri">Base-uri to the jolokia server (normally ends with jolokia)</param>
        /// <param name="httpSettings">IHttpSettings or null for default</param>
        /// <returns>Jolokia</returns>
        public static JolokiaClient Create(Uri jolokiaUri, IHttpSettings httpSettings = null)
        {
            return  new JolokiaClient(jolokiaUri, httpSettings);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUri">Base URL, e.g. https://yourjolokiaserver/jolokia</param>
        /// <param name="httpSettings">IHttpSettings or null for default</param>
        private JolokiaClient(Uri baseUri, IHttpSettings httpSettings = null)
        {
            // Make sure Jolokia answers with json, see Release 1.0.2 – 2012-01-06: https://jolokia.org/changes-report.html
            BaseUri = baseUri.ExtendQuery("mimeType", "application/json");
            Behaviour = ConfigureBehaviour(new HttpBehaviour(), httpSettings);
        }

        /// <summary>
        ///     Helper method to configure the IChangeableHttpBehaviour
        /// </summary>
        /// <param name="behaviour">IChangeableHttpBehaviour</param>
        /// <param name="httpSettings">IHttpSettings</param>
        /// <returns>the behaviour, but configured as IHttpBehaviour </returns>
        private IHttpBehaviour ConfigureBehaviour(IChangeableHttpBehaviour behaviour, IHttpSettings httpSettings = null)
        {
            behaviour.HttpSettings = httpSettings ?? HttpExtensionsGlobals.HttpSettings.ShallowClone();

            // Specify the Json Serializer
            behaviour.JsonSerializer = new SimpleJsonSerializer();

            behaviour.OnHttpRequestMessageCreated = httpMessage =>
            {
                if (!string.IsNullOrEmpty(_user) && _password != null)
                {
                    httpMessage?.SetBasicAuthorization(_user, _password);
                }
                return httpMessage;
            };
            return behaviour;
        }

        /// <inheritdoc />
        public void SetBasicAuthentication(string user, string password)
        {
            _user = user;
            _password = password;
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
        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            await LoadVersionAsync(cancellationToken).ConfigureAwait(false);
            await LoadListAsync(null, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Process the MBean information
        /// </summary>
        /// <param name="domainName">Name of the domain</param>
        /// <param name="mbeanName">Name of the mbean</param>
        /// <param name="mbean">MBean</param>
        private void ProcessMBean(string domainName, string mbeanName, MBean mbean)
        {
            mbean.Name = mbeanName;
            mbean.Domain = domainName;

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
            Domains[mbean.Domain][mbeanName] = mbean;
        }


        /// <summary>
        /// Process the MBeans information
        /// </summary>
        /// <param name="domainName">Name of the domain</param>
        /// <param name="mbeans">IDictionary with MBean elements</param>
        private void ProcessMBeans(string domainName, IDictionary<string, MBean> mbeans)
        {
            foreach (var mbeanKey in mbeans.Keys)
            {
                var mbean = mbeans[mbeanKey];
                mbean.Update(domainName, mbeanKey);
                Domains[domainName][mbeanKey] = mbean;
            }
        }

        /// <summary>
        /// Load the list output from Jolokia, and parse it to MBeans and Operations
        /// </summary>
        /// <param name="domainPath">domain to load, null if all</param>
        /// <param name="mbeanPath">Mbean to load, null if all</param>
        /// <param name="cancellationToken"></param>
        public async Task LoadListAsync(string domainPath = null, string mbeanPath = null, CancellationToken cancellationToken = default)
        {
            var listUri = BaseUri.AppendSegments("list", domainPath, mbeanPath);
            Behaviour.MakeCurrent();


            // Check if this is loading a MBean
            if (!string.IsNullOrEmpty(domainPath) && !string.IsNullOrEmpty(mbeanPath))
            {
                // No path means we handle a result with domains
                var jmxResponseDomains = await listUri.GetAsAsync<ValueContainer<MBean>>(cancellationToken).ConfigureAwait(false);
                if (jmxResponseDomains.Status != 200)
                {
                    throw new InvalidOperationException("Status != 200");
                }

                if (!Domains.TryGetValue(domainPath, out var mbeans))
                {
                    mbeans = new Dictionary<string, MBean>();
                    Domains[domainPath] = mbeans;
                }
                ProcessMBean(domainPath, mbeanPath, jmxResponseDomains.Value);
            }
            // Check if this is loading a domain
            else if (!string.IsNullOrEmpty(domainPath))
            {
                // No path means we handle a result with domains
                var jmxResponseDomains = await listUri.GetAsAsync<ValueContainer<IDictionary<string, IDictionary<string, MBean>>>>(cancellationToken).ConfigureAwait(false);
                if (jmxResponseDomains.Status != 200)
                {
                    throw new InvalidOperationException("Status != 200");
                }
                foreach (var domainKey in jmxResponseDomains.Value.Keys)
                {
                    if (!Domains.TryGetValue(domainKey, out var mbeans))
                    {
                        mbeans = new Dictionary<string, MBean>();
                        Domains[domainPath] = mbeans;
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
        public async Task<string> LoadVersionAsync(CancellationToken cancellationToken = default)
        {
            Behaviour.MakeCurrent();
            var versionUri = BaseUri.AppendSegments("version");
            var versionResult = await versionUri.GetAsAsync<ValueContainer<AgentInfo>>(cancellationToken).ConfigureAwait(false);
            return versionResult.Value.Agent;
        }


        /// <summary>
        /// Reset and turn of history
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task ResetHistoryEntriesAsync(CancellationToken cancellationToken = default)
        {
            Behaviour.MakeCurrent();
            var resetHistoryUri = BaseUri.AppendSegments("exec/jolokia:type=Config/resetHistoryEntries");
            await resetHistoryUri.GetAsAsync<dynamic>(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Execute the operation
        /// </summary>
        /// <param name="operation">Operation to execute</param>
        /// <param name="arguments">Array of strings for the arguments, check the arguments for what needs to be passed</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>result with string, check the return type for what can be returned</returns>
        public async Task<TResult> ExecuteAsync<TResult>(MBeanOperation operation, IEnumerable<string> arguments, CancellationToken cancellationToken = default)
        {
            Behaviour.MakeCurrent();

            var args = arguments?.ToList() ?? new List<string>();
            int passedArgumentCount = args.Count;
            int neededArgumentCount = operation.Arguments?.Count ?? 0;
            if (passedArgumentCount != neededArgumentCount)
            {
                throw new ArgumentException($"Passed arguments for operation {operation.Name} do not match.");
            }
            var execUri = BaseUri.AppendSegments("exec", operation.Parent, operation.Name).AppendSegments(args);
            var result = await execUri.GetAsAsync<HttpResponse<ValueContainer<TResult>>>(cancellationToken).ConfigureAwait(false);
            return result.Response.Value;
        }

        /// <summary>
        /// Set a history for the operation
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="count">Length of history</param>
        /// <param name="seconds">seconds to keep elements</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public async Task EnableHistoryAsync(MBeanOperation operation, int count, int seconds, CancellationToken cancellationToken = default)
        {
            Behaviour.MakeCurrent();
            var historyLimitUri = BaseUri.AppendSegments("exec/jolokia:type=Config/setHistoryLimitForOperation", operation.Parent, operation.Name, "[null]", "[null]", count, seconds);
            await historyLimitUri.GetAsAsync<dynamic>(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Read the attribute
        /// </summary>
        /// <typeparam name="TValue">the type to which the value of the attribute is deserialized to. Could be dynamic if you don't know what it is</typeparam>
        /// <param name="attribute">Attr</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>TValue</returns>
        public async Task<TValue> ReadAsync<TValue>(MBeanAttribute attribute, CancellationToken cancellationToken = default)
        {
            Behaviour.MakeCurrent();

            var readUri = BaseUri.AppendSegments("read", attribute.Parent, attribute.Name);
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
        public async Task<dynamic> WriteAsync(MBeanAttribute attribute, string value, CancellationToken cancellationToken = default)
        {
            Behaviour.MakeCurrent();
            var writeUri = BaseUri.AppendSegments("write", attribute.Parent, attribute.Name).AppendSegments(value);
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
        public async Task EnableHistoryAsync(MBeanAttribute attribute, int count, int seconds, CancellationToken cancellationToken = default)
        {
            Behaviour.MakeCurrent();
            var historyLimitUri = BaseUri.AppendSegments("exec/jolokia:type=Config/setHistoryLimitForAttribute", attribute.Parent, attribute.Name, "[null]", "[null]", count, seconds);
            await historyLimitUri.GetAsAsync<dynamic>(cancellationToken).ConfigureAwait(false);
        }
    }
}
