using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Dapplo.CaliburnMicro;
using Dapplo.CaliburnMicro.Extensions;
using Dapplo.Jolokia.Entities;
using Dapplo.Jolokia.Ui.Configuration;
using Dapplo.Jolokia.Ui.Entities;
using Dapplo.Log;
using LiveCharts;
using MahApps.Metro.Controls.Dialogs;

namespace Dapplo.Jolokia.Ui.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel : Screen, IShell
    {
        private static readonly LogSource Log = new LogSource();
        private JolokiaClient _jolokia;
        private MBeanAttribute _heapMemoryUsageAttribute;
        private MBeanAttribute _nonHeapMemoryUsageAttribute;
        private MBeanOperation _garbageCollectOperation;

        [Import]
        public IJolokiaConfiguration JolokiaConfiguration { get; set; }

        /// <summary>
        ///     Used to make it possible to show a MahApps dialog
        /// </summary>
        [Import]
        private IDialogCoordinator Dialogcoordinator { get; set; }

        public ChartValues<double> HeapMemoryValues { get; set; } = new ChartValues<double>();

        public ChartValues<double> NonHeapMemoryValues { get; set; } = new ChartValues<double>();

        protected override void OnActivate()
        {
            base.OnActivate();
            JolokiaConfiguration.OnPropertyChanged(nameof(IJolokiaConfiguration.JolokiaUri)).Subscribe(args => Log.Info().WriteLine(args.PropertyName));

            JolokiaConfiguration.OnPropertyChanged(nameof(IJolokiaConfiguration.JolokiaUri))
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(DispatcherScheduler.Current)
                .Subscribe(args =>
                {
                    UpdateCanConnect();
                });
            UpdateCanConnect();
        }

        private void UpdateCanConnect(bool disable = false)
        {
            if (disable)
            {
                CanConnect = false;
            }
            else
            {
                CanConnect = _jolokia == null && JolokiaConfiguration.JolokiaUri != null && JolokiaConfiguration.JolokiaUri?.IsAbsoluteUri == true && JolokiaConfiguration.JolokiaUri?.IsFile != true;
            }
            NotifyOfPropertyChange(nameof(CanConnect));
        }

        /// <summary>
        /// Describes if the connect can be pressed
        /// </summary>
        public bool CanConnect { get; private set; }

        /// <summary>
        /// Connect to the Jolokia server, with the given Jolokia URL
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            UpdateCanConnect(true);
            try
            {
                _jolokia = JolokiaClient.Create(JolokiaConfiguration.JolokiaUri);
            }
            catch (Exception ex)
            {
                // show the error message
                await Dialogcoordinator.ShowMessageAsync(this, "Error", ex.Message, MessageDialogStyle.AffirmativeAndNegative);
                // Enable the connect button again
                UpdateCanConnect();
                return;
            }
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


            await ReadValuesAsync();
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(10))
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(DispatcherScheduler.Current)
                .Subscribe(async tick => await ReadValuesAsync());

            CanGarbageCollect = true;
            NotifyOfPropertyChange(nameof(CanGarbageCollect));
        }

        private async Task ReadValuesAsync()
        {
            double usedHeap = 0;
            double usedNonHeap = 0;
            try
            {
                var heapMemoryUsage = await _jolokia.ReadAsync<MemoryUsage>(_heapMemoryUsageAttribute);
                var nonHeapMemoryUsage = await _jolokia.ReadAsync<MemoryUsage>(_nonHeapMemoryUsageAttribute);
                usedNonHeap = nonHeapMemoryUsage.Used;
                usedHeap = heapMemoryUsage.Used;
                Log.Info().WriteLine("heapMemoryUsage: {0}, nonHeapMemoryUsage: {1}", usedHeap, usedNonHeap);
            }
            catch (Exception ex)
            {
                // Ignore
                Log.Error().WriteLine(ex, "Problem retrieving heap usage");
            }
            HeapMemoryValues.Add(usedHeap);
            if (HeapMemoryValues.Count > 10)
            {
                HeapMemoryValues.RemoveAt(0);
            }
            NonHeapMemoryValues.Add(usedNonHeap);
            if (NonHeapMemoryValues.Count > 10)
            {
                NonHeapMemoryValues.RemoveAt(0);
            }
        }

        /// <summary>
        /// Describes if the GarbageCollect can be executed
        /// </summary>
        public bool CanGarbageCollect { get; private set; }

        /// <summary>
        /// Call a garbage collect
        /// </summary>
        public async Task GarbageCollect()
        {
            CanGarbageCollect = false;
            NotifyOfPropertyChange(nameof(CanGarbageCollect));
            await _jolokia.ExecuteAsync<string>(_garbageCollectOperation, Enumerable.Empty<string>());
            CanGarbageCollect = true;
        }
    }
}
