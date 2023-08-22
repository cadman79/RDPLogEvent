﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

        #region 참고자료 사이트
        /**
         * 원격-데스크톱rdp-악용-침해사고-이벤트-로그-분석
         * https://www.igloo.co.kr/security-information/%EC%9B%90%EA%B2%A9-%EB%8D%B0%EC%8A%A4%ED%81%AC%ED%86%B1rdp-%EC%95%85%EC%9A%A9-%EC%B9%A8%ED%95%B4%EC%82%AC%EA%B3%A0-%EC%9D%B4%EB%B2%A4%ED%8A%B8-%EB%A1%9C%EA%B7%B8-%EB%B6%84%EC%84%9D/
         *
         **/

        #endregion

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

            //statusStrip1.RightToLeft = RightToLeft.Yes;
            
            toolStripStatusLabel1.Text = "time:";
            toolStripStatusLabel2.Text = "OK";


            List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();

            DDLCertify.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            data.Add(new KeyValuePair<string, string>("사용자 인증 실패", "4625"));
            data.Add(new KeyValuePair<string, string>("사용자 인증 성공", "ASC"));
            DDLCertify.DataSource = null;            
            DDLCertify.DataSource = new BindingSource(data, null);
            DDLCertify.DisplayMember = "Key";
            DDLCertify.ValueMember = "Value";
            DDLCertify.SelectedValue = "4625";


            DDLOrderBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            data.Clear();
            data.Add(new KeyValuePair<string, string>("최근순", "DESC"));
            data.Add(new KeyValuePair<string, string>("오래된순", "ASC"));
            DDLOrderBy.DataSource = null;
            DDLOrderBy.DataSource = new BindingSource(data, null);
            DDLOrderBy.DisplayMember = "Key";
            DDLOrderBy.ValueMember = "Value";
            DDLOrderBy.SelectedValue = "DESC";

            DDLMaxRow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            data.Clear();
            data.Add(new KeyValuePair<string, string>("500건 조회", "500"));
            data.Add(new KeyValuePair<string, string>("1000건 조회", "1000"));
            data.Add(new KeyValuePair<string, string>("5000건 조회", "5000"));
            data.Add(new KeyValuePair<string, string>("10000건 조회", "10000"));
            DDLMaxRow.DataSource = null;
            DDLMaxRow.DataSource = new BindingSource(data, null);
            DDLMaxRow.DisplayMember = "Key";
            DDLMaxRow.ValueMember = "Value";
            DDLMaxRow.SelectedValue = "1000";

        }


        private int limitCount = 1000000;

        private void btnLog_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int currentCount = 0;
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

            stopwatch.Stop();
            toolStripStatusLabel1.Text = string.Format("time : {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine(string.Format("time : {0} ms", stopwatch.ElapsedMilliseconds));

        } //btnLog_Click (e)



    } // public class (e)
} // namespace (e)
