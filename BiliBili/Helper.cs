using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace BiliBili
{
    public class Helper
    {
        public Exception Error;

        TcpClient Client;
        string host;
        int port;
        NetworkStream NetStream;
        bool Connected;
        bool debuglog = false;

        string RoomInfoUrl = "https://api.live.bilibili.com/room/v1/Room/room_init?id=";
        string CIDInfoUrl = "http://live.bilibili.com/api/player?id=cid:";

        public Action<string, string, int> ReceiveGift;
        public Action<string, string> ReceiveDanmu;
        public Action<uint> ReceivePopularValue;

        public Helper(int roomID)
        {
            try
            {
                var request = new HttpClient();
                {
                    var text = request.GetStringAsync(RoomInfoUrl + roomID).Result;
                    var json = JObject.Parse(text);
                    if (json["code"].ToString() != "0")
                    {
                        throw new Exception("房间号错误!");
                    }
                    roomID = int.Parse(json["data"]["room_id"].ToString());
                }
                {
                    var text = request.GetStringAsync(CIDInfoUrl + roomID).Result;
                    var xml = "<root>" + text + "</root>";
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    this.host = doc["root"]["dm_server"].InnerText;
                    this.port = int.Parse(doc["root"]["dm_port"].InnerText);
                    Connect(roomID);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Helper(string host, int port, int roomID)
        {
            this.host = host;
            this.port = port;
            Connect(roomID);
        }

        

        bool Connect(int roomID)
        {
            try
            {
                Client = new TcpClient();
                Client.Connect(host, port);
                NetStream = Client.GetStream();
                if (SendJoinChannel(roomID))
                {
                    Connected = true;
                    this.HeartbeatLoop();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }

        public void StartReceive()
        {
            var thread = new Thread(this.ReceiveMessageLoop)
            {
                IsBackground = true
            };
            thread.Start();
        }

        async void HeartbeatLoop()
        {

            try
            {
                while (this.Connected)
                {
                    this.SendHeartbeat();
                    await Task.Delay(30000);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void SendHeartbeat()
        {
            SendSocketData(2);
        }

        void SendSocketData(int action, string body = "")
        {
            SendSocketData(0, 16, 1, action, 1, body);
        }
        void SendSocketData(int packetlength, short magic, short ver, int action, int param = 1, string body = "")
        {
            var playload = Encoding.UTF8.GetBytes(body);
            if (packetlength == 0)
            {
                packetlength = playload.Length + 16;
            }
            var buffer = new byte[packetlength];
            using (var ms = new MemoryStream(buffer))
            {


                var b = BitConverter.GetBytes(buffer.Length).ToBE();

                ms.Write(b, 0, 4);
                b = BitConverter.GetBytes(magic).ToBE();
                ms.Write(b, 0, 2);
                b = BitConverter.GetBytes(ver).ToBE();
                ms.Write(b, 0, 2);
                b = BitConverter.GetBytes(action).ToBE();
                ms.Write(b, 0, 4);
                b = BitConverter.GetBytes(param).ToBE();
                ms.Write(b, 0, 4);
                if (playload.Length > 0)
                {
                    ms.Write(playload, 0, playload.Length);
                }
                NetStream.Write(buffer, 0, buffer.Length);
                NetStream.Flush();
            }
        }

        bool SendJoinChannel(int roomID)
        {

            Random r = new Random();
            var tmpuid = (long)(1e14 + 2e14 * r.NextDouble());
            var packetModel = new { roomid = roomID, uid = tmpuid };
            var playload = JsonConvert.SerializeObject(packetModel);
            SendSocketData(7, playload);
            return true;
        }

        void ReceiveMessageLoop()
        {
            try
            {
                var stableBuffer = new byte[Client.ReceiveBufferSize];

                while (this.Connected)
                {

                    NetStream.ReadB(stableBuffer, 0, 4);
                    var packetlength = BitConverter.ToInt32(stableBuffer, 0);
                    packetlength = IPAddress.NetworkToHostOrder(packetlength);

                    if (packetlength < 16)
                    {
                        throw new NotSupportedException("协议失败: (L:" + packetlength + ")");
                    }

                    NetStream.ReadB(stableBuffer, 0, 2);//magic
                    NetStream.ReadB(stableBuffer, 0, 2);//protocol_version 

                    NetStream.ReadB(stableBuffer, 0, 4);
                    var typeId = BitConverter.ToInt32(stableBuffer, 0);
                    typeId = IPAddress.NetworkToHostOrder(typeId);

                    //Console.WriteLine(typeId);
                    NetStream.ReadB(stableBuffer, 0, 4);//magic, params?
                    var playloadlength = packetlength - 16;
                    if (playloadlength == 0)
                    {
                        continue;//没有内容了

                    }
                    typeId = typeId - 1;//和反编译的代码对应 
                    var buffer = new byte[playloadlength];
                    NetStream.ReadB(buffer, 0, playloadlength);
                    switch (typeId)
                    {
                        case 0:
                            break;
                        case 1:
                            break;
                        case 2:
                            {
                                var viewer = BitConverter.ToUInt32(buffer.Take(4).Reverse().ToArray(), 0); //观众人数
                                Console.WriteLine("人气值 : " + viewer);
                                ReceivePopularValue?.Invoke(viewer);
                                break;
                            }
                        case 3:
                        case 4://playerCommand
                            {

                                var json = Encoding.UTF8.GetString(buffer, 0, playloadlength);
                                if (debuglog)
                                {
                                    Console.WriteLine(json);
                                }
                                try
                                {
                                    GiftFilter(json);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                                break;
                            }
                        case 5://newScrollMessage
                            {
                                break;
                            }
                        case 7:
                            {
                                break;
                            }
                        case 16:
                            {
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
            catch (NotSupportedException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GiftFilter(string json)
        {
            var obj = JObject.Parse(json);

            string cmd = obj["cmd"].ToString();
            switch (cmd)
            {
                case "DANMU_MSG":
                    {
                        var commentText = obj["info"][1].ToString();
                        var userName = obj["info"][2][1].ToString();
                        //Console.WriteLine("弹幕-" + userName + " : " + commentText);
                        ReceiveDanmu?.Invoke(userName, commentText);
                    }
                    break;
                case "SEND_GIFT":
                    {
                        var giftName = obj["data"]["giftName"].ToString();
                        var userName = obj["data"]["uname"].ToString();
                        var giftCount = obj["data"]["num"].ToObject<int>();
                        //Console.WriteLine("礼物-" + userName + " : " + giftName + "*" + giftCount);
                        ReceiveGift?.Invoke(userName, giftName, giftCount);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
