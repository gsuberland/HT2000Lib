using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32.SafeHandles;
using Device.Net;
using Hid.Net;
using Hid.Net.Windows;
using System.Runtime.InteropServices;

namespace HT2000Lib
{
    public class HT2000ManagerNative
    {
        private const ushort VendorID = 0x10c4;
        private const ushort ProductID = 0x82cd;

        private const int SensorReportID = 5;
        private const int SensorReportMinSize = 32;

        private string GetDeviceId()
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
                throw new Exception("Failed to find device.");
            }
            return definition.DeviceId;
        }

        public HT2000DataPoint[] DumpData()
        {
            string deviceId = GetDeviceId();

            var dataPoints = new List<HT2000DataPoint>();

            using (SafeFileHandle hDevice = Win32Api.CreateFile(deviceId,
                Win32Api.GENERIC_READ | Win32Api.GENERIC_WRITE,
                0,
                IntPtr.Zero,
                Win32Api.OPEN_EXISTING,
                Win32Api.FILE_FLAG_OVERLAPPED,
                IntPtr.Zero))
            {
                if (hDevice.IsInvalid)
                {
                    Console.WriteLine("Invalid handle.");
                    return null;
                }

                Console.WriteLine("Opened device.");
                Win32Api.HidD_SetNumInputBuffers(hDevice, 512);

                int index = 0;
                while (true)
                {
                    byte[] indexBuffer = new byte[61];
                    indexBuffer[0] = 4; // command to set the read buffer
                    indexBuffer[1] = (byte)((index & 0xFF00) >> 8);
                    indexBuffer[2] = (byte)(index & 0xFF);
                    uint bytesWritten = 0;

                    using (var waitEvent = new System.Threading.ManualResetEvent(false))
                    {
                        var overlapped = new System.Threading.NativeOverlapped();
                        overlapped.EventHandle = waitEvent.SafeWaitHandle.DangerousGetHandle();
                        bool writeOK = Win32Api.WriteFile(hDevice, indexBuffer, (uint)indexBuffer.Length, out bytesWritten, ref overlapped);
                        if (writeOK)
                        {
                            Console.WriteLine("Write OK! Continuing without waiting.");
                        }
                        else
                        {
                            int w32error = Marshal.GetLastWin32Error();
                            if (w32error == 997) // Overlapped IO in progress
                            {

                                Console.WriteLine("Waiting for IO...");
                                if (!waitEvent.WaitOne(5000))
                                {
                                    Console.WriteLine("Error: Data download stalled for 5 seconds.");
                                    return null;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Write fail. Reason: " + w32error);
                            }
                        }
                    }

                    Console.WriteLine("IO completed.");

                    byte[] reportBuffer = new byte[61];
                    reportBuffer[0] = 0x08;
                    if (Win32Api.HidD_GetInputReport(hDevice, reportBuffer, (uint)reportBuffer.Length))
                    {
                        Console.WriteLine("Got input report!");

                        for (int ofs = 1; ofs < reportBuffer.Length - 5; ofs += 5)
                        {
                            Console.WriteLine("Extracting data points...");
                            if (reportBuffer[ofs] == 0xFF)
                            {
                                if (BitConverter.ToUInt32(reportBuffer, ofs + 1) == 0xFFFFFFFFU)
                                {
                                    Console.WriteLine("Final data point.");
                                    break;
                                }
                            }

                            // each data element is five bytes: ab cd ef gh ij

                            // temperature is (fcd - 400.0) / 10.0
                            // humidity is eab / 10.0
                            // co2 is ijgh

                            double temperature = ((((reportBuffer[ofs + 2] & 0xF) << 8) + reportBuffer[ofs + 1]) - 400.0) / 10.0;
                            double humidity = (((reportBuffer[ofs + 2] & 0xF0) << 4) + reportBuffer[ofs]) / 10.0;
                            int co2 = (reportBuffer[ofs + 4] << 8) + reportBuffer[ofs + 3];
                            //Console.WriteLine($"Data Point: Temp = {temperature}, Humidity = {humidity}, CO2 = {co2}");

                            dataPoints.Add(new HT2000DataPoint { Temperature = temperature, Humidity = humidity, CO2 = co2 });
                        }

                        if (BitConverter.ToUInt32(reportBuffer, 57) == 0xFFFFFFFFU)
                        {
                            Console.WriteLine("Final report.");
                            break;
                        }
                    }
                    if (index == UInt16.MaxValue)
                    {
                        Console.WriteLine("Hit max index. Breaking.");
                        break;
                    }
                    index++;
                }
            }
            return dataPoints.ToArray();
        }

        public HT2000State GetState()
        {
            string deviceId = GetDeviceId();

            using (SafeFileHandle hDevice = Win32Api.CreateFile(deviceId,
                Win32Api.GENERIC_READ | Win32Api.GENERIC_WRITE,
                0,
                IntPtr.Zero,
                Win32Api.OPEN_EXISTING,
                Win32Api.FILE_FLAG_OVERLAPPED,
                IntPtr.Zero))
            {
                if (hDevice.IsInvalid)
                {
                    Console.WriteLine("Invalid handle.");
                    return null;
                }

                Console.WriteLine("Opened device.");
                Win32Api.HidD_SetNumInputBuffers(hDevice, 512);

                byte[] reportBuffer = new byte[61];
                reportBuffer[0] = 0x05;
                if (Win32Api.HidD_GetInputReport(hDevice, reportBuffer, (uint)reportBuffer.Length))
                {
                    Console.WriteLine("Success!");
                    using (var ms = new BinaryReader(new MemoryStream(reportBuffer)))
                    {
                        byte reportID = ms.ReadByte();
                        UInt32 raw_timestamp = ms.ReadUInt32BE();
                        UInt16 raw_totalRecords = ms.ReadUInt16BE();
                        UInt16 raw_temperature = ms.ReadUInt16BE();
                        UInt16 raw_humidity = ms.ReadUInt16BE();
                        UInt16 raw_temperatureAlarmLow = ms.ReadUInt16BE();
                        UInt16 raw_temperatureAlarmHigh = ms.ReadUInt16BE();
                        UInt16 raw_humidityAlarmLow = ms.ReadUInt16BE();
                        UInt16 raw_humidityAlarmHigh = ms.ReadUInt16BE();
                        ms.ReadBytes(3); // unknown
                        UInt16 raw_co2AlarmHigh = ms.ReadUInt16BE();
                        UInt16 raw_co2 = ms.ReadUInt16BE();
                        UInt16 raw_co2AlarmLow = ms.ReadUInt16BE();

                        DateTime timestamp = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(raw_timestamp);

                        UInt16 totalRecords = raw_totalRecords;

                        double temperature = (raw_temperature - 400.0) / 10.0;
                        double temperatureAlarmLow = (raw_temperatureAlarmLow - 400.0) / 10.0;
                        double temperatureAlarmHigh = (raw_temperatureAlarmHigh - 400.0) / 10.0;

                        double humidity = raw_humidity / 10.0;
                        double humidityAlarmLow = raw_humidityAlarmLow / 10.0;
                        double humidityAlarmHigh = raw_humidityAlarmHigh / 10.0;

                        int co2level = raw_co2;
                        int co2levelAlarmLow = raw_co2AlarmLow;
                        int co2levelAlarmHigh = raw_co2AlarmHigh;

                        var newState = new HT2000State(
                            timestamp,
                            totalRecords,
                            co2level, 
                            co2levelAlarmLow, 
                            co2levelAlarmHigh, 
                            temperature, 
                            temperatureAlarmLow,
                            temperatureAlarmHigh,
                            humidity,
                            humidityAlarmLow,
                            humidityAlarmHigh);

                        return newState;
                    }
                }
                else
                {
                    Console.WriteLine("Failed. Last error: " + Marshal.GetLastWin32Error());
                    return null;
                }
            }
        }
    }
}
