using System;
using System.ComponentModel;
using Dapplo.Ini;

namespace Dapplo.Jolokia.Ui.Configuration
{
	[IniSection("Core")]
	public interface IJolokiaConfiguration : IIniSection, INotifyPropertyChanged
	{
		Uri JolokiaUri { get; set; }
	}
}
