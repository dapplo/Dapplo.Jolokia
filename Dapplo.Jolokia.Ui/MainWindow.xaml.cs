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

using Dapplo.Jolokia.Model;
using LiveCharts.Series;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Dapplo.Jolokia.Ui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private DispatcherTimer _timer;
		private Attr _heapMemoryUsageAttribute;
		private Operation _garbageCollectOperation;
		private LineSerie _memorySerie;

		public MainWindow()
		{
			InitializeComponent();
			LineChart.PrimaryAxis.LabelFormatter = x => string.Format("{0:0.##} MB", x > 0 ? ((x / 1024) / 1024) : 0);
		}

		private async void GC_Button_Click(object sender, RoutedEventArgs e)
		{
			GCButton.IsEnabled = false;
			await _garbageCollectOperation.Execute(null);
			GCButton.IsEnabled = true;
		}

		private void Button_Close_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private async void Connect_Click(object sender, RoutedEventArgs e)
		{
			ConnectButton.IsEnabled = false;
			Jolokia jolokia;
            try
			{
				jolokia = await Jolokia.Create(new Uri(JolokiaUri.Text));

			} catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				ConnectButton.IsEnabled = true;
				return;
			}
            JolokiaUri.IsEnabled = false;
			await jolokia.RefreshAsync();

			var javaLangDomain = jolokia.Domains["java.lang"];
			var memoryMBean = (from mbean in javaLangDomain.Values
							   where mbean.Name == "type=Memory"
							   select mbean).First();

			_garbageCollectOperation = (from operation in memoryMBean.Operations
										where operation.Key == "gc"
										select operation.Value).First();
			_heapMemoryUsageAttribute = (from attribute in memoryMBean.Attributes
										 where attribute.Key == "HeapMemoryUsage"
										 select attribute.Value).First();

			var initialHeapMemoryUsage = await _heapMemoryUsageAttribute.Read();

			LineChart.PrimaryAxis.MaxValue = initialHeapMemoryUsage.max;
			LineChart.PrimaryAxis.MinValue = 0;
			_memorySerie = new LineSerie
			{
				PrimaryValues = new ObservableCollection<double>
					{
						initialHeapMemoryUsage.used, initialHeapMemoryUsage.used
					}
			};

			LineChart.Series = new ObservableCollection<Serie>
			{
				_memorySerie
			};

			_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
			_timer.Tick += async (tickSender, args) =>
			{
				var heapMemoryUsage = await _heapMemoryUsageAttribute.Read();
				LineChart.PrimaryAxis.MaxValue = initialHeapMemoryUsage.max;
				_memorySerie.PrimaryValues.Add(heapMemoryUsage.used);
				if (_memorySerie.PrimaryValues.Count > 10)
				{
					_memorySerie.PrimaryValues.RemoveAt(0);
				}
			};
			_timer.Start();
			GCButton.IsEnabled = true;
		}
	}
}
