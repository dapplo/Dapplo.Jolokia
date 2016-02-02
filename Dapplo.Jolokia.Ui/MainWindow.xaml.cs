/*
	Dapplo - building blocks for desktop applications
	Copyright (C) 2015-2016 Dapplo

	For more information see: http://dapplo.net/
	Dapplo repositories are hosted on GitHub: https://github.com/dapplo

	This file is part of Dapplo.Jolokia.

	Dapplo.Jolokia is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	Dapplo.Jolokia is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with Dapplo.Jolokia. If not, see <http://www.gnu.org/licenses/>.
 */

using Dapplo.Jolokia.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;
using LiveCharts;
using Dapplo.LogFacade;
using Dapplo.LogFacade.Loggers;
using Dapplo.Jolokia.Ui.Entities;

namespace Dapplo.Jolokia.Ui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private static readonly LogSource Log = new LogSource();
		private DispatcherTimer _timer;
		private Attr _heapMemoryUsageAttribute;
		private Attr _nonHeapMemoryUsageAttribute;
		private Operation _garbageCollectOperation;
		private LineSeries _heapMemorySerie;
		private LineSeries _nonHeapMemorySerie;
		private Jolokia _jolokia;

		public MainWindow()
		{
			LogSettings.Logger = new TraceLogger();
			InitializeComponent();
			LineChart.Series = new SeriesCollection();
			LineChart.AxisY.LabelFormatter = x => string.Format("{0:0.##} MB", x > 0 ? ((x / 1024) / 1024) : 0);
		}

		private async void GC_Button_Click(object sender, RoutedEventArgs e)
		{
			GCButton.IsEnabled = false;

			await _jolokia.Execute(_garbageCollectOperation, null);
			GCButton.IsEnabled = true;
		}

		private void Button_Close_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private async void Connect_Click(object sender, RoutedEventArgs e)
		{
			ConnectButton.IsEnabled = false;
            try
			{
				_jolokia = await Jolokia.Create(new Uri(JolokiaUri.Text));

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				ConnectButton.IsEnabled = true;
				return;
			}
            JolokiaUri.IsEnabled = false;
			await _jolokia.LoadListAsync("java.lang", "type=Memory");

			var javaLangDomain = _jolokia.Domains["java.lang"];
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

			LineChart.AxisY.MaxValue = 500;
			LineChart.AxisY.MinValue = 0;

			_heapMemorySerie = new LineSeries
			{	
				Values = new ChartValues<double>(),
				Title = "Heap memory"
			};
			_heapMemorySerie.Values.Add(0d);
			_nonHeapMemorySerie = new LineSeries {
				Values = new ChartValues<double>(),
				Title = "Non Heap memory"
			};
			_nonHeapMemorySerie.Values.Add(0d);

			LineChart.Series.Add(_heapMemorySerie);
			LineChart.Series.Add(_nonHeapMemorySerie);

			await ReadValuesAsync();
			_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
			_timer.Tick += async (tickSender, args) =>
			{
				await Dispatcher.Invoke(async () => { await ReadValuesAsync(); });
            };
			_timer.Start();
			GCButton.IsEnabled = true;
		}

		private async Task ReadValuesAsync()
		{
			double usedHeap = 0;
			double usedNonHeap = 0;
			try
			{
				var heapMemoryUsage = await _jolokia.Read<MemoryUsage>(_heapMemoryUsageAttribute);
				var nonHeapMemoryUsage = await _jolokia.Read<MemoryUsage>(_nonHeapMemoryUsageAttribute);
				usedNonHeap = nonHeapMemoryUsage.Used;
				usedHeap = heapMemoryUsage.Used;
				Log.Info().WriteLine("heapMemoryUsage: {0}, nonHeapMemoryUsage: {1}", usedHeap, usedNonHeap);
				LineChart.AxisY.MaxValue = Math.Max(usedHeap, usedNonHeap);
			}
			catch (Exception ex)
			{
				// Ignore
				Log.Error().WriteLine(ex, "Problem retrieving heap usage");
			}
			_heapMemorySerie.Values.Add(usedHeap);
			if (_heapMemorySerie.Values.Count > 10)
			{
				_heapMemorySerie.Values.RemoveAt(0);
			}
			_nonHeapMemorySerie.Values.Add(usedNonHeap);
			if (_nonHeapMemorySerie.Values.Count > 10)
			{
				_nonHeapMemorySerie.Values.RemoveAt(0);
			}
		}
	}
}
