using System;

namespace App.Bluetooth.ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            // New Watcher
            Console.WriteLine("Hello World!");
            var watcher = new DnaBluetoothLEAdvertisementWatcher();

            watcher.StartedListening += () =>
            {
                Console.WriteLine("Started Listening");
            };

            watcher.StoppedListening += () =>
            {   
                Console.WriteLine("Stopped  Listening");
            };
            watcher.NewDeviceDiscovered += (device) => 
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"New device: {device }");
            };
            watcher.DeviceNameChanged += (device) => 
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Device name changed {device}");
            };
            watcher.DeviceTimeout += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Device name changed {device}");
            };

            // Start Listening 
            watcher.StartListening();

            while (true)
            {
                // Pause until press enter
                Console.ReadLine();

                // Get discover devices 
                var devices = watcher.DiscoveredDevices;

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{devices.Count} devices...");

                foreach (var device in devices) 
                {
                    Console.WriteLine(device);
                }


            }

            // Don't close
            Console.ReadLine();
        }
    }
}
