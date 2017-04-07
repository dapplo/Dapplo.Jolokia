using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Dapplo.Jolokia.Entities
{
    /// <summary>
    /// Information on the Jolokia Agent
    /// </summary>
    [DataContract]
    public class AgentInfo
    {
        /// <summary>
        /// Version for the protocol
        /// </summary>
        [DataMember(Name = "protocol")]
        public string Protocol { get; set; }

        /// <summary>
        /// Version for the Agent
        /// </summary>
        [DataMember(Name = "agent")]
        public string Agent { get; set; }
    }
}
