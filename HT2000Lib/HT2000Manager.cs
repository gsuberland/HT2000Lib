using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;

namespace HT2000Lib
{
    public class HT2000Manager : IDisposable
    {
        private const ushort VendorID = 0x10c4;
        private const ushort ProductID = 0x82cd;

        private const int SensorReportID = 5;
        private const int SensorReportMinSize = 24;
        private const int SensorReportTemperatureOffset = 6;
        private const int SensorReportHumidityOffset = 8;
        private const int SensorReportCO2Offset = 23;

        private HidDevice _device;
        private bool _attached = false;
        private bool _connectedToDriver = false;
        private HT2000State _prevState;
        private bool _disposed = false;

        public bool IsDisposed { get => _disposed; }

        /// <summary>
        /// Occurs when a HT2000 device is attached.
        /// </summary>
        public event EventHandler DeviceAttached;

        /// <summary>
        /// Occurs when a HT2000 device is removed.
        /// </summary>
        public event EventHandler DeviceRemoved;

        /// <summary>
        /// Occurs when HT2000 state has changed.
        /// </summary>
        public event EventHandler<HT2000EventArgs> StateChanged;

        public HT2000Manager()
        {
            _prevState = new HT2000State();
        }

        public bool OpenDevice()
        {
            _device = HidDevices.Enumerate(VendorID, ProductID).FirstOrDefault(d => d.IsConnected);
            if (_device != null)
            {
                _connectedToDriver = true;
                _device.OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped, ShareMode.Exclusive);
                _device.Inserted += DeviceAttachedHandler;
                _device.Removed += DeviceRemovedHandler;
                _device.MonitorDeviceEvents = true;
                //_device.ReadReport(DeviceReportHandler);
                return true;
            }

            return false;
        }

        public void CloseDevice()
        {
            _device.CloseDevice();
            _connectedToDriver = false;
        }

        private void DeviceAttachedHandler()
        {
            _attached = true;
            DeviceAttached?.Invoke(this, EventArgs.Empty);

            _device.ReadReport(DeviceReportHandler);
        }

        private void DeviceRemovedHandler()
        {
            _attached = false;
            DeviceRemoved?.Invoke(this, EventArgs.Empty);
        }

        public HT2000State GetStateDirect()
        {
            var deviceData = _device.Read();
            if (deviceData.Status != HidDeviceData.ReadStatus.Success)
            {
                return null;
            }

            int temperatureRaw =
                        (deviceData.Data[SensorReportTemperatureOffset] * 256)
                        + deviceData.Data[SensorReportTemperatureOffset + 1];
            double temperature = (temperatureRaw - 400.0) / 10.0;

            int humidityRaw =
                (deviceData.Data[SensorReportHumidityOffset] * 256)
                + deviceData.Data[SensorReportHumidityOffset + 1];
            double humidity = humidityRaw / 10.0;

            int co2level =
                (deviceData.Data[SensorReportCO2Offset] * 256)
                + deviceData.Data[SensorReportCO2Offset + 1];

            //var newState = new HT2000State(co2level, temperature, humidity);
            //return newState;
            return null;
        }

        public HT2000State GetFeatureState()
        {
            //bool ok = _device.WriteReport(new HidReport(4, new HidDeviceData(new byte[] { 0x05, 0xFF, 0xFF, 0xFF }, HidDeviceData.ReadStatus.NoDataRead)));
            byte[] reportData;
            if (!_device.ReadFeatureData(out reportData))
            {
                return null;
            }
            if (reportData.Length < SensorReportMinSize)
            {
                return null;
            }
            int temperatureRaw =
                        (reportData[SensorReportTemperatureOffset] * 256)
                        + reportData[SensorReportTemperatureOffset + 1];
            double temperature = (temperatureRaw - 400.0) / 10.0;

            int humidityRaw =
                (reportData[SensorReportHumidityOffset] * 256)
                + reportData[SensorReportHumidityOffset + 1];
            double humidity = humidityRaw / 10.0;

            int co2level =
                (reportData[SensorReportCO2Offset] * 256)
                + reportData[SensorReportCO2Offset + 1];

            //var newState = new HT2000State(co2level, temperature, humidity);
            /*if (!newState.Equals(_prevState))
            {
                _prevState = newState;
                StateChanged?.Invoke(this, new HT2000EventArgs(newState));
            }*/
            //return newState;
            return null;
        }

        public HT2000State GetState()
        {
            //var report = _device.CreateReport();
            var report = _device.ReadReport();
            if (report.ReportId == SensorReportID)
            {
                if (report.Data.Length >= SensorReportMinSize)
                {
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
            }
            return null;
        }


        private void DeviceReportHandler(HidReport report)
        {
            if (_attached == false)
            {
                return;
            }

            if (report.ReportId == SensorReportID)
            {
                if (report.Data.Length >= SensorReportMinSize)
                {
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
                    HT2000State newState = null; // nulled 'cos errors
                    if (!newState.Equals(_prevState))
                    {
                        _prevState = newState;
                        StateChanged?.Invoke(this, new HT2000EventArgs(newState));
                    }
                }
            }

            _device.ReadReport(DeviceReportHandler);
        }

        /// <summary>
        /// Closes the connection to the device.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Closes any connected devices.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseDevice();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Destroys instance and frees device resources (if not freed already)
        /// </summary>
        ~HT2000Manager()
        {
            Dispose(false);
        }
    }
}
