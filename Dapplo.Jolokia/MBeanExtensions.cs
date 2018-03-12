using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.HttpExtensions;
using Dapplo.Jolokia.Entities;

namespace Dapplo.Jolokia
{
    /// <summary>
    /// Extensions of the IJolokiaClient which handle MBeans
    /// </summary>
    public static class MBeanExtensions
    {
        /// <summary>
        /// Load the list output from Jolokia, and parse it to MBeans and Operations
        /// </summary>
        /// <param name="jolokiaClient">IJolokiaClient</param>
        /// <param name="domainPath">domain for the mbean to find</param>
        /// <param name="mbeanPath">Mbean name to find</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public static async Task<MBean> GetAsync(this IJolokiaClient jolokiaClient, string domainPath, string mbeanPath, CancellationToken cancellationToken = default)
        {
            jolokiaClient.Behaviour.MakeCurrent();

            var listUri = jolokiaClient.BaseUri.AppendSegments("list", domainPath, mbeanPath);
            // No path means we handle a result with domains
            var jmxResponseDomains = await listUri.GetAsAsync<ValueContainer<MBean>>(cancellationToken).ConfigureAwait(false);
            if (jmxResponseDomains.Status != 200)
            {
                throw new InvalidOperationException("Status != 200");
            }

            if (!jolokiaClient.Domains.TryGetValue(domainPath, out var mbeans))
            {
                mbeans = new Dictionary<string, MBean>();
                jolokiaClient.Domains[domainPath] = mbeans;
            }
            var mbean = jmxResponseDomains.Value;
            mbean.Update(domainPath, mbeanPath);
            return mbean;
        }
    }
}
