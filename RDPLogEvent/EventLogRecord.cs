using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDPLogEvent
{
    class EventLogRecord : ICloneable
    {
        private DateTime timestamp;
        private int eventID;
        private string eventData;
        private string ipaddress;

        public int EventID
        {
            get
            {
                return eventID;
            }

            set
            {
                eventID = value;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return timestamp;
            }

            set
            {
                timestamp = value;
            }
        }

        public string EventData
        {
            get
            {
                return eventData;
            }

            set
            {
                eventData = value;
            }
        }

        public string IPAddress { get { return ipaddress; } set { ipaddress = value; } }

        public EventLogRecord(EventRecord eventdetail)
        {
            eventID = eventdetail.Id;
            timestamp = eventdetail.TimeCreated.Value;
            eventData = GetEventData(eventdetail);
            ipaddress = eventdetail.LevelDisplayName;
        }

        private string GetEventData(EventRecord eventdetail)
        {
            StringBuilder eventData = new StringBuilder();
            foreach (EventProperty prop in eventdetail.Properties)
            {
                if (prop.Value is byte[])
                {
                    eventData.Append(prop.Value.ToString() + "\n");
                }
                else
                {
                    eventData.Append(prop.Value.ToString() + "\n");
                }


            }

            IList<object> logEventProperties = new List<object>();
            var loginEventPropertySelector = new EventLogPropertySelector(new[] { "Event/EventData/Data[@Name='IpAddress']" });
            logEventProperties = ((System.Diagnostics.Eventing.Reader.EventLogRecord)eventdetail).GetPropertyValues(loginEventPropertySelector);

            return logEventProperties[0].ToString();

        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
