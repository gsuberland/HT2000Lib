using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device.Net;
using Hid.Net;
using Hid.Net.Windows;


namespace HT2000Lib
{
    public class HT2000ManagerDevNet : IDisposable
    {
        private const ushort VendorID = 0x10c4;
        private const ushort ProductID = 0x82cd;

        private const int SensorReportID = 5;
        private const int SensorReportMinSize = 24;
        private const int SensorReportTemperatureOffset = 6;
        private const int SensorReportHumidityOffset = 8;
        private const int SensorReportCO2Offset = 23;

        WindowsHidDevice _device = null;

        public HT2000ManagerDevNet()
        {
        }

        public bool OpenDevice()
        {
            WindowsHidDeviceFactory.Register(null, null);
            var getDeviceTask = DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null);
            getDeviceTask.Wait();
            var devices = getDeviceTask.Result;
            ConnectedDeviceDefinition definition = null;
            foreach (var deviceDefinition in devices)
            {
                if (deviceDefinition.DeviceType == DeviceType.Hid)
                {
                    if (deviceDefinition.VendorId == VendorID && deviceDefinition.ProductId == ProductID)
                    {
                        definition = deviceDefinition;
                        break;
                    }
                }
            }
            if (definition == null)
            {
                return false;
            }

            _device = new WindowsHidDevice(definition.DeviceId);
            var initTask = _device.InitializeAsync();
            initTask.Wait();
            return true;
        }

        public HT2000State GetState()
        {
            _device.DefaultReportId = 5;
            var reportTask = _device.ReadReportAsync();
            reportTask.Wait();
            var report = reportTask.Result;

            int temperatureRaw =
                        (report.Data[SensorReportTemperatureOffset] * 256)
                        + report.Data[SensorReportTemperatureOffset + 1];
            double temperature = (temperatureRaw - 400.0) / 10.0;

            int humidityRaw =
                (report.Data[SensorReportHumidityOffset] * 256)
                + report.Data[SensorReportHumidityOffset + 1];
            double humidity = humidityRaw / 10.0;

            int co2level =
                (report.Data[SensorReportCO2Offset] * 256)
                + report.Data[SensorReportCO2Offset + 1];


            //var newState = new HT2000State(co2level, temperature, humidity);
            //return newState;
            return null;
        }

        public void Dispose()
        {
            _device.Dispose();
        }
    }
}
