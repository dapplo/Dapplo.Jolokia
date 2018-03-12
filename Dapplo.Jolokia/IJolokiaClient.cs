using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.HttpExtensions;
using Dapplo.Jolokia.Entities;

namespace Dapplo.Jolokia
{
    /// <summary>
    /// The public interface for the Jolokia client
    /// </summary>
    public interface IJolokiaClient
    {
        /// <summary>
        /// The IHttpBehaviour
        /// </summary>
        IHttpBehaviour Behaviour { get; }

        /// <summary>
        /// The base URI for Jolokia
        /// </summary>
        Uri BaseUri { get; }

        /// <summary>
        /// Get all the domains, with their mbeans
        /// </summary>
        IDictionary<string, IDictionary<string, MBean>> Domains { get; }

        /// <summary>
        ///     Set Basic Authentication for the current client
        /// </summary>
        /// <param name="user">username</param>
        /// <param name="password">password</param>
        void SetBasicAuthentication(string user, string password);

        /// <summary>
        /// Load all the JMX information
        /// </summary>
        Task RefreshAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Load the list output from Jolokia, and parse it to MBeans and Operations
        /// </summary>
        /// <param name="domainPath">domain to load, null if all</param>
        /// <param name="mbeanPath">Mbean to load, null if all</param>
        /// <param name="cancellationToken"></param>
        Task LoadListAsync(string domainPath = null, string mbeanPath = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Load (or reload) the Jolokia Agent Version
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task<string> LoadVersionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Reset and turn of history
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task ResetHistoryEntriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute the operation
        /// </summary>
        /// <param name="operation">Operation to execute</param>
        /// <param name="arguments">Array of strings for the arguments, check the arguments for what needs to be passed</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>result with string, check the return type for what can be returned</returns>
        Task<TResult> ExecuteAsync<TResult>(MBeanOperation operation, IEnumerable<string> arguments, CancellationToken cancellationToken = default);

        /// <summary>
        /// Set a history for the operation
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="count">Length of history</param>
        /// <param name="seconds">seconds to keep elements</param>
        /// <param name="cancellationToken">CancellationToken</param>
        Task EnableHistoryAsync(MBeanOperation operation, int count, int seconds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Read the attribute
        /// </summary>
        /// <typeparam name="TValue">the type to which the value of the attribute is deserialized to. Could be dynamic if you don't know what it is</typeparam>
        /// <param name="attribute">Attr</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>TValue</returns>
        Task<TValue> ReadAsync<TValue>(MBeanAttribute attribute, CancellationToken cancellationToken = default);

        /// <summary>
        /// Write the attribute
        /// </summary>
        /// <param name="attribute">Attr</param>
        /// <param name="value">string with the </param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>dynamic, check the Type for what it is</returns>
        Task<object> WriteAsync(MBeanAttribute attribute, string value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Set a history for the attribute
        /// </summary>
        /// <param name="attribute">Attr</param>
        /// <param name="count">Length of history</param>
        /// <param name="seconds">seconds to keep elements</param>
        /// <param name="cancellationToken">CancellationToken</param>
        Task EnableHistoryAsync(MBeanAttribute attribute, int count, int seconds, CancellationToken cancellationToken = default);
    }
}