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

        // 국가코드, 국가명
        Dictionary<string, string> CountryCode = new Dictionary<string, string>{
            { "GH","가나" },
            { "GA","가봉" },
            { "GY","가이아나" },
            { "GM","감비아" },
            { "GG","건지섬" },
            { "GP","과들루프" },
            { "GT","과테말라" },
            { "GU","괌" },
            { "GD","그레나다" },
            { "GR","그리스" },
            { "GL","그린란드" },
            { "GN","기니" },
            { "GW","기니비사우" },
            { "NA","나미비아" },
            { "NR","나우루" },
            { "NG","나이지리아" },
            { "AQ","남극" },
            { "SS","남수단" },
            { "ZA","남아프리카 공화국" },
            { "NL","네덜란드" },
            { "AN","네덜란드령 안틸레스" },
            { "NP","네팔" },
            { "NO","노르웨이" },
            { "NF","노퍽섬" },
            { "NC","누벨칼레도니" },
            { "NZ","뉴질랜드" },
            { "NU","니우에" },
            { "NE","니제르" },
            { "NI","니카라과" },
            { "KR","대한민국" },
            { "DK","덴마크" },
            { "DO","도미니카 공화국" },
            { "DM","도미니카 연방" },
            { "DE","독일" },
            { "TL","동티모르" },
            { "LA","라오스" },
            { "LR","라이베리아" },
            { "LV","라트비아" },
            { "RU","러시아" },
            { "LB","레바논" },
            { "LS","레소토" },
            { "RE","레위니옹" },
            { "RO","루마니아" },
            { "LU","룩셈부르크" },
            { "RW","르완다" },
            { "LY","리비아" },
            { "LT","리투아니아" },
            { "LI","리히텐슈타인" },
            { "MG","마다가스카르" },
            { "MQ","마르티니크" },
            { "MH","마셜 제도" },
            { "YT","마요트" },
            { "MO","마카오" },
            { "MK","북마케도니아" },
            { "MW","말라위" },
            { "MY","말레이시아" },
            { "ML","말리" },
            { "IM","맨섬" },
            { "MX","멕시코" },
            { "MC","모나코" },
            { "MA","모로코" },
            { "MU","모리셔스" },
            { "MR","모리타니" },
            { "MZ","모잠비크" },
            { "ME","몬테네그로" },
            { "MS","몬트세랫" },
            { "MD","몰도바" },
            { "MV","몰디브" },
            { "MT","몰타" },
            { "MN","몽골" },
            { "US","미국" },
            { "UM","미국령 군소 제도" },
            { "VI","미국령 버진아일랜드" },
            { "MM","미얀마" },
            { "FM","미크로네시아 연방" },
            { "VU","바누아투" },
            { "BH","바레인" },
            { "BB","바베이도스" },
            { "VA","바티칸 시국" },
            { "BS","바하마" },
            { "BD","방글라데시" },
            { "BM","버뮤다" },
            { "BJ","베냉" },
            { "VE","베네수엘라" },
            { "VN","베트남" },
            { "BE","벨기에" },
            { "BY","벨라루스" },
            { "BZ","벨리즈" },
            { "BQ","보네르섬" },
            { "BA","보스니아 헤르체고비나" },
            { "BW","보츠와나" },
            { "BO","볼리비아" },
            { "BI","부룬디" },
            { "BF","부르키나파소" },
            { "BV","부베섬" },
            { "BT","부탄" },
            { "MP","북마리아나 제도" },
            { "BG","불가리아" },
            { "BR","브라질" },
            { "BN","브루나이" },
            { "WS","사모아" },
            { "SA","사우디아라비아" },
            { "GS","사우스조지아 사우스샌드위치 제도" },
            { "SM","산마리노" },
            { "ST","상투메 프린시페" },
            { "MF","생마르탱" },
            { "BL","생바르텔레미" },
            { "PM","생피에르 미클롱" },
            { "EH","서사하라" },
            { "SN","세네갈" },
            { "RS","세르비아" },
            { "SC","세이셸" },
            { "LC","세인트루시아" },
            { "VC","세인트빈센트 그레나딘" },
            { "KN","세인트키츠 네비스" },
            { "SH","세인트헬레나" },
            { "SO","소말리아" },
            { "SB","솔로몬 제도" },
            { "SD","수단" },
            { "SR","수리남" },
            { "LK","스리랑카" },
            { "SJ","스발바르 얀마옌" },
            { "SE","스웨덴" },
            { "CH","스위스" },
            { "ES","스페인" },
            { "SK","슬로바키아" },
            { "SI","슬로베니아" },
            { "SY","시리아" },
            { "SL","시에라리온" },
            { "SX","신트마르턴" },
            { "SG","싱가포르" },
            { "AE","아랍에미리트" },
            { "AW","아루바" },
            { "AM","아르메니아" },
            { "AR","아르헨티나" },
            { "AS","아메리칸사모아" },
            { "IS","아이슬란드" },
            { "HT","아이티" },
            { "IE","아일랜드" },
            { "AZ","아제르바이잔" },
            { "AF","아프가니스탄" },
            { "AD","안도라" },
            { "AL","알바니아" },
            { "DZ","알제리" },
            { "AO","앙골라" },
            { "AG","앤티가 바부다" },
            { "AI","앵귈라" },
            { "ER","에리트레아" },
            { "SZ","에스와티니" },
            { "EE","에스토니아" },
            { "EC","에콰도르" },
            { "ET","에티오피아" },
            { "SV","엘살바도르" },
            { "GB","영국" },
            { "VG","영국령 버진아일랜드" },
            { "IO","영국령 인도양 지역" },
            { "YE","예멘" },
            { "OM","오만" },
            { "AU","오스트레일리아" },
            { "AT","오스트리아" },
            { "HN","온두라스" },
            { "AX","올란드 제도" },
            { "JO","요르단" },
            { "UG","우간다" },
            { "UY","우루과이" },
            { "UZ","우즈베키스탄" },
            { "UA","우크라이나" },
            { "WF","왈리스 푸투나" },
            { "IQ","이라크" },
            { "IR","이란" },
            { "IL","이스라엘" },
            { "EG","이집트" },
            { "IT","이탈리아" },
            { "IN","인도" },
            { "ID","인도네시아" },
            { "JP","일본" },
            { "JM","자메이카" },
            { "ZM","잠비아" },
            { "JE","저지섬" },
            { "GQ","적도 기니" },
            { "KP","조선민주주의인민공화국" },
            { "GE","조지아" },
            { "CN","중국" },
            { "CF","중앙아프리카 공화국" },
            { "DJ","지부티" },
            { "GI","지브롤터" },
            { "ZW","짐바브웨" },
            { "TD","차드" },
            { "CZ","체코" },
            { "CL","칠레" },
            { "CM","카메룬" },
            { "CV","카보베르데" },
            { "KZ","카자흐스탄" },
            { "QA","카타르" },
            { "KH","캄보디아" },
            { "CA","캐나다" },
            { "KE","케냐" },
            { "KY","케이맨 제도" },
            { "KM","코모로" },
            { "CR","코스타리카" },
            { "CC","코코스 제도" },
            { "CI","코트디부아르" },
            { "CO","콜롬비아" },
            { "CG","콩고 공화국" },
            { "CD","콩고 민주 공화국" },
            { "CU","쿠바" },
            { "KW","쿠웨이트" },
            { "CK","쿡 제도" },
            { "CW","퀴라소" },
            { "HR","크로아티아" },
            { "CX","크리스마스섬" },
            { "KG","키르기스스탄" },
            { "KI","키리바시" },
            { "CY","키프로스" },
            { "TW","대만" },
            { "TJ","타지키스탄" },
            { "TZ","탄자니아" },
            { "TH","태국" },
            { "TC","터크스 케이커스 제도" },
            { "TR","튀르키예" },
            { "TG","토고" },
            { "TK","토켈라우" },
            { "TO","통가" },
            { "TM","투르크메니스탄" },
            { "TV","투발루" },
            { "TN","튀니지" },
            { "TT","트리니다드 토바고" },
            { "PA","파나마" },
            { "PY","파라과이" },
            { "PK","파키스탄" },
            { "PG","파푸아뉴기니" },
            { "PW","팔라우" },
            { "PS","팔레스타인" },
            { "FO","페로 제도" },
            { "PE","페루" },
            { "PT","포르투갈" },
            { "FK","포클랜드 제도" },
            { "PL","폴란드" },
            { "PR","푸에르토리코" },
            { "FR","프랑스" },
            { "GF","프랑스령 기아나" },
            { "TF","프랑스령 남방 및 남극 지역" },
            { "PF","프랑스령 폴리네시아" },
            { "FJ","피지" },
            { "FI","핀란드" },
            { "PH","필리핀" },
            { "PN","핏케언 제도" },
            { "HM","허드 맥도널드 제도" },
            { "HU","헝가리" },
            { "HK","홍콩" }
            };



        /// <summary>
        /// 초기화 설정
        /// </summary>
        private void Init()
        {
            //foreach (KeyValuePair<string, string> item in CountryCode) {
            //    Console.WriteLine("[{0} / {1}]", item.Key, item.Value);
            //}
            if(CountryCode.ContainsKey("US") == true)
                Console.WriteLine(CountryCode["US"]);

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
