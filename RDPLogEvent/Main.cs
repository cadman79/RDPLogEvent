using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDPLogEvent
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

    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            Init();
        }

        /// <summary>
        /// 초기화 설정
        /// </summary>
        private void Init()
        {
            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("NO", 50);
            listView1.Columns.Add("IP Address", 100);
            listView1.Columns.Add("WorkstationName", 150);
            listView1.Columns.Add("UserName", 100);
            listView1.Columns.Add("TimeCreated", 150);
        }


        private int limitCount = 1000000;

        private void btnLog_Click(object sender, EventArgs e)
        {
            int currentCount = 0;
            string[] arr = new string[5];
            string QueryString = @"*[System/EventID=4625]";
            var eventQuery = new EventLogQuery("Security", PathType.LogName, QueryString);
            eventQuery.ReverseDirection = true;    // 이벤트 최신 or 오래된순서 (order by)

            IList<object> logEventProperties = new List<object>();
            EventLogReader logReader = new EventLogReader(eventQuery);
            
            ListViewItem item = new ListViewItem();            

            for (EventRecord logEntry = logReader.ReadEvent(); logEntry != null; logEntry = logReader.ReadEvent())
            {
                // Read Event details
                var loginEventPropertySelector = new EventLogPropertySelector(new[] {
                                "Event/EventData/Data[@Name='IpAddress']",
                                "Event/EventData/Data[@Name='WorkstationName']",
                                "Event/EventData/Data[@Name='TargetUserName']",
                                "Event/System/TimeCreated/@SystemTime"
                            });
                logEventProperties = ((System.Diagnostics.Eventing.Reader.EventLogRecord)logEntry).GetPropertyValues(loginEventPropertySelector);

                //(IList<object>)sList.Cast<object>().ToList();
                //lstItem = new ListViewItem(arr);
                //item = new ListViewItem()[new ];
                arr[0] = currentCount.ToString();
                arr[1] = (string)logEventProperties[0];
                arr[2] = (string)logEventProperties[1];
                arr[3] = (string)logEventProperties[2];
                arr[4] = logEventProperties[3].ToString();

                item = new ListViewItem(arr);

                listView1.Items.Add(item);

                if (++currentCount >= limitCount)
                {
                    break;
                }
            }

        } //btnLog_Click (e)



    } // public class (e)
} // namespace (e)
