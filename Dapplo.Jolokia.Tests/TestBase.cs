using System;
using Dapplo.Log;
using Xunit.Abstractions;
using Dapplo.HttpExtensions.ContentConverter;
using Dapplo.Log.XUnit;

namespace Dapplo.Jolokia.Tests
{
    /// <summary>
    /// Abstract base class for tests
    /// </summary>
    public abstract class TestBase
    {
        protected static readonly LogSource Log = new LogSource();

        /// <summary>
        ///     The instance of the JolokiaClient
        /// </summary>
        protected IJolokiaClient Client { get; }

        protected string TestUri => Environment.GetEnvironmentVariable("jolokia_test_uri");
        protected string Username => Environment.GetEnvironmentVariable("jolokia_test_username");
        protected string Password => Environment.GetEnvironmentVariable("jolokia_test_password");

        /// <summary>
        ///     Default test setup, can also take care of setting the authentication
        /// </summary>
        /// <param name="testOutputHelper">ITestOutputHelper</param>
        /// <param name="doLogin">bool which specifies if a login should be made</param>
        protected TestBase(ITestOutputHelper testOutputHelper, bool doLogin = true)
        {
            // For tests, make sure we get everything
            DefaultJsonHttpContentConverter.Instance.Value.LogThreshold = 0;

            LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
            Client = JolokiaClient.Create(new Uri(TestUri));

            if (doLogin && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                Log.Verbose().WriteLine("Setting basic authentication for user {0}", Username);
                Client.SetBasicAuthentication(Username, Password);
            }
        }
    }
}
