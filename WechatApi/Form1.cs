#define DEBUG
#undef DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Xml;
using System.Web.Script.Serialization;
using Wechat;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        private String                           uuid;
        private int                              tip;
        private String                           base_url;
        private String                           redirect_url;
        private String                           skey;
        private String                           wxsid;
        private String                           wxuin;
        private String                           pass_ticket;
        private String                           deviceId = "e000000000000021";
        private Dictionary<string, dynamic>      self;
        private int                              MAX_GROUP_NUM = 35;
        private int                              COL_NUM = 4;

        private CookieContainer                  cookieContainer;

        private Wechat.Wechat wechat;

        public Form1()
        {
            InitializeComponent();
            info_display.ReadOnly = true;
            anchorControls(info_display);
            //qrcode_img.Anchor = AnchorStyles.Top;
            start_btn.Anchor = AnchorStyles.Top;
            this.MinimumSize = new System.Drawing.Size(333, 495);

            cookieContainer = new CookieContainer();

            wechat = new Wechat.Wechat(qrcode_img);
#if (DEBUG)
            pass_ticket = "6v2gj8oihiKV%2B%2FzFx31fc4zQ8ZB4aNvDfcfavgdseMo8EybRul8OscZylnts%2BKSZ";
            skey = "@crypt_3ca759_e9b5bc9e6cfb79a07a9f49541a9d912f";
            wxsid = "ZHu/WOz9i7GZXcFN";
            wxuin = "2622149902";
            base_url = "https://wx.qq.com/cgi-bin/mmwebwx-bin/";
            self = new Dictionary<string, dynamic>();
            //self.Add("UserName", "@b8a8bddd76a0b0e7c98225c86962de1a780a81b595ad18ae4383e647b3b8227f"); // Liao's
            self.Add("UserName", "@7b04f82ebadc2829a6b7c156bb44e411b79413f2861b5e682eb9bec55658f108"); // me
#endif
        }

        private void anchorControls(Control control)
        {
            control.Anchor =
                    AnchorStyles.Bottom |
                    AnchorStyles.Right |
                    AnchorStyles.Top |
                    AnchorStyles.Left;
        }

        private void startbtn_Click(object sender, EventArgs e)
        {
            main();
        }

        private string get_timestamp()
        {
            return Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
        }

        private Tuple<String, HttpWebResponse> getResponseText(HttpWebRequest http, Encoding enc)
        {
            var response = (HttpWebResponse) http.GetResponse(); // should catch timeout exception

            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), enc))
            {
                return Tuple.Create(reader.ReadToEnd(), response);
            }
        }

        private HttpWebResponse getPostResponse(HttpWebRequest request, string payload)
        {
            request.Method = "POST";
            byte[] byteArray = Encoding.UTF8.GetBytes(payload);
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            return (HttpWebResponse) request.GetResponse();
        }

        private Dictionary<string, dynamic> deserilizeJson(HttpWebResponse response)
        {
            Console.WriteLine((int)response.StatusCode);        
            string serverResponse = "";
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var responseStreamReader = new StreamReader(responseStream))
                    {
                        serverResponse = responseStreamReader.ReadToEnd();
                    }
                }
            }
            var deserializer = new JavaScriptSerializer();
            var dic = deserializer.Deserialize<Dictionary<string, dynamic>>(serverResponse);
            return dic;
        }

        private Boolean getUUID()
        {
            var url = String.Format("https://login.weixin.qq.com/jslogin?appid=wx782c26e4c19acffb&fun=new&lang=zh_CN&_={0}", get_timestamp());
            var http = WebRequest.Create(url) as HttpWebRequest;
            http.CookieContainer = cookieContainer;
            // var param = new { appid = "wx782c26e4c19acffb", fun = "new", lang = "zh_CN", _ = get_timestamp() };
            // var response = http.Get(url, param);
            var responseText = getResponseText(http, new UTF8Encoding(true, true)).Item1;
            Console.WriteLine(responseText);
            var regex = @"window.QRLogin.code = (\d+); window.QRLogin.uuid = ""(\S+?)""";
            var r = new Regex(regex, RegexOptions.IgnoreCase);
            Match m = r.Match(responseText);
            Console.WriteLine(m.Success);

            var code = m.Groups[1].Value;
            uuid = m.Groups[2].Value;
            Console.WriteLine(code);
            Console.WriteLine(uuid);

            if (code == "200")
            {
                return true;
            }

            return false;
        }

        //显示微信登录二维码
        private void showQRImage()
        {
            var url = String.Format("https://login.weixin.qq.com/qrcode/" + uuid + "?t=webwx&_={0}", get_timestamp());
            Console.WriteLine(url);
            tip = 1;
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";

            updateUITextLine(info_display, "正在读取二维码，请稍等……", Environment.NewLine, Color.Black);
            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {              
                    Image image = Image.FromStream(stream); // should catch timeout exception                   
                    qrcode_img.Invoke(new Action( ()=>
                    {
                        qrcode_img.Image = image;
                        qrcode_img.Height = image.Height;
                        qrcode_img.Width = image.Width;
                    }));

                    start_btn.Invoke(new Action( ()=>
                    {
                        this.Controls.Remove(start_btn);
                        start_btn.Dispose();
                    }));

                    updateUITextLine(info_display, "请使用微信扫描二维码以登录", Environment.NewLine, Color.Black);                 
                }
            }
        }
        //加载登录
        private String waitForLogin()
        {
            var url = String.Format("https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?tip={0}&uuid={1}&_={2}", tip, uuid, get_timestamp());
            Console.WriteLine(url);
            var http = WebRequest.Create(url) as HttpWebRequest;
            http.CookieContainer = cookieContainer;
            var tuple = getResponseText(http, new UTF8Encoding(true, true));
            var text = tuple.Item1;
            Console.WriteLine(text);
            var regex = @"window.code=(\d+);";
            var r = new Regex(regex, RegexOptions.None);
            Match m = r.Match(text);
            var code = m.Groups[1].Value;

            var response = (HttpWebResponse)tuple.Item2;
            foreach (Cookie cookie in response.Cookies)
            {
                Console.WriteLine(String.Format("Cookie_in_wait_login: {0}, {1}", cookie.Name,cookie.Value));
            }
            
            if(code == "201")
            {
                updateUITextLine(info_display, "扫描成功,请在手机上点击确认以登录", Environment.NewLine, Color.Black);
                tip = 0;
            } else if(code == "200")
            {

                updateUITextLine(info_display, "正在登录", Environment.NewLine, Color.Black);
                regex = @"window.redirect_uri=""(\S+?)"";";
                r = new Regex(regex, RegexOptions.None);
                m = r.Match(text);
                redirect_url = m.Groups[1] + "&fun=new";
                for (int i = redirect_url.Length - 1; i >= 0; i--)
                {
                    if(redirect_url[i] == '/')
                    {
                        base_url = redirect_url.Substring(0, i + 1);
                        break;
                    }
                }           
            } else if(code == "408")
            {
                // do nothing
            }
            return code;
        }
        
        //登录微信
        private Boolean login()
        {
            qrcode_img.Invoke(new Action(() =>
            {
                this.Controls.Remove(qrcode_img);
                qrcode_img.Dispose();
            }));

            info_display.Invoke(new Action(() =>
            {
                info_display.Location = new System.Drawing.Point(0, 0);
                info_display.Size = new Size(333, 455);
                anchorControls(info_display);
                info_display.Clear();
                info_display.AppendText("正在扫描……" + Environment.NewLine);
            }));            
            var http = WebRequest.Create(redirect_url) as HttpWebRequest;
            http.CookieContainer = cookieContainer;
            var tuple = getResponseText(http, new UTF8Encoding(true, true));
            var data = tuple.Item1;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);
            var root = doc.DocumentElement;
            skey = root.SelectSingleNode("skey").InnerText;
            wxsid = root.SelectSingleNode("wxsid").InnerText;
            wxuin = root.SelectSingleNode("wxuin").InnerText;
            pass_ticket = root.SelectSingleNode("pass_ticket").InnerText;
            var response = (HttpWebResponse)tuple.Item2;
            foreach (Cookie cookie in response.Cookies)
            {
                Console.WriteLine(String.Format("Cookie_in_login: {0}: {1}", cookie.Name, cookie.Value));
            }
            if (String.IsNullOrEmpty(skey) || String.IsNullOrEmpty(wxsid) || String.IsNullOrEmpty(wxuin) || String.IsNullOrEmpty(pass_ticket))
            {
                return false;
            }
            return true;
        }
        
        //初始化微信协议
        private bool webwxinit()
        {

            var url = base_url + String.Format("webwxinit?pass_ticket={0}&skey={1}&r={2}", pass_ticket, skey, get_timestamp());
            Console.WriteLine(String.Format("base URL: {0}", base_url));
            var http = WebRequest.Create(url) as HttpWebRequest;
            http.CookieContainer = cookieContainer;
            http.ContentType = "application/json; charset=UTF-8";
            http.Method = "POST";
            var serializer = new JavaScriptSerializer();
            var base_req_param = new { Uin = Int64.Parse(wxuin), Sid = wxsid, Skey = skey, DeviceID = deviceId };
            var BaseRequest = serializer.Serialize(new { BaseRequest = base_req_param });
            Console.WriteLine(String.Format("BaseRequest: {0}", BaseRequest));
            var response = getPostResponse(http, BaseRequest);
            var dic = deserilizeJson(response);
            Console.WriteLine(dic);
            self = dic["User"];
            var ErrMsg = dic["BaseResponse"]["ErrMsg"];
            if (ErrMsg.Length > 0)
            {
                updateUITextLine(info_display, ErrMsg, Environment.NewLine, Color.Red);
            }
            if( dic["BaseResponse"]["Ret"] != 0)
            {
                return false;
            }     
            return true;
        }
        
        //获取联系人
        private List<Dictionary<string, dynamic>> webwxgetcontact()
        {
            var url = base_url + String.Format("webwxgetcontact?pass_ticket={0}&skey={1}&r={2}", pass_ticket, skey, get_timestamp());
            var http = WebRequest.Create(url) as HttpWebRequest;
#if (DEBUG)
            Uri target = new Uri("https://wx.qq.com");
            cookieContainer.Add(new Cookie("mm_lang", "zh_CN") { Domain = target.Host });
            cookieContainer.Add(new Cookie("webwx_data_ticket", "AQcsqhDcQuYzt1F1QvsvBV9W") { Domain = target.Host });
            cookieContainer.Add(new Cookie("wxuin", "2622149902") { Domain = target.Host });
            cookieContainer.Add(new Cookie("wxsid", "ZHu/WOz9i7GZXcFN") { Domain = target.Host });
            cookieContainer.Add(new Cookie("wxloadtime", "1452642551") { Domain = target.Host });
            cookieContainer.Add(new Cookie("webwxuvid", "20e90937e5f3173d94c75ee2c3805a26a0da27cb6eb2a7f692f9b26c09eb4a2d68ac2e5055bac129bc2e653ddc9b3312") { Domain = target.Host });
#endif
            http.CookieContainer = cookieContainer;
            http.ContentType = "application/json; charset=UTF-8";
            http.Method = "GET";
            var response = http.GetResponse() as HttpWebResponse;
            var dic = deserilizeJson(response);
            List<Dictionary<string, dynamic>> memberlist = new List<Dictionary<string, dynamic>>(dic["MemberList"].
                                                           ToArray(typeof(Dictionary<string, dynamic>)));
            var special_users = new List<string>()
            {
                "newsapp",
                "fmessage",
                "filehelper",
                "weibo",
                "qqmail",
                "tmessage",
                "qmessage",
                "qqsync",
                "floatbottle",
                "lbsapp", "shakeapp","medianote", "qqfriend", "readerapp",
                "blogapp", "facebookapp","masssendapp", "meishiapp", "feedsapp", "voip", "blogappweixin","weixin",
                "brandsessionholder", "weixinreminder","wxid_novlwrv3lqwv11", "gh_22b87fa7cb3c", "officialaccounts","notification_messages",
                "wxitil", "userexperience_alarm"
            };

            for (int i = memberlist.Count - 1; i > -1; i--)
            {
                Dictionary<string, dynamic> member = memberlist[i];
                if ((8 & Convert.ToInt32(member["VerifyFlag"])) != 0)
                {
                    memberlist.Remove(member);
                }
                else if (special_users.Contains(member["UserName"]))
                {
                    memberlist.Remove(member);
                }
                else if (member["UserName"].Contains("@@"))
                {
                    memberlist.Remove(member);
                }
                else if (member["UserName"] == self["UserName"])
                {
                    memberlist.Remove(member);
                }
            }

            return memberlist;

        }
        
        //添加日志
        private void updateUITextLine(RichTextBox control, string text, string end, Color color) {
            control.Invoke(new Action(() =>
            {
                control.SelectionColor = color;
                control.AppendText(text + end);
                control.ScrollToCaret();
            }));
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if ((worker.CancellationPending == true))
            {
                e.Cancel = true;
            }
            else
            {
                //if (!getUUID())
                //{
                //    updateUITextLine(info_display, "获取uuid失败", Environment.NewLine, Color.Red);             
                //    return;
                //}
                //showQRImage();
                //while (waitForLogin() != "200") ;

                //if (!login())
                //{
                //    updateUITextLine(info_display, "登录失败", Environment.NewLine, Color.Red);
                //    return;
                //}

                //if (!webwxinit())
                //{
                //    updateUITextLine(info_display, "初始化失败", Environment.NewLine, Color.Red);
                //    return;
                //}

                //SendTextMsg(self["UserName"], "hello world");

                wechat.AutoLogin();

                //var member_list = webwxgetcontact();
                //if(member_list.Count == 0)
                //{
                //    updateUITextLine(info_display, "好友列表为空", Environment.NewLine, Color.Red);
                //    return;
                //}

                //Console.WriteLine("member_list: ");
                //foreach(var member in member_list)
                //{
                //    Console.WriteLine(member["UserName"]);
                //}

                //var member_count = member_list.Count;

                //updateUITextLine(info_display, String.Format("通讯录共{0}位好友", member_count), Environment.NewLine, Color.Black);
            }          
        }

        private void SendTextMsg(string text,string userName) {
            var url = base_url + string.Format("webwxsendmsg?pass_ticket={0}",pass_ticket);
            var http = WebRequest.Create(url) as HttpWebRequest;
            http.CookieContainer = cookieContainer;
            http.ContentType = "application/json; charset=UTF-8";
            http.Method = "POST";
            var serializer = new JavaScriptSerializer();
            var base_req_param = new { Uin = Int64.Parse(wxuin), Sid = wxsid, Skey = skey, DeviceID = deviceId };
            var id = Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds) * 1e4;
            var msg = new
            {
                Type = 1,
                Content = "test ",
                FromUserName = self["UserName"],
                ToUserName = "filehelper",
                ClientMsgId = id,
                LocalID = id
            };
            var payload = serializer.Serialize(new
            {
                BaseRequest = base_req_param,
                Msg = msg
         
            });

            var response = getPostResponse(http, payload);
            var dic = deserilizeJson(response);
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        //获取登录验证码
        private void main()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            info_display.AppendText(Environment.NewLine + "正在发送请求……" + Environment.NewLine);
            info_display.ScrollToCaret();
            bw.RunWorkerAsync();
        }

    }
}
