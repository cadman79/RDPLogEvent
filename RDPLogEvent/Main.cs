using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

    #region 참고자료 사이트
    /**
     * 원격-데스크톱rdp-악용-침해사고-이벤트-로그-분석
     * https://www.igloo.co.kr/security-information/%EC%9B%90%EA%B2%A9-%EB%8D%B0%EC%8A%A4%ED%81%AC%ED%86%B1rdp-%EC%95%85%EC%9A%A9-%EC%B9%A8%ED%95%B4%EC%82%AC%EA%B3%A0-%EC%9D%B4%EB%B2%A4%ED%8A%B8-%EB%A1%9C%EA%B7%B8-%EB%B6%84%EC%84%9D/
     *
     **/

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
            listView1.Columns.Add("UserName", 120);
            listView1.Columns.Add("TimeCreated", 150);
            listView1.Columns.Add("Type", 50);            


            //statusStrip1.RightToLeft = RightToLeft.Yes;

            toolStripStatusLabel1.Text = "time: 0ms,";
            toolStripStatusLabel2.Text = "rows:";


            List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();

            DDLCertify.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            data.Clear();
            data.Add(new KeyValuePair<string, string>("사용자 인증 실패", "4625"));
            data.Add(new KeyValuePair<string, string>("사용자 인증 성공", "4624"));
            DDLCertify.DataSource = null;            
            DDLCertify.DataSource = new BindingSource(data, null);
            DDLCertify.DisplayMember = "Key";
            DDLCertify.ValueMember = "Value";
            DDLCertify.SelectedValue = "4625";

            List<KeyValuePair<string, bool>> data2 = new List<KeyValuePair<string, bool>>();
            DDLOrderBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            data2.Clear();
            data2.Add(new KeyValuePair<string, bool>("최근순", true));
            data2.Add(new KeyValuePair<string, bool>("오래된순", false));
            DDLOrderBy.DataSource = null;
            DDLOrderBy.DataSource = new BindingSource(data2, null);
            DDLOrderBy.DisplayMember = "Key";
            DDLOrderBy.ValueMember = "Value";
            DDLOrderBy.SelectedValue = true;

            List<KeyValuePair<string, int>> data3 = new List<KeyValuePair<string, int>>();
            DDLMaxRow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            data3.Clear();            
            data3.Add(new KeyValuePair<string, int>("500건 조회", 500));
            data3.Add(new KeyValuePair<string, int>("1000건 조회", 1000));
            data3.Add(new KeyValuePair<string, int>("5000건 조회", 5000));
            data3.Add(new KeyValuePair<string, int>("10000건 조회", 10000));
            data3.Add(new KeyValuePair<string, int>("전체조회", 0));
            DDLMaxRow.DataSource = null;
            DDLMaxRow.DataSource = new BindingSource(data3, null);
            DDLMaxRow.DisplayMember = "Key";
            DDLMaxRow.ValueMember = "Value";
            DDLMaxRow.SelectedValue = 1000;

        }



        private void btnLog_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            #region 이전소스
            /**
            int currentCount = 0;
            int limitCount = 100000;
            string QueryString = @"*[System/EventID=4625]";
            var eventQuery = new EventLogQuery("Security", PathType.LogName, QueryString);
            eventQuery.ReverseDirection = true;    // 이벤트 최신 or 오래된순서 (order by)

            EventLogReader logReader = new EventLogReader(eventQuery);
            
            var items = new List<ListViewItem>();
            ListViewItem item;

            //label1.Text = ((EventRecord)logEntry).c
            ///((EventLogReader)logReader).

            for (EventRecord logEntry = logReader.ReadEvent(); logEntry != null; logEntry = logReader.ReadEvent())
            {
                // Read Event details
                var loginEventPropertySelector = new EventLogPropertySelector(new[] {
                                "Event/EventData/Data[@Name='IpAddress']",
                                "Event/EventData/Data[@Name='WorkstationName']",
                                "Event/EventData/Data[@Name='TargetUserName']",
                                "Event/System/TimeCreated/@SystemTime"
                            });
                IList<object> logEventProperties = ((System.Diagnostics.Eventing.Reader.EventLogRecord)logEntry).GetPropertyValues(loginEventPropertySelector);
                

                item = new ListViewItem(new[] { 
                    currentCount.ToString(), 
                    (string)logEventProperties[0], 
                    (string)logEventProperties[1], 
                    (string)logEventProperties[2], 
                    logEventProperties[3].ToString() 
                });

                items.Add(item);

                //listView1.Items.Add(item);

                if (++currentCount >= limitCount)
                {
                    break;
                }
            }

            listView1.Items.AddRange(items.ToArray());
            **/
            #endregion
            listView1.BeginUpdate();
            listView1.Items.Clear();

            string EventID = (string)DDLCertify.SelectedValue;
            bool ReverseDirection = (bool)DDLOrderBy.SelectedValue;
            int MaxCount = (int)DDLMaxRow.SelectedValue;

            List<ListViewItem> items = EventDataQuery(EventID, ReverseDirection, MaxCount);

            listView1.Items.AddRange(items.ToArray());
            listView1.EndUpdate();
            stopwatch.Stop();
            toolStripStatusLabel1.Text = string.Format("time : {0}ms,", stopwatch.ElapsedMilliseconds);
            toolStripStatusLabel2.Text = string.Format("rows : {0}", items.Count);



        } //btnLog_Click (e)



        /// <summary>
        /// 이벤트 로그 조회
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="ReverseDirection"></param>
        /// <param name="MaxCount"></param>
        /// <returns></returns>
        private List<ListViewItem> EventDataQuery(string EventID, bool ReverseDirection, int MaxCount)
        {
            int currentCount = 0;
            string QueryString = string.Format(@"*[System/EventID={0}]", EventID);
            var eventQuery = new EventLogQuery("Security", PathType.LogName, QueryString);
            eventQuery.ReverseDirection = ReverseDirection;    // 이벤트 최신=true or 오래된순서=false (order by)

            EventLogReader logReader = new EventLogReader(eventQuery);

            var items = new List<ListViewItem>();
            ListViewItem item;

            //label1.Text = ((EventRecord)logEntry).c
            ///((EventLogReader)logReader).

            for (EventRecord logEntry = logReader.ReadEvent(); logEntry != null; logEntry = logReader.ReadEvent())
            {
                // Read Event details
                var loginEventPropertySelector = new EventLogPropertySelector(new[] {
                                "Event/EventData/Data[@Name='IpAddress']",
                                "Event/EventData/Data[@Name='WorkstationName']",
                                "Event/EventData/Data[@Name='TargetUserName']",
                                "Event/System/TimeCreated/@SystemTime",
                                "Event/EventData/Data[@Name='LogonType']"
                            });
                IList<object> logEventProperties = ((System.Diagnostics.Eventing.Reader.EventLogRecord)logEntry).GetPropertyValues(loginEventPropertySelector);

                //if(( EventID.Equals("4624") && (logEventProperties[4].Equals(2)))
                if (EventID.Equals("4624")) // 사용자 인증 성공
                {
                    //if (logEventProperties[4].Equals((object)2) || logEventProperties[4].Equals((object)3) || logEventProperties[4].Equals((object)10))
                    if (Convert.ToInt32(logEventProperties[4]).Equals(3) || Convert.ToInt32(logEventProperties[4]).Equals(10))
                    {
                        item = new ListViewItem(new[] {
                        currentCount.ToString(),
                        (string)logEventProperties[0],
                        (string)logEventProperties[1],
                        (string)logEventProperties[2],
                        logEventProperties[3].ToString(),
                        logEventProperties[4].ToString()
                    });

                        items.Add(item);
                    }
                }
                else if (EventID.Equals("4625")) // 사용자 인증 실패
                {
                    item = new ListViewItem(new[] {
                        currentCount.ToString(),
                        (string)logEventProperties[0],
                        (string)logEventProperties[1],
                        (string)logEventProperties[2],
                        logEventProperties[3].ToString(),
                        logEventProperties[4].ToString()
                    });

                    items.Add(item);
                }
                else { }


                //listView1.Items.Add(item);

                if ((++currentCount >= MaxCount) && (MaxCount > 0))
                {
                    break;
                }
            }

            return items;
        }


    } // public class (e)
} // namespace (e)
