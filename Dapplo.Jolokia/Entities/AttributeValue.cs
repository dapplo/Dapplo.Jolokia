using System.Runtime.Serialization;

namespace Dapplo.Jolokia.Entities
{
	/// <summary>
	/// Result container for reading attributes
	/// </summary>
	[DataContract]
	public class AttributeValue<TValue>
	{
		[DataMember(Name = "timestamp")]
		public long Timestamp { get; set; }
		[DataMember(Name = "status")]
		public int Status { get; set; }
		[DataMember(Name = "value")]
		public TValue Value { get; set; }
	}
}
