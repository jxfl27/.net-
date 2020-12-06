using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class NetworkManagement
    {
        public Socket serverSocket;
        public int BUFFER_SIZE = 1024 * 10;//接受数据的缓冲区大小10KB
        //定义一个集合，存储客户端信息
        public Dictionary<string, Socket> onlineList = new Dictionary<string, Socket>();
        public GameRoom room = null;
        public List<GameRoom> roomList = new List<GameRoom>();

        /// <summary>
        /// 找到发生异常的套接字对象，进行善后工作
        /// </summary>
        /// <param name="exceptionSocket"></param>
        /// <param name="room"></param>
        /// <param name="roomList"></param>
        /// <param name="uuid"></param>
        /// <param name="target"></param>
        public void FindExceptionSocket(Socket exceptionSocket, ref GameRoom room, ref List<GameRoom> roomList, ref string uuid, ref string target)
        {
            try
            {
                if (room != null)
                {
                    if (room.FirstSocket == exceptionSocket)
                    {
                        onlineList.Remove(room.FirstUUID);
                        uuid = room.FirstUUID;
                        target = room.SecondUUID;
                        room = null;
                        return;
                    }
                    else if (room.SecondSocket == exceptionSocket)
                    {
                        onlineList.Remove(room.SecondUUID);
                        uuid = room.SecondUUID;
                        target = room.FirstUUID;
                        room = null;
                        return;
                    }
                }
                else
                {
                    for (int i = 0; i < roomList.Count; i++)
                    {
                        var item = roomList[i];
                        if (item.FirstSocket == exceptionSocket)
                        {
                            onlineList.Remove(item.FirstUUID);
                            uuid = item.FirstUUID;
                            target = item.SecondUUID;
                            return;
                        }
                        else if (item.SecondSocket == exceptionSocket)
                        {
                            onlineList.Remove(item.SecondUUID);
                            uuid = item.SecondUUID;
                            target = item.FirstUUID;
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// 判断客户端socket是否存活（处于连接状态）
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool IsAlive(Socket s)
        {
            try
            {
                byte[] buf = new byte[1024];
                s.ReceiveTimeout = 1000;
                if (s.Poll(1000, SelectMode.SelectRead))
                {
                    int nRead = s.Receive(buf);
                    if (nRead == 0)
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Close the socket safely.
        /// </summary>
        /// <param name="socket">The socket.</param>
        public void SafeClose(Socket socket)
        {
            if (socket == null)
                return;

            if (!socket.Connected)
                return;

            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }

            try
            {
                socket.Close();
            }
            catch
            {
            }
        }
    }

}
