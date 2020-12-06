using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameRoom
    {
        public int RoomID { get; set; }
        public string FirstUUID { get; set; }
        public string SecondUUID { get; set; }
        public Socket FirstSocket { get; set; }
        public Socket SecondSocket { get; set; }
        public string WhoseTurn { get; set; }//轮到谁
        public string FirstColor { get; set; }
        public string SecondColor { get; set; }
        public string FirstHead { get; set; }
        public string SecondHead { get; set; }
        /// <summary>
        /// 指定两个人创建一个房间
        /// </summary>
        /// <param name="firstUUID"></param>
        /// <param name="secondUUID"></param>
        public GameRoom(string firstUUID,string secondUUID)
        {
            this.FirstUUID = firstUUID;
            this.SecondUUID = secondUUID;
            Random r = new Random();
            FirstHead = string.Format("http://pics.sc.chinaz.com/Files/pic/icons128/7066/b{0}.png", r.Next(17));
            SecondHead = string.Format("http://pics.sc.chinaz.com/Files/pic/icons128/7066/b{0}.png", r.Next(17));
            //随机设置先手后手代表的颜色
            if (r.Next(2) == 0)
            {
                FirstColor = Message.OPPONENT_B ;
                SecondColor = Message.OPPONENT_A ;
            }
            else
            {
                FirstColor = Message.OPPONENT_A ;
                SecondColor = Message. OPPONENT_B ;
            }
            //随机设置先手
            WhoseTurn = r.Next(2) > 0 ? Message.OPPONENT_A  : Message.OPPONENT_B ;
        }
        /// <summary>
        /// 一方创建房间等待另一方加入
        /// </summary>
        public GameRoom()
        {
            //设置UUID，通过UUID来识别不同的socket
            FirstUUID = Guid.NewGuid().ToString("N");
            SecondUUID = Guid.NewGuid().ToString("N");
            Random r = new Random();

            FirstHead = string.Format("http://pics.sc.chinaz.com/Files/pic/icons128/7066/b{0}.png", r.Next(17));
            SecondHead = string.Format("http://pics.sc.chinaz.com/Files/pic/icons128/7066/b{0}.png", r.Next(17));
            //随机设置先手后手代表的颜色
            if (r.Next(2)==0)
            {
                FirstColor = Message.OPPONENT_B ;
                SecondColor = Message.OPPONENT_A ;
            }
            else
            {
                FirstColor = Message.OPPONENT_A ;
                SecondColor = Message.OPPONENT_B ;
            }
            //随机设置先手
            WhoseTurn = r.Next(2) > 0 ? Message.OPPONENT_A  : Message.OPPONENT_B ;

        }
    }
}
