using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Security.Principal;
using System.Diagnostics;

namespace RDPLogEvent
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //https://mattmofdoom.com/detecting-logins-like-a-boss-the-seq-client-for-windows-logins/

            string query = @"*[System/EventID=4625]";
            EventLogQuery eventsQuery = new EventLogQuery("Security", PathType.LogName, query);
            try
            {
                EventLogReader logReader = new EventLogReader(eventsQuery);
                
                for (EventRecord logEntry = logReader.ReadEvent(); logEntry != null; logEntry = logReader.ReadEvent())
                {
                    // Read Event details
                    var loginEventPropertySelector = new EventLogPropertySelector(new[] {
                        "Event/EventData/Data[@Name='IpAddress']",
                        "Event/EventData/Data[@Name='IpPort']"});
                    IList<object> logEventProperties = ((EventLogRecord)logEntry).GetPropertyValues(loginEventPropertySelector);
                    Console.WriteLine("{0} / {1}", logEventProperties[0], logEventProperties[1]);
                    Console.WriteLine("{0} 컴퓨터이름", logEntry.TimeCreated);

                    //Console.WriteLine("a");

                    //EventLogPropertySelector logPropertyContext = new EventLogPropertySelector(xPathEnum);

                    //IList<object> logEventProps = ((EventLogRecord)arg.EventRecord).GetPropertyValues(logPropertyContext);
                    //Log("Time: ", logEventProps[0]);
                    //foreach (var p in logEventProperties)
                    //{
                    //    Console.WriteLine(p);
                    //}

                    /**
                        EventLogReader reader = new 
                        EventLogReader(eventLogQuery);
                                                            reader.Seek(SeekOrigin.Begin, filter.PageStart);

                        eventLogs.TotalLogs = **totalRowsAffected**;                    
                        EventRecord eventInstance = reader.ReadEvent();

                        int i = filter.PageSize;
                        while (eventInstance != null && i-- > 0)
                        {
                                             try
                                             {
                                              eventLogs.Entries.Add(new EventLogData
                                              {
                                               Type = eventInstance.LevelDisplayName,
                                               Source = eventInstance.ProviderName,
                                               Time = eventInstance.TimeCreated,
                                               Category = eventInstance.TaskDisplayName,
                                               EventId = eventInstance.Id,
                                               User = eventInstance.UserId != null ? eventInstance.UserId.Value : "",
                                               Computer = eventInstance.MachineName,
                                               Message = eventInstance.FormatDescription(),
                                               FullXml = eventInstance.ToXml()
                                              });
                                             }catch{}
                        eventInstance = reader.ReadEvent();
                        }
                        }
                        return eventLogs;
                     **/
                }




            }
            catch (EventLogNotFoundException ex)
            {
                Console.WriteLine("Error while reading the event logs");
                return;
            }
        }




    } // public(e)
} // namespace(e)
