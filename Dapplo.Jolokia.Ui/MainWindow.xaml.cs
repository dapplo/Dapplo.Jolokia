/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) Dapplo 2015-2016
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
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using LiveCharts;

namespace Dapplo.Jolokia.Ui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private DispatcherTimer _timer;
		private Attr _heapMemoryUsageAttribute;
		private Attr _nonHeapMemoryUsageAttribute;
		private Operation _garbageCollectOperation;
		private LineSeries _heapMemorySerie;
		private LineSeries _nonHeapMemorySerie;

		public MainWindow()
		{
			InitializeComponent();
			LineChart.Series = new ObservableCollection<Series>();
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
			await jolokia.LoadListAsync("java.lang", "type=Memory");

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
			_nonHeapMemoryUsageAttribute = (from attribute in memoryMBean.Attributes
										 where attribute.Key == "NonHeapMemoryUsage"
										 select attribute.Value).First();

			var initialHeapMemoryUsage = await _heapMemoryUsageAttribute.Read();
			var initialNonHeapMemoryUsage = await _nonHeapMemoryUsageAttribute.Read();
			LineChart.PrimaryAxis.MaxValue = Math.Max(initialHeapMemoryUsage.max, initialNonHeapMemoryUsage.max);
			LineChart.PrimaryAxis.MinValue = 0;



			_heapMemorySerie = new LineSeries
			{
				PrimaryValues = new ObservableCollection<double>
					{
						initialHeapMemoryUsage.used, initialHeapMemoryUsage.used
					}
			};
			_nonHeapMemorySerie = new LineSeries {
				PrimaryValues = new ObservableCollection<double>
					{
						initialNonHeapMemoryUsage.used, initialNonHeapMemoryUsage.used
					}
			};

			LineChart.Series = new ObservableCollection<Series>
			{
				_heapMemorySerie, _nonHeapMemorySerie
			};

			_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
			_timer.Tick += async (tickSender, args) =>
			{
				double usedHeap = 0;
				double usedNonHeap = 0;
				try
				{
					var heapMemoryUsage = await _heapMemoryUsageAttribute.Read();
					var nonHeapMemoryUsage = await _nonHeapMemoryUsageAttribute.Read();
					usedNonHeap = nonHeapMemoryUsage.used;
                    usedHeap = heapMemoryUsage.used;
				} catch
				{
					// Ignore
				}
				_heapMemorySerie.PrimaryValues.Add(usedHeap);
				if (_heapMemorySerie.PrimaryValues.Count > 10)
				{
					_heapMemorySerie.PrimaryValues.RemoveAt(0);
				}
				_nonHeapMemorySerie.PrimaryValues.Add(usedNonHeap);
				if (_nonHeapMemorySerie.PrimaryValues.Count > 10) {
					_nonHeapMemorySerie.PrimaryValues.RemoveAt(0);
				}
			};
			_timer.Start();
			GCButton.IsEnabled = true;
		}
	}
}
