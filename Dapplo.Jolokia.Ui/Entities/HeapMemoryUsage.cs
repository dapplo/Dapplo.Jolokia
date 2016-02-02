using System.Runtime.Serialization;

namespace Dapplo.Jolokia.Ui.Entities
{
	[DataContract]
	public class MemoryUsage
	{
		[DataMember(Name = "max")]
		public long Max { get; set; }
		[DataMember(Name = "committed")]
		public long Committed { get; set; }
		[DataMember(Name = "init")]
		public long Init { get; set; }
		[DataMember(Name = "used")]
		public long Used { get; set; }
	}
}
