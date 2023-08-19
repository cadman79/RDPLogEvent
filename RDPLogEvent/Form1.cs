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
using System.Collections;
using System.Data.Common;
using System.Reflection.Emit;
using System.Xml.Linq;
//using IP2Country;
//using IP2Country.MaxMind;

namespace RDPLogEvent
{
    public partial class Form1 : Form
    {


        #region 로그 데이터 구조 (https://gist.github.com/epsom-software/5791709)
        /*
              <QueryConfig>
                <QueryParams>
                  <UserQuery />
                </QueryParams>
                <QueryNode>
                  <Name LanguageNeutralValue = "Last 5 Minutes" > Last 5 Minutes</Name>
                  <QueryList>
                    <Query Id = "0" Path="Application">
                      <Select Path = "Application" > *[System[(Level = 1  or Level = 2 or Level = 3 or Level = 5) and TimeCreated[timediff(@SystemTime) & lt;= 300000]]]</Select>
                    </Query>
                  </QueryList>
                </QueryNode>
              </QueryConfig>
              <ResultsConfig>
                <Columns>
                  <Column Name = "Level" Type="System.String" Path="Event/System/Level" Visible="">226</Column>
                  <Column Name = "Keywords" Type="System.String" Path="Event/System/Keywords">70</Column>
                  <Column Name = "Date and Time" Type="System.DateTime" Path="Event/System/TimeCreated/@SystemTime" Visible="">276</Column>
                  <Column Name = "Source" Type="System.String" Path="Event/System/Provider/@Name" Visible="">186</Column>
                  <Column Name = "Event ID" Type="System.UInt32" Path="Event/System/EventID" Visible="">186</Column>
                  <Column Name = "Task Category" Type="System.String" Path="Event/System/Task" Visible="">189</Column>
                  <Column Name = "User" Type="System.String" Path="Event/System/Security/@UserID">50</Column>
                  <Column Name = "Operational Code" Type="System.String" Path="Event/System/Opcode">110</Column>
                  <Column Name = "Log" Type="System.String" Path="Event/System/Channel">80</Column>
                  <Column Name = "Computer" Type="System.String" Path="Event/System/Computer">170</Column>
                  <Column Name = "Process ID" Type="System.UInt32" Path="Event/System/Execution/@ProcessID">70</Column>
                  <Column Name = "Thread ID" Type="System.UInt32" Path="Event/System/Execution/@ThreadID">70</Column>
                  <Column Name = "Processor ID" Type="System.UInt32" Path="Event/System/Execution/@ProcessorID">90</Column>
                  <Column Name = "Session ID" Type="System.UInt32" Path="Event/System/Execution/@SessionID">70</Column>
                  <Column Name = "Kernel Time" Type="System.UInt32" Path="Event/System/Execution/@KernelTime">80</Column>
                  <Column Name = "User Time" Type="System.UInt32" Path="Event/System/Execution/@UserTime">70</Column>
                  <Column Name = "Processor Time" Type="System.UInt32" Path="Event/System/Execution/@ProcessorTime">100</Column>
                  <Column Name = "Correlation Id" Type="System.Guid" Path="Event/System/Correlation/@ActivityID">85</Column>
                  <Column Name = "Relative Correlation Id" Type="System.Guid" Path="Event/System/Correlation/@RelatedActivityID">140</Column>
                  <Column Name = "Event Source Name" Type="System.String" Path="Event/System/Provider/@EventSourceName">140</Column>
                </Columns>
              </ResultsConfig>
            </ViewerConfig>
        */
        #endregion

        #region NuGet
        /*
         * IP-2-Country.MaxMind
         * https://github.com/RobThree/IP2Country
         */
        #endregion


        
        public Form1()
        {
            InitializeComponent();
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            //async
            //https://stackoverflow.com/questions/76293166/eventlogquery-invalid

            //https://mattmofdoom.com/detecting-logins-like-a-boss-the-seq-client-for-windows-logins/

            //var resolver = new IP2CountryResolver( new MaxMindGeoLiteFileSource(@"c:\Temp\GeoLite2-Country-CSV_20230815.zip"));
            //var result = resolver.Resolve("172.217.17.110");

            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("No", 200);
            listView1.Columns.Add("Name", 50);
            listView1.Columns.Add("Name", 100);
            listView1.Columns.Add("Name", 100);
            string[] arr = new string[4];
            ListViewItem lstItem;
            

            string query = @"*[System/EventID=4625]";
            EventLogQuery eventsQuery = new EventLogQuery("Security", PathType.LogName, query);
            eventsQuery.ReverseDirection = true;    // 이벤트 최신 or 오래된순서 (order by)
            try
            {
                int limitCount = 1000;   // 이벤트 최대갯수
                int currentCount = 0;   // 현재 이벤트 count
                EventLogReader logReader = new EventLogReader(eventsQuery);
                for (EventRecord logEntry = logReader.ReadEvent(); logEntry != null; logEntry = logReader.ReadEvent())
                {
                    // Read Event details
                    var loginEventPropertySelector = new EventLogPropertySelector(new[] {
                        "Event/EventData/Data[@Name='IpAddress']",
                        "Event/EventData/Data[@Name='IpPort']",
                        "Event/System/TimeCreated/@SystemTime"
                    });
                    //IList<object> logEventProperties = ((EventLogRecord)logEntry).GetPropertyValues(loginEventPropertySelector);
                    var logEventProperties = ((EventLogRecord)logEntry).GetPropertyValues(loginEventPropertySelector);
                    
                    //var result = resolver.Resolve(logEventProperties[0].ToString());
                    //Console.WriteLine("Country: " + result?.Country);
                    //textBox1.Text += string.Format("{0} / {1} / {2} / {3}\r\n", logEventProperties[0], logEventProperties[1], logEventProperties[2], "aa");
                    Console.WriteLine("{0} / {1} / {2} / {3}", logEventProperties[0], logEventProperties[1], logEventProperties[2], "aa");
                    //Console.WriteLine("{0} 컴퓨터이름", logEntry.TimeCreated);
                    arr[0] = logEventProperties[0].ToString();
                    arr[1] = logEventProperties[1].ToString();
                    arr[2] = logEventProperties[2].ToString();
                    arr[3] = "a";
                    lstItem = new ListViewItem(arr);
                    listView1.Items.Add(lstItem);

                    if (++currentCount >= limitCount) {
                        break;
                    }

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

        private void button2_Click(object sender, EventArgs e)
        {
            //var resolver = new IP2CountryResolver(
            //    new MaxMindGeoLiteFileSource(@"c:\Temp\GeoLite2-Country-CSV_20230815.zip")
            //);
            //var result = resolver.Resolve("172.217.17.110");
            //Console.WriteLine("Country: " + result?.Country);
        }
    } // public(e)
} // namespace(e)
