using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;

namespace App.Bluetooth
{
    /// <summary>
    /// Wraps and use of the <see cref="BluetoothLEAdvertisementWatcher"/>
    /// for easier consumption
    /// </summary>
    public class DnaBluetoothLEAdvertisementWatcher
    {
        #region Private Members
        /// <summary>
        /// my underlying bluethooth wathcher class
        /// </summary>
        private readonly BluetoothLEAdvertisementWatcher mWatcher;

        /// <summary>
        /// A list of discover devices
        /// </summary>
        private readonly Dictionary<ulong, DnaBluetoothLEDevice> mDiscovereDevices = new Dictionary<ulong, DnaBluetoothLEDevice>();

        /// <summary>
        /// A thread lock of this class
        /// </summary>
        private readonly object mThreadLock = new object();
        #endregion

        #region Public Properties
        /// <summary>
        /// indicate if this watcher is listiening for advertisements
        /// </summary>
        public bool Listiening => mWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;

        /// <summary>
        /// A list of discovered devices
        /// </summary>
        public IReadOnlyCollection<DnaBluetoothLEDevice> DiscoveredDevices
        {
            get
            {
                // Clean up any timeouts
                CleanupTimeouts();
                lock (mThreadLock)
                {
                    // convert readonly list
                    return mDiscovereDevices.Values.ToList().AsReadOnly();
                }
            }
        }

        /// <summary>
        /// The timeout in seconds that a device is removed from the <see cref="mDiscovereDevices"/>
        /// </summary>
        public int HeartbeatTimeout { get; set; } = 30;
        #endregion

        #region Public Event
        /// <summary>
        /// Fired when the bluetooth watcher stops listening 
        /// </summary>
        public event Action StoppedListening = () => { };

        /// <summary>
        /// Fired when the bluetooth watcher starts listening 
        /// </summary>
        public event Action StartedListening = () => { };

        /// <summary>
        /// Fired when a device is discovered
        /// </summary>
        public event Action<DnaBluetoothLEDevice> DeviceDiscovered = (device) => { };

        /// <summary>
        /// Fired when a device name changes
        /// </summary>
        public event Action<DnaBluetoothLEDevice> DeviceNameChanged = (device) => { };

        /// <summary>
        /// Fired when a new device  is discovered
        /// </summary>
        public event Action<DnaBluetoothLEDevice> NewDeviceDiscovered = (device) => { };


        /// <summary>
        /// Fired when a device is removed for timeing out
        /// </summary>
        public event Action<DnaBluetoothLEDevice> DeviceTimeout = (device) => { };

        #endregion

        #region Constructor
        /// <summary>
        /// the default constructor
        /// </summary>
        public DnaBluetoothLEAdvertisementWatcher()
        {
            mWatcher = new BluetoothLEAdvertisementWatcher()
            {
                // Create Bluetooth listener
                ScanningMode = BluetoothLEScanningMode.Active
            };
            // listen out for new advertisements 
            mWatcher.Received += WatcherAdvertisementReceived;

            // listen out for when the wather stop listening
            mWatcher.Stopped += (watcher, e) =>
            {
                // inform listeners
                StoppedListening();
            };
        }

        #endregion

        #region Private Method
        /// <summary>
        /// listen out for watcher advertisements 
        /// </summary>
        /// <param name="sender">the watcher</param>
        /// <param name="args">the arguments</param>
        private void WatcherAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // Clean up any timeouts
            CleanupTimeouts();
            DnaBluetoothLEDevice device = null;

            // Is new disccovery ?
            var newDiscovery = !mDiscovereDevices.ContainsKey(args.BluetoothAddress);

            // Is changed ?
            var nameChanged =
                // If is alredy exists
                !newDiscovery &&
                // and is is not a blank name
                !string.IsNullOrEmpty(args.Advertisement.LocalName) &&
                // And the name is different
                mDiscovereDevices[args.BluetoothAddress].Name != args.Advertisement.LocalName;

            lock (mThreadLock)
            {
                // Get the name of device
                var name = args.Advertisement.LocalName;

                // if new name is blank ,and we already have a device...
                if (string.IsNullOrEmpty(name) && !newDiscovery)
                    // Don't override what cloud ba an actuall name already
                    name = mDiscovereDevices[args.BluetoothAddress].Name;

                // Create new device info class
                device = new DnaBluetoothLEDevice
                (
                    // Name
                    name: name,
                    // Bluetooth Address
                    address: args.BluetoothAddress,
                    //  Signal strenth
                    rssi: args.RawSignalStrengthInDBm,
                    // broadcast time
                    braodcastTime: args.Timestamp
                );

                // Add/Update the  device in the dictionary
                mDiscovereDevices[args.BluetoothAddress] = device;
            }

            // Inform listerner
            DeviceDiscovered(device);

            // If name changed
            if (nameChanged)
                // Inform listener
                DeviceNameChanged(device);


            // If new discovery...
            if (newDiscovery)
                // Inform listener
                NewDeviceDiscovered(device);
        }

        /// <summary>
        /// Prune any time out devices that we have not heard of
        /// </summary>
        private void CleanupTimeouts()
        {
            lock (mThreadLock)
            {
                // the date in time that if less than means a devices has time out
                var threshold = DateTime.UtcNow - TimeSpan.FromSeconds(HeartbeatTimeout);

                // Any device that have not sent a new broadcast within the heartbeat time
                mDiscovereDevices.Where(f => f.Value.BroadcastTime < threshold).ToList().ForEach(device =>
                {
                    // Remove device 
                    mDiscovereDevices.Remove(device.Key);

                    // Inform listener
                    DeviceTimeout(device.Value);
                });
            }

        }
        #endregion

        #region Piblic Method 
        /// <summary>
        /// Start listening for advertisements
        /// </summary>
        public void StartListening()
        {
            lock (mThreadLock)
            {
                // if already listening...
                if (Listiening)
                    // Do nothing more
                    return;

                // Start underlying watcher
                mWatcher.Start();

            }
            // Inform listener
            StartedListening();
        }

        /// <summary>
        ///  Stop listening for advertisements
        /// </summary>
        public void StopListening()
        {
            lock (mThreadLock)
            {
                // if we are no currently listening
                if (!Listiening)
                    //Do nothing more
                    return;

                // stop listening
                mWatcher.Stop();

                // Clear any devices
                mDiscovereDevices.Clear();
            }
        }
        #endregion

    }
}
