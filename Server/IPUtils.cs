using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class IPUtils
    {
        /// <summary>
        /// 获取本机ipv4地址
        /// </summary>
        /// <returns></returns>
        public static HashSet<string> GetLocalHost()
        {
            HashSet<string> localHost=new HashSet<string>();
            localHost.Add("0.0.0.0");
            localHost.Add("127.0.0.1");
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        string addr = IpEntry.AddressList[i].ToString();
                        localHost.Add(addr);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            return localHost;
        }
    }
}
