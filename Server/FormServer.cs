using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class FormServer : Form
    {
        NetworkManagement networkManagement = new NetworkManagement();

        #region 关于UI线程的委托
        // 定义委托类型
        // 用于将log信息添加到listbox控件上
        delegate void AddItemToListView(ListViewItem item);
        delegate void RemoveItemFromOnline(string item);//移除在线玩家
        delegate void ChessEventReceiveHander(object sender, Message e);  //定义委托类型
        ChessEventReceiveHander OnReceiveMsg;        //声明委托变量
        AddItemToListView delLog;
        AddItemToListView delOnline;

        private void AddItemToListViewLog(ListViewItem item)
        {
            this.listView_log.Invoke(new Action(() => {
                listView_log.Items.Add(item);
            })); 
        }

        private void AddItemToListViewOnline(ListViewItem item)
        {
            this.listView_log.Invoke(new Action(() => {
                this.listView_online.Items.Add(item);
            }));
        }

        private void RemoveItemFromListViewOnline(string item)
        {
            if (this.listView_online.InvokeRequired)//如果调用控件的线程和创建控件的线程不是同一个则为True
            {
                //也即跨线程访问
                while (!this.listView_online.IsHandleCreated)
                {
                    //解决窗体关闭时出现“访问已释放句柄“的异常
                    if (this.listView_online.Disposing || this.listView_online.IsDisposed)
                        return;
                }
                RemoveItemFromOnline del = new RemoveItemFromOnline(RemoveItemFromListViewOnline);
                this.listView_online.Invoke(del, item);
            }
            else
            {
                for (int i = 0; i < this.listView_online.Items.Count; i++)
                {
                    if (this.listView_online.Items[i].Text.Equals(item))
                    {
                        this.listView_online.Items.RemoveAt(i);
                    }
                }
               
            }
        }
        #endregion

        public FormServer()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            HashSet<string> set = IPUtils.GetLocalHost();
            listBox_IP.Items.AddRange(set.ToArray());
            listBox_IP.SelectedIndex = 0;
            delLog = new AddItemToListView(AddItemToListViewLog);
            delOnline = new AddItemToListView(AddItemToListViewOnline);
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            try
            {
                //当点击开始监听的时候 在服务器端创建一个负责监听IP地址和端口号的Socket
                networkManagement.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //获取ip地址
                IPAddress ip = IPAddress.Parse(listBox_IP.SelectedItem.ToString());
                //创建端口号
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(numericUpDown_Port.Value));
                //绑定IP地址和端口号
                networkManagement.serverSocket.Bind(point);
                //开始监听:设置最大可以同时连接多少个请求
                networkManagement.serverSocket.Listen(10);
                //记录日志信息
                ListViewItem item = new ListViewItem(new string[] { DateTime.Now.ToString(), "监听"+ networkManagement.serverSocket.LocalEndPoint+"成功" });
                item.ForeColor = Color.Green;
                delLog(item);
                //禁用按钮
                btnListen.Enabled = false;
                OnReceiveMsg += new ChessEventReceiveHander(ManageChessEvent);

                //负责监听客户端的线程:创建一个监听线程  
                Thread threadwatch = new Thread(WaitConnect);
                //将窗体线程设置为与后台同步，随着主线程结束而结束  
                threadwatch.IsBackground = true;
                //启动线程     
                threadwatch.Start();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        //监听客户端发来的请求  
        private void WaitConnect()
        {
            Socket connection = null;
            //持续不断监听客户端发来的连接请求     
            while (true)
            {
                try
                {
                    connection = networkManagement.serverSocket.Accept();
                    connection.NoDelay = true;
                    connection.DontFragment = true;
                }
                catch (Exception ex)
                {
                    //提示套接字监听异常     
                    ListViewItem temp = new ListViewItem(new string[] { DateTime.Now.ToString(), ex.Message });
                    temp.ForeColor = Color.Red;
                    delLog(temp);
                    //Console.WriteLine(ex.Message);
                    break;
                }

                //获取客户端的IP和端口号  
                IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (connection.RemoteEndPoint as IPEndPoint).Port;

                //客户端网络结点号  
                string remoteEndPoint = connection.RemoteEndPoint.ToString();

                //显示与客户端连接情况到log
                ListViewItem item = new ListViewItem(new string[] { DateTime.Now.ToString(), "与" + remoteEndPoint + "客户端建立连接！" });
                item.ForeColor = Color.Blue;
                delLog(item);

                //新建一个初始化消息对象
                Message init = new Message();
                init.Action = Message.ID_STATUS_INIT;
                init.ExtraMsg = "欢迎来到四子棋游戏！";
                init.IsGameOver = false;
                ListViewItem it;
                try
                {
                    //加入到房间
                    if (networkManagement.room == null)
                    {
                        networkManagement.room = new GameRoom();
                        networkManagement.room.RoomID = networkManagement.roomList.Count + 1;
                        networkManagement.room.FirstSocket = connection;
                        init.Color = networkManagement.room.FirstColor;

                        //init.Receiver = room.FirstHead;
                        //init.Sender = room.SecondHead;
                        connection.Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(init)));

                        ListViewItem it3 = new ListViewItem(new string[] { DateTime.Now.ToString(), remoteEndPoint + "创建房间，房间号" + networkManagement.room.RoomID + "," + "正在等待棋友加入" });
                        delLog(it3);

                        //添加客户端信息  
                        networkManagement.onlineList.Add(networkManagement.room.FirstUUID, connection);
                        it = new ListViewItem(new string[] { networkManagement.room.FirstUUID, remoteEndPoint, DateTime.Now.ToString() });
                    }
                    else
                    {
                        networkManagement.room.SecondSocket = connection;
                        init.Color = networkManagement.room.SecondColor;
                        //init.Sender = room.FirstHead;
                        //init.Receiver = room.SecondHead;
                        connection.Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(init)));
                        networkManagement.roomList.Add(networkManagement.room);
                        Message pp = new Message();
                        pp.Action = Message.ID_STATUS_PP;
                        pp.Winner = "暂无";
                        pp.Sender = networkManagement.room.SecondUUID;
                        pp.Receiver = networkManagement.room.FirstUUID;
                        pp.WhoseTurn = networkManagement.room.WhoseTurn;
                        pp.Color = networkManagement.room.FirstColor;
                        networkManagement.room.FirstSocket.Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(pp)));
                        pp.Sender = networkManagement.room.FirstUUID;
                        pp.Receiver = networkManagement.room.SecondUUID;
                        pp.Color = networkManagement.room.SecondColor;
                        networkManagement.room.SecondSocket.Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(pp)));

                        ListViewItem it2 = new ListViewItem(new string[] { DateTime.Now.ToString(), remoteEndPoint + "进入房间，房间号" + networkManagement.roomList.Count });
                        delLog(it2);

                        //添加客户端信息  
                        networkManagement.onlineList.Add(networkManagement.room.SecondUUID, connection);
                        it = new ListViewItem(new string[] { networkManagement.room.SecondUUID, remoteEndPoint, DateTime.Now.ToString() });

                        networkManagement.room = null;
                    }
                }
                catch (Exception ex)
                {
                    
                    throw ex;
                }
               

                //更新到界面上在线列表
                AddItemToListViewOnline(it);

                //创建一个通信线程      
                ParameterizedThreadStart pts = new ParameterizedThreadStart(Receive);
                Thread thread = new Thread(pts);
                //设置为后台线程，随着主线程退出而退出 
                thread.IsBackground = true;
                //启动线程     
                thread.Start(connection);
            }
        }

        /// <summary>
        /// 接收客户端发来的信息
        /// </summary>
        /// <param name="socketclientpara">客户端套接字对象</param>    
        private void Receive(object socketclientpara)
        {
            Socket socketServer = socketclientpara as Socket;
            while (true)
            {
                //创建一个内存缓冲区，其大小为BUFFER_SIZE字节  即10KB
                byte[] buffer = new byte[networkManagement.BUFFER_SIZE];    
                try
                {
                    int len;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        do
                        {
                            //Receive方法是阻塞式接收数据
                            //流中没有数据时会阻塞
                            //将接收到的信息存入到内存缓冲区，并返回其字节数组的长度
                            len = socketServer.Receive(buffer, networkManagement.BUFFER_SIZE, SocketFlags.None);
                            ms.Write(buffer, 0, len);
                            if (socketServer.Available <= 0)
                            {
                                Thread.Sleep(50);
                            }
                            //可以利用Available属性进行循环读取
                        } while (socketServer.Available > 0);
                        buffer = ms.GetBuffer();
                    }

                    //将套接字获取到的字符数组转换为人可以看懂的字符串  
                    string xml = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    Message mes = XmlUtils.DeserializeObject<Message>(xml);
                    OnReceiveMsg(this, mes);
                }
                catch (Exception ex)
                {
                    //如果发生异常，说明客户端已经关闭了连接或者反序列化出错
                    string uuid = "", target = "";
                    networkManagement.FindExceptionSocket(socketServer,ref networkManagement.room, ref networkManagement.roomList, ref uuid, ref target);

                    RemoveItemFromListViewOnline(uuid);
                    Console.WriteLine("Client Count:" + networkManagement.onlineList.Count);

                    //提示套接字监听异常   
                    //Console.WriteLine("客户端" + socketServer.RemoteEndPoint + "已经中断连接" + "\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n");
                    //记录日志信息
                    try
                    {
                        ListViewItem item = new ListViewItem(new string[] { DateTime.Now.ToString(), "客户端" + socketServer.RemoteEndPoint + "已经中断连接" + "。" + ex.Message });
                        item.ForeColor = Color.Red;
                        delLog(item);
                    }
                    catch (Exception)
                    {

                        //throw;
                    }
                    
                    //自定义一个消息，让manageChessEvent去处理
                    Message msgOffline = new Message();
                    msgOffline.Action = Message.ID_STATUS_OFFLINE;
                    msgOffline.Sender = uuid;
                    msgOffline.Receiver = target;
                    msgOffline.ExtraMsg = "对方已掉线";
                    try
                    {
                        networkManagement.onlineList[target].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(msgOffline)));
                        networkManagement.onlineList.Remove(target);
                    }
                    catch (Exception exs)
                    {

                        Console.WriteLine(exs.Message);
                    }
                    
                    //关闭之前accept出来的和客户端进行通信的套接字 
                    socketServer.Close();
                    break;
                }
            }
        }

        /// <summary>
        /// 管理消息事件，根据不同的Action对消息进行不同的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManageChessEvent(object sender, Message e)
        {
            Console.WriteLine(e.Action);
            switch (e.Action)
            {
                case Message.ID_STATUS_INIT:
                    break;
       
                case Message.ID_STATUS_MSG:
                    string msgContent3 = string.Format("[{0}({1})]对[{2}]说【{3}】", e.Sender, e.Name, e.Receiver, e.ExtraMsg);
                    ListViewItem it3 = new ListViewItem(new string[] { DateTime.Now.ToString(), msgContent3 });
                    it3.ForeColor = Color.DarkRed;
                    delLog(it3);
                    try
                    {
                        networkManagement.onlineList[e.Receiver].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(e)));
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                    break;
                case Message.ID_STATUS_OVER:

                    break;
                case Message.ID_STATUS_PP:
                    break;
                case Message.ID_STATUS_BACKREQUEST:
                    networkManagement.onlineList[e.Receiver].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(e)));
                    break;
                case Message.ID_STATUS_BACK:
                    if (e.BPieces.Count <= 0 || e.APieces.Count <= 0)
                    {
                        e.IsUpdateBoard = false;
                        e.ExtraMsg = "没走两步就想悔棋？没门！";
                        e.IsSysMsg = true;
                        e.Name = "悔棋失败";
                        networkManagement.onlineList[e.Sender].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(e)));
                        return;
                    }
                    else
                    {
                        e.IsUpdateBoard = true;
                        e.ExtraMsg = "成功撤销";
                        e.IsSysMsg = true;
                        e.Name = "悔棋提示";

                        networkManagement.onlineList[e.Receiver].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(e)));
                    }
                    break;
                case Message.ID_STATUS_PUT:
                    Message outMsg,upMsg;
                    bool isOver=ChessFinishiUtils.CheckGameOver(e,out outMsg,out upMsg);
                    //无论是否结束都要更新另一方的棋盘
                    networkManagement.onlineList[upMsg.Receiver].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(upMsg)));
                    //如果结束，还要向两方发游戏结束的消息
                    if (isOver)
                    {
                        if (outMsg.Winner==Message.OPPONENT_NONE)
                        {
                            outMsg.ExtraMsg = "平局";
                            networkManagement.onlineList[e.Receiver].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(outMsg)));
                            networkManagement.onlineList[e.Sender].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(outMsg)));
                        }
                        else
                        {
                            //先向胜利者发消息
                            outMsg.ExtraMsg = "您胜利了";
                            networkManagement.onlineList[outMsg.Receiver].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(outMsg)));
                            //交换一下发件人，向失败者发消息
                            //outMsg.Sender = e.Sender;
                            //outMsg.Receiver = e.Receiver;
                            outMsg.ExtraMsg = "不要气馁";
                            networkManagement.onlineList[outMsg.Sender].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(outMsg)));
                        }

                    }
                   
                    break;
                case Message.ID_STATUS_OFFLINE:
                    break;
                case Message.ID_STATUS_UPDATEBOARD:
                    networkManagement.onlineList[e.Receiver].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(e)));
                    networkManagement.onlineList[e.Sender].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(e)));
                    break;
                case Message.ID_STATUS_STARTREQUEST:
                    e.ExtraMsg = "对方想要重新开始游戏，您是否愿意？";
                    networkManagement.onlineList[e.Receiver].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(e)));
                    break;
                case Message.ID_STATUS_START:
                    Message msg = new Message();
                    if (e.IsReStartAgree)
                    {
                        GameRoom r = new GameRoom(e.Sender, e.Receiver);
                        r.RoomID = networkManagement.roomList.Count;
                        networkManagement.roomList.Add(r);

                        msg.Action = Message.ID_STATUS_PP;
                        msg.Sender = e.Receiver;
                        msg.Receiver = e.Sender;
                        msg.Color = r.SecondColor;
                        msg.Winner = "暂无";
                        msg.WhoseTurn = r.WhoseTurn;
                        msg.IsUpdateBoard = true;
                        networkManagement.onlineList[e.Sender].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(msg)));
                        msg.Sender = e.Sender;
                        msg.Receiver = e.Receiver;
                        msg.Color = r.FirstColor;
                        networkManagement.onlineList[e.Receiver].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(msg)));
                    }
                    else
                    {
                        msg.Action = Message.ID_STATUS_START;
                        msg.IsReStartAgree = false;
                        networkManagement.onlineList[e.Receiver].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(msg)));
                    }
                    break;

                case Message.ID_STATUS_MSGREFUSED:
                    e.ExtraMsg = "消息已发出，但被对方拒收了";
                    e.IsSysMsg = true;
                    networkManagement.onlineList[e.Receiver].Send(Encoding.UTF8.GetBytes(XmlUtils.XmlSerializer<Message>(e)));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 刷新在线玩家列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(() => progressBar.Invoke(new Action(() =>
            {
                progressBar.Maximum = networkManagement.onlineList.Count + listView_online.Items.Count;
                progressBar.Value = 0;
                foreach (var item in networkManagement.onlineList.ToList())
                {
                    progressBar.Value++;
                    if (this.networkManagement.IsAlive(item.Value) == false)
                    {
                        networkManagement.onlineList.Remove(item.Key);
                    }
                }
                for (int i = 0; i < listView_online.Items.Count; i++)
                {
                    progressBar.Value++;
                    if (networkManagement.onlineList.ContainsKey(listView_online.Items[i].Text) == false)
                    {
                        listView_online.Items.RemoveAt(i);
                    }
                }
            })));
            t.Start();
            ListViewItem item2 = new ListViewItem(new string[] { DateTime.Now.ToString(), "已更新客户端列表" });
            delLog(item2);
        }
        /// <summary>
        /// 清除日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            listView_log.Items.Clear();
        }
        /// <summary>
        /// 强制下线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView_online.SelectedItems.Count; i++)
            {
                string ssid = listView_online.SelectedItems[i].Text;
                networkManagement.SafeClose(networkManagement.onlineList[ssid]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormPVE pve = new FormPVE();
            pve.Show();
        }
    }
}
