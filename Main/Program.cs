using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using BiliBili;
using Newtonsoft.Json;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip = System.Configuration.ConfigurationManager.AppSettings["ip"].ToString();
            int port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["port"].ToString());
            Console.WriteLine("请输入BiliBili直播房间号: ");
            int roomID = int.Parse(Console.ReadLine());
            Helper helper = new Helper(roomID);
            helper.ReceiveGift += (username, giftname, count) =>
            {
                var obj = new
                {
                    name = giftname,
                    count = count,
                    user = username
                };
                var json = JsonConvert.SerializeObject(obj);
                Console.WriteLine("礼物-" + username + " : " + giftname + "*" + count);
                try
                {
                    SocketHelper.SendTcp(Encoding.UTF8.GetBytes(json), new IPEndPoint(IPAddress.Parse(ip), port));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("异常: " + ex.Message);
                }
            };
            helper.StartReceive();
            ConsoleKeyInfo key;
            Random _rd = new Random(Guid.NewGuid().GetHashCode());
            do
            {
                key = Console.ReadKey();
                string name = null;
                int count = 1;
                switch (key.Key)
                {
                    case ConsoleKey.A:
                        name = "辣条";
                        count = _rd.Next(1, 521);
                        break;
                    case ConsoleKey.B:
                        name = "凉了";
                        count = _rd.Next(1, 260);
                        break;
                    case ConsoleKey.C:
                        name = "吃瓜";
                        count = _rd.Next(1, 100);
                        break;
                    case ConsoleKey.D:
                        name = "flag";
                        count = _rd.Next(1, 100);
                        break;
                    case ConsoleKey.E:
                        name = "爆米花";
                        count = _rd.Next(1, 50);
                        break;
                    case ConsoleKey.F:
                        name = "233";
                        count = _rd.Next(1, 30);
                        break;
                    case ConsoleKey.G:
                        name = "比心";
                        count = _rd.Next(1, 20);
                        break;
                    case ConsoleKey.H:
                        name = "干杯";
                        count = _rd.Next(1, 10);
                        break;
                    case ConsoleKey.I:
                        name = "666";
                        break;
                    case ConsoleKey.J:
                        name = "咸鱼";
                        break;
                    case ConsoleKey.K:
                        name = "冰阔落";
                        break;
                    case ConsoleKey.L:
                        name = "炮车";
                        break;
                    case ConsoleKey.M:
                        name = "情书";
                        break;
                    case ConsoleKey.N:
                        name = "真香";
                        break;
                    case ConsoleKey.O:
                        name = "给大佬递茶";
                        break;
                    case ConsoleKey.P:
                        name = "盛典门票";
                        break;
                    case ConsoleKey.Q:
                        name = "喵娘";
                        break;
                    case ConsoleKey.R:
                        name = "B坷垃";
                        break;
                    case ConsoleKey.S:
                        name = "礼花";
                        break;
                    case ConsoleKey.T:
                        name = "氪金键盘";
                        break;
                    case ConsoleKey.U:
                        name = "疯狂打call";
                        break;
                    case ConsoleKey.V:
                        name = "节奏风暴";
                        break;
                    case ConsoleKey.W:
                        name = "摩天大楼";
                        break;
                    case ConsoleKey.X:
                        name = "嗨翻全城";
                        break;
                    case ConsoleKey.Y:
                        name = "小电视飞船";
                        break;
                    case ConsoleKey.Z:
                        break;
                    case ConsoleKey.Escape:
                        break;
                    default:
                        break;
                }
                if (name != null)
                {
                    var obj = new
                    {
                        name = name,
                        count = count,
                        user = "测试"
                    };
                    Console.WriteLine("礼物-" + obj.user + " : " + obj.name + "*" + count);
                    var json = JsonConvert.SerializeObject(obj);
                    try
                    {
                        SocketHelper.SendTcp(Encoding.UTF8.GetBytes(json), new IPEndPoint(IPAddress.Parse(ip), port));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("异常: " + ex.Message);
                    }
                }
            } while (key.Key != ConsoleKey.Escape);
        }
    }
}
