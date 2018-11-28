using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Main
{
    public class SocketHelper
    {
        public static void SendTcp(byte[] buffer, IPEndPoint remoteEP)
        {
            Socket tcpClient = ConnectTo(remoteEP);
            if (tcpClient == null)
            {
                throw new Exception("###通讯报告###：创建服务器连接失败!");
            }
            try
            {
                tcpClient.Send(buffer);
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                tcpClient.Close();
                throw new Exception("###通讯报告###：数据发送失败!", ex);
            }
        }

        /// <summary>
        /// 创建连接
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <returns></returns>
        private static Socket ConnectTo(IPEndPoint remoteEP)
        {
            Socket tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                tcpClient.Connect(remoteEP);
                return tcpClient;
            }
            catch
            {
                return null;
            }
        }

    }
}
