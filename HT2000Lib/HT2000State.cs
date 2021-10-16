using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HT2000Lib
{
    public class HT2000State
    {
        public DateTime Timestamp { get; private set; }
        public int RecordCount { get; private set; }
        public int CO2Level { get; private set; }
        public double Temperature { get; private set; }
        public double Humidity { get; private set; }
        public double TemperatureAlarmLow { get; private set; }
        public double TemperatureAlarmHigh { get; private set; }
        public double HumidityAlarmLow { get; private set; }
        public double HumidityAlarmHigh { get; private set; }
        public int CO2LevelAlarmLow { get; private set; }
        public int CO2LevelAlarmHigh { get; private set; }
        public bool IsValid { get; private set; }
        
        public HT2000State(
            DateTime timestamp,
            int recordCount,
            int co2level,
            int co2levelAlarmLow, 
            int co2levelAlarmHigh, 
            double temperature, 
            double temperatureAlarmLow,
            double temperatureAlarmHigh, 
            double humidity, 
            double humidityAlarmLow, 
            double humidityAlarmHigh)
        {
            Timestamp = timestamp;
            RecordCount = recordCount;
            CO2Level = co2level;
            CO2LevelAlarmLow = co2levelAlarmLow;
            CO2LevelAlarmHigh = co2levelAlarmHigh;
            Temperature = temperature;
            TemperatureAlarmLow = temperatureAlarmLow;
            TemperatureAlarmHigh = temperatureAlarmHigh;
            Humidity = humidity;
            HumidityAlarmLow = humidityAlarmLow;
            HumidityAlarmHigh = humidityAlarmHigh;
            IsValid = true;
        }

        internal HT2000State()
        {
            IsValid = false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is HT2000State))
            {
                return false;
            }
            var state = (HT2000State)obj;
            // two invalid states are always considered equal
            if (!this.IsValid && !state.IsValid)
            {
                return true;
            }
            return this.CO2Level == state.CO2Level &&
                this.Temperature == state.Temperature &&
                this.Humidity == state.Humidity;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().Name);
            sb.Append(" { Timestamp: ");
            sb.Append(Timestamp.ToShortDateString());
            sb.Append(" ");
            sb.Append(Timestamp.ToLongTimeString());
            sb.Append(", Record Count: ");
            sb.Append(RecordCount);
            sb.Append(", CO2 Level: ");
            sb.Append(CO2Level);
            sb.Append("ppm, Temperature: ");
            sb.Append(Temperature);
            sb.Append("°C, Humidity: ");
            sb.Append(Humidity);
            sb.Append("%, Alarms: { ");
            sb.Append("CO2 Low: ");
            sb.Append(CO2LevelAlarmLow);
            sb.Append("ppm, CO2 High: ");
            sb.Append(CO2LevelAlarmHigh);
            sb.Append("ppm, Temp Low: ");
            sb.Append(TemperatureAlarmLow);
            sb.Append("°C, Temp High: ");
            sb.Append(TemperatureAlarmHigh);
            sb.Append("°C, Hum Low: ");
            sb.Append(HumidityAlarmLow);
            sb.Append("%, Hum High: ");
            sb.Append(HumidityAlarmHigh);
            sb.Append("% } }");
            return sb.ToString();
        }
    }
}
