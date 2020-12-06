using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// 判定输赢的类
    /// 四个相同颜色棋子连成一线即为结束
    /// </summary>
    class ChessFinishiUtils
    {
        /// <summary>
        ///接受一个message,返回新的message
        ///out 传址, 用于返回多个数据
        /// </summary>
        /// <param name="e"></param>
        /// <param name="resMsg"></param>
        /// <param name="updateMsg"></param>
        /// <returns></returns>
        public static bool CheckGameOver(Message e, out Message resMsg, out Message updateMsg)
        {
            Message upMsg = new Message();
            //构造棋盘更新的消息
            upMsg.Action = Message.ID_STATUS_UPDATEBOARD;
            upMsg.IsGameOver = false;
            upMsg.WhoseTurn = e.WhoseTurn == Message.OPPONENT_B ? Message.OPPONENT_A : Message.OPPONENT_B;
            upMsg.BPieces = e.BPieces;
            upMsg.APieces = e.APieces;
            upMsg.AColorQ = e.AColorQ;
            upMsg.BColorQ = e.BColorQ;
            upMsg.Sender = e.Sender;
            upMsg.Receiver = e.Receiver;
            upMsg.IsUpdateBoard = true;
            upMsg.Color = e.Color == Message.OPPONENT_B ? Message.OPPONENT_A : Message.OPPONENT_B;
            //如果结束则构造结束消息
            Message msg = new Message();
            if (e.IsGameOver || e.BPieces.Count + e.APieces.Count == Message.MAX_LINE_COUNT * Message.MAX_LINE_COUNT)
            {
                msg.Action = Message.ID_STATUS_OVER;
                msg.IsGameOver = true;
                msg.WhoseTurn = Message.OPPONENT_NONE;
                msg.Receiver = e.Sender;
                msg.Sender = e.Receiver;
                msg.APieces = e.APieces;
                msg.BPieces = e.BPieces;
                msg.AColorQ = e.AColorQ;
                msg.BColorQ = e.BColorQ;
                if (e.IsGameOver)
                {
                    msg.Winner = e.Winner;
                }
                else
                {
                    msg.Winner = Message.OPPONENT_NONE;
                }

                resMsg = msg;
                updateMsg = upMsg;
                return true;
            }
            resMsg = msg;
            updateMsg = upMsg;
            return false;
        }

        private const int MAX_COUNT_IN_LINE = 4;

        public static bool checkWin(List<Chess> chesses)
        {
            foreach (Chess chess in chesses)
            {
                int x = chess._point.X;
                int y = chess._point.Y;
                int _color = chess.color;
                bool isWin = ChessFinishiUtils.checkHorizontal(x, y, _color, chesses);
                if (isWin)
                {
                    return true;
                }
                isWin = ChessFinishiUtils.checkVertical(x, y, _color, chesses);
                if (isWin)
                {
                    return true;
                }
                isWin = ChessFinishiUtils.checkMainDiagonal(x, y, _color, chesses);
                if (isWin)
                {
                    return true;
                }
                isWin = ChessFinishiUtils.checkMinorDiagonal(x, y, _color, chesses);
                if (isWin)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool checkVertical(int x, int y, int color, List<Chess> chesses)
        {
            int count = 1;
            for (int i = 1; i < MAX_COUNT_IN_LINE; i++)
            {
                if (chesses.Contains(new Chess(x, y - i, color)))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            if (count == MAX_COUNT_IN_LINE)
            {
                return true;
            }
            count = 1;
            for (int i = 1; i < MAX_COUNT_IN_LINE; i++)
            {
                if (chesses.Contains(new Chess(x, y + i, color)))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            if (count == MAX_COUNT_IN_LINE)
            {
                return true;
            }
            return false;
        }

        public static bool checkHorizontal(int x, int y, int color, List<Chess> chesses)
        {
            int count = 1;
            for (int i = 1; i < MAX_COUNT_IN_LINE; i++)
            {
                if (chesses.Contains(new Chess(x - i, y, color)))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            if (count == MAX_COUNT_IN_LINE)
            {
                return true;
            }
            count = 1;
            for (int i = 1; i < MAX_COUNT_IN_LINE; i++)
            {
                if (chesses.Contains(new Chess(x + i, y, color)))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            if (count == MAX_COUNT_IN_LINE)
            {
                return true;
            }
            return false;
        }

        public static bool checkMainDiagonal(int x, int y, int color, List<Chess> chesses)
        {
            int count = 1;
            for (int i = 1; i < MAX_COUNT_IN_LINE; i++)
            {
                if (chesses.Contains(new Chess(x - i, y + i, color)))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            if (count == MAX_COUNT_IN_LINE)
            {
                return true;
            }
            count = 1;
            for (int i = 1; i < MAX_COUNT_IN_LINE; i++)
            {
                if (chesses.Contains(new Chess(x + i, y - i, color)))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            if (count == MAX_COUNT_IN_LINE)
            {
                return true;
            }
            return false;
        }

        public static bool checkMinorDiagonal(int x, int y, int color, List<Chess> chesses)
        {
            int count = 1;
            for (int i = 1; i < MAX_COUNT_IN_LINE; i++)
            {
                if (chesses.Contains(new Chess(x - i, y - i, color)))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            if (count == MAX_COUNT_IN_LINE)
            {
                return true;
            }
            count = 1;
            for (int i = 1; i < MAX_COUNT_IN_LINE; i++)
            {
                if (chesses.Contains(new Chess(x + i, y + i, color)))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            if (count == MAX_COUNT_IN_LINE)
            {
                return true;
            }
            return false;
        }
    }
}
