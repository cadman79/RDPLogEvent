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
                    var eventProperties = ((EventLogRecord)logEntry).GetPropertyValues(loginEventPropertySelector);
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
