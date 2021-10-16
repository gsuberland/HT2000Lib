using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;


namespace HT2000Lib
{
    public class HT2000ManagerLibUsb : IDisposable
    {
        private const ushort VendorID = 0x10c4;
        private const ushort ProductID = 0x82cd;

        private const int SensorReportID = 5;
        private const int SensorReportMinSize = 24;
        private const int SensorReportTemperatureOffset = 6;
        private const int SensorReportHumidityOffset = 8;
        private const int SensorReportCO2Offset = 23;

        private LibUsbDevice _device;

        public HT2000ManagerLibUsb()
        {
            var finder = new UsbDeviceFinder(VendorID, ProductID);

            foreach (LibUsbRegistry registry in LibUsbRegistry.DeviceList)
            {
                if (registry.Vid == VendorID && registry.Pid == ProductID)
                {
                    if (!LibUsbDevice.Open(registry.DevicePath, out _device))
                    {
                        continue;
                    }
                    break;
                }
            }

            if (_device == null)
            {
                throw new Exception("Could not find device.");
            }
            if (!_device.ResetDevice())
            {
                throw new Exception("Could not reset device.");
            }
            if (!_device.Open())
            {
                throw new Exception("Could not open device.");
            }
            /*if (!_device.ClaimInterface(0))
            {
                throw new Exception("Could not claim interface.");
            }*/
        }

        public HT2000State GetState()
        {
            var setupPacket = new UsbSetupPacket(
                (byte)(UsbCtrlFlags.Direction_In | UsbCtrlFlags.RequestType_Class | UsbCtrlFlags.Recipient_Interface),
                (byte)0x01,
                0x0105,
                0x0,
                64);

            IntPtr memory = Marshal.AllocHGlobal(8192);
            try
            {
                int rb = 0;
                if (!_device.ControlTransfer(ref setupPacket, memory, 64, out rb))
                {
                    return null;
                }
                if (rb < SensorReportMinSize)
                {
                    return null;
                }

                byte[] buffer = new byte[rb];
                Marshal.Copy(memory, buffer, 0, rb);

                int temperatureRaw =
                        (buffer[SensorReportTemperatureOffset] * 256)
                        + buffer[SensorReportTemperatureOffset + 1];
                double temperature = (temperatureRaw - 400.0) / 10.0;

                int humidityRaw =
                    (buffer[SensorReportHumidityOffset] * 256)
                    + buffer[SensorReportHumidityOffset + 1];
                double humidity = humidityRaw / 10.0;

                int co2level =
                    (buffer[SensorReportCO2Offset] * 256)
                    + buffer[SensorReportCO2Offset + 1];

                //var newState = new HT2000State(co2level, temperature, humidity);
                //return newState;
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(memory);
            }
        }

        public void Dispose()
        {

        }
    }
}
