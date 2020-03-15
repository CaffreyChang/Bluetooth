using System;
using System.Collections.Generic;
using System.Text;

namespace App.Bluetooth
{
    /// <summary>
    /// Information about a BLE device
    /// </summary>
    public  class DnaBluetoothLEDevice
    {
        #region Public Properties
        /// <summary>
        /// the time of broadcast adertisement message of the device
        /// </summary>  
        public DateTimeOffset BroadcastTime { get; }
        /// <summary>
        ///the address of the device  
        /// </summary>
        public ulong Address { get; }
        /// <summary>
        /// the name of device
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// the signal strength in db
        /// </summary>
        public short SignalStrengthInDB { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// default constructor
        /// </summary>
        public DnaBluetoothLEDevice(string name,ulong address,short rssi,DateTimeOffset braodcastTime)
        {
            Address = address;
            Name = name;
            SignalStrengthInDB = rssi;
            BroadcastTime = braodcastTime;
        }
        #endregion

        /// <summary>
        /// User friendly to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{((string.IsNullOrEmpty(Name)) ? "[No Name]" : Name)} {Address} ({SignalStrengthInDB})";
        }
    }
}
