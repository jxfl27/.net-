using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Server
{
    class Score
    {
        public Chess[,] chessBoard { get; set; }
        private Chess pChess, aChess;
        public Score(List<Chess> chesses, Chess pChess, Chess aChess)
        {
            this.chessBoard = transBoard(chesses);
            this.pChess = pChess;
            this.aChess = aChess;
        }
        private int S_4 = 100000;
        private int S_H3 = 10000;
        private int S_M3 = 10000;
        private int S_H2 = 1000;
        private int S_M2 = 1000;
        private readonly List<int[]> situation_c = new List<int[]>{
        new int []{ 1, 1, 1 }, new int []{ 2, 2, 2 },//4
        new int []{ 1, 1, 0 }, new int []{ 0, 1, 1 }, new int []{ 2, 2, 0 }, new int []{ 0, 2, 2 }, //h3
        new int []{ 1, 1, 2 }, new int []{ 2, 1, 1 }, new int []{ 2, 2, 1 }, new int []{ 1, 2, 2 }, //m3
        new int []{ 1, 0, 0 }, new int []{ 0, 0, 1 }, new int []{ 0, 1, 0 },
        new int []{ 2, 0, 0 }, new int []{ 0, 0, 2 }, new int []{ 0, 2, 0 }, //h2
        new int []{ 1, 2, 0 }, new int []{ 1, 2, 1 }, new int []{ 0, 2, 1 },
        new int []{ 1, 2, 1 }, new int []{ 2, 1, 0 }, new int []{ 0, 1, 2 },
        new int []{ 2, 1, 0 }, new int []{ 2, 1, 1 }, new int []{ 0, 1, 2 },
        new int []{ 2, 1, 2 }, new int []{ 1, 2, 0 }, new int []{ 0, 2, 1 } //m2
    };
        private readonly List<int []> situation_s = new List<int []>{
        new int []{ 1, 1, 1 }, new int []{ 2, 2, 2}, //4
        new int []{ 1, 1, 0 }, new int []{ 2, 2, 0 }, //h3
        new int []{ 1, 1, 2 }, new int []{ 2, 2, 1 }, //m3
        new int []{ 1, 0, 0 }, new int []{ 1, 0, 1 }, new int []{ 1, 0, 2 }, 
        new int []{ 2, 0, 0 }, new int []{ 2, 0, 1 }, new int []{ 2, 0, 2 }, //h2
        new int []{ 1, 2, 0 }, new int []{ 1, 2, 1 }, new int []{ 1, 2, 2 }, 
        new int []{ 2, 1, 0 }, new int []{ 2, 1, 1 }, new int []{ 2, 1, 2 } //m2
    };

        private int calculate(int[] position, List<int []>situation)
        {
            int sum = 0;
            int index = -1;
            for (int i = 0; i < situation.Count; i++)
            {
                if (same(position, situation[i]))
                {
                    index = i;
                    if (i < 2)
                    {
                        sum += S_4;// 连4棋型
                    }
                    else if (i < 6)
                    {
                        sum += S_H3;// 活3棋型
                    }
                    else if (i < 10)
                    {
                        sum += S_M3;// 眠3棋型
                    }
                    else if (i < 16)
                    {
                        sum += S_H2;// 活2棋型
                    }
                    else if (i < 28)
                    {
                        sum += S_M2;// 眠2棋型
                    }
                }
            }
            if (index < 0)
            {
                sum += 1;// 其它情况
            }
            return sum;
        }
        public Chess[,] transBoard(List<Chess> chesses)
        {
            Chess[,] aBoard = new Chess[15, 15];
            foreach (Chess chess in chesses)
            {
                int x = chess._point.X;
                int y = chess._point.Y;
                aBoard[x, y] = chess;
            }
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (aBoard[i, j] == null)
                    {
                        aBoard[i, j] = new Chess(i + 1, j + 1, 0);
                    }
                }
            }
            return aBoard;
        }

        public int getScore(int x, int y, Chess playerChess, Chess aiChess)
        {
            int sum = 0;
            sum += vertical1Score(x, y, playerChess, aiChess);
            sum += vertical2Score(x, y, playerChess, aiChess);
            sum += horizental1Score(x, y, playerChess, aiChess);
            sum += horizental2Score(x, y, playerChess, aiChess);
            sum += mainDiagonal1Score(x, y, playerChess, aiChess);
            sum += mainDiagonal2Score(x, y, playerChess, aiChess);
            sum += minorDiagonal1Score(x, y, playerChess, aiChess);
            sum += minorDiagonal2Score(x, y, playerChess, aiChess);
            sum += allDirectScore(x, y, playerChess, aiChess);
            return sum;
        }


        public int getIndex(int x, int y, Chess playerChess, Chess aiChess)
        {
            if (chessBoard[x, y].color >= 1 && chessBoard[x, y].color <= 3)
            {
                return 1;
            }
            if (chessBoard[x, y].color >= 4 && chessBoard[x, y].color <= 6)
            {
                return 2;
            }
            else return 0;
        }
        public int vertical1Score(int x, int y, Chess playerChess, Chess aiChess)
        {
            int[] position = new int[3];
            if (y - 1 >= 0)
            {
                position[0] = getIndex(x, y - 1, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (y + 1 < 15)
            {
                position[1] = getIndex(x, y + 1, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (y + 2 < 15)
            {
                position[2] = getIndex(x, y + 2, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            int sum = 0;
            sum += calculate(position, situation_c);
            return sum;
        }
        public int vertical2Score(int x, int y, Chess playerChess, Chess aiChess)
        {
            int[] position = new int[3];
            if (y - 2 >= 0)
            {
                position[0] = getIndex(x, y - 2, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (y - 1 >= 0)
            {
                position[1] = getIndex(x, y - 1, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (y + 1 < 15)
            {
                position[2] = getIndex(x, y + 1, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            int sum = 0;
            sum += calculate(position, situation_c);
            return sum;
        }
        public int horizental1Score(int x, int y, Chess playerChess, Chess aiChess)
        {
            int[] position = new int[3];
            if (x - 2 >= 0)
            {
                position[0] = getIndex(x - 2, y, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x - 1 >= 0)
            {
                position[1] = getIndex(x - 1, y, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x + 1 < 15)
            {
                position[2] = getIndex(x + 1, y, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            int sum = 0;
            sum += calculate(position, situation_c);
            return sum;
        }
        public int horizental2Score(int x, int y, Chess playerChess, Chess aiChess)
        {
            int[] position = new int[3];
            if (x - 1 >= 0)
            {
                position[0] = getIndex(x - 1, y, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x + 1 < 15)
            {
                position[1] = getIndex(x + 1, y, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x + 2 < 15)
            {
                position[2] = getIndex(x + 2, y, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            int sum = 0;
            sum += calculate(position, situation_c);
            return sum;
        }
        public int mainDiagonal1Score(int x, int y, Chess playerChess, Chess aiChess)
        {
            int[] position = new int[3];
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                position[0] = getIndex(x - 1, y - 1, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x + 1 < 15 && y + 1 < 15)
            {
                position[1] = getIndex(x + 1, y + 1, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x + 2 < 15 && y + 2 < 15)
            {
                position[2] = getIndex(x + 2, y + 2, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            int sum = 0;
            sum += calculate(position, situation_c);
            return sum;
        }
        public int mainDiagonal2Score(int x, int y, Chess playerChess, Chess aiChess)
        {
            int[] position = new int[3];
            if (x - 2 >= 0 && y - 2 >= 0)
            {
                position[0] = getIndex(x - 2, y - 2, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                position[1] = getIndex(x - 1, y - 1, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x + 1 < 15 && y + 1 < 15)
            {
                position[2] = getIndex(x + 1, y + 1, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            int sum = 0;
            sum += calculate(position, situation_c);
            return sum;
        }
        public int minorDiagonal1Score(int x, int y, Chess playerChess, Chess aiChess)
        {
            int[] position = new int[3];
            if (x + 1 < 15 && y - 1 >= 0)
            {
                position[0] = getIndex(x + 1, y - 1, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x - 1 >= 0 && y + 1 < 15)
            {
                position[1] = getIndex(x - 1, y + 1, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x - 2 >= 0 && y + 2 < 15)
            {
                position[2] = getIndex(x - 2, y + 2, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            int sum = 0;
            sum += calculate(position, situation_c);
            return sum;
        }
        public int minorDiagonal2Score(int x, int y, Chess playerChess, Chess aiChess)
        {
            int[] position = new int[3];
            if (x + 2 < 15 && y - 2 >= 0)
            {
                position[0] = getIndex(x + 2, y - 2, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x + 1 < 15 && y - 1 >= 0)
            {
                position[1] = getIndex(x + 1, y - 1, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x - 1 >= 0 && y + 1 < 15)
            {
                position[2] = getIndex(x - 1, y + 1, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            int sum = 0;
            sum += calculate(position, situation_c);
            return sum;
        }
        public int allDirectScore(int x, int y, Chess playerChess, Chess aiChess)
        {
            int sum = 0;
            int[] position = new int[3];
            // 上方向
            if (x - 1 >= 0)
            {
                position[0] = getIndex(x - 1, y, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x - 2 >= 0)
            {
                position[1] = getIndex(x - 2, y, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x - 3 >= 0)
            {
                position[2] = getIndex(x - 3, y, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            sum += calculate(position, situation_s);
            // 下方向
            if (x + 1 < 15)
            {
                position[0] = getIndex(x + 1, y, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x + 2 < 15)
            {
                position[1] = getIndex(x + 2, y, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x + 3 < 15)
            {
                position[2] = getIndex(x + 3, y, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            sum += calculate(position, situation_s);
            // 左方向
            if (y - 1 >= 0)
            {
                position[0] = getIndex(x, y - 1, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (y - 2 >= 0)
            {
                position[1] = getIndex(x, y - 2, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (y - 3 >= 0)
            {
                position[2] = getIndex(x, y - 3, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            sum += calculate(position, situation_s);
            // 右方向
            if (y + 1 < 15)
            {
                position[0] = getIndex(x, y + 1, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (y + 2 < 15)
            {
                position[1] = getIndex(x, y + 2, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (y + 3 < 15)
            {
                position[2] = getIndex(x, y + 3, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            sum += calculate(position, situation_s);
            // 左斜上方向
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                position[0] = getIndex(x - 1, y - 1, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x - 2 >= 0 && y - 2 >= 0)
            {
                position[1] = getIndex(x - 2, y - 2, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x - 3 >= 0 && y - 3 >= 0)
            {
                position[2] = getIndex(x - 3, y - 3, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            sum += calculate(position, situation_s);
            // 左斜下方向
            if (x + 1 < 15 && y + 1 < 15)
            {
                position[0] = getIndex(x + 1, y + 1, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x + 2 < 15 && y + 2 < 15)
            {
                position[1] = getIndex(x + 2, y + 2, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x + 3 < 15 && y + 3 < 15)
            {
                position[2] = getIndex(x + 3, y + 3, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            // 右斜上方向
            if (x - 1 >= 0 && y + 1 < 15)
            {
                position[0] = getIndex(x - 1, y + 1, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x - 2 >= 0 && y + 2 < 15)
            {
                position[1] = getIndex(x - 2, y + 2, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x - 3 >= 0 && y + 3 < 15)
            {
                position[2] = getIndex(x - 3, y + 3, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            sum += calculate(position, situation_s);
            // 右斜下
            if (x + 1 < 15 && y - 1 >= 0)
            {
                position[0] = getIndex(x + 1, y - 1, playerChess, aiChess);
            }
            else
            {
                position[0] = 0;
            }
            if (x + 2 < 15 && y - 2 >= 0)
            {
                position[1] = getIndex(x + 2, y - 2, playerChess, aiChess);
            }
            else
            {
                position[1] = 0;
            }
            if (x + 3 < 15 && y - 3 >= 0)
            {
                position[2] = getIndex(x + 3, y - 3, playerChess, aiChess);
            }
            else
            {
                position[2] = 0;
            }
            sum += calculate(position, situation_s);
            return sum;
        }

        public List<int> getPosition()
        {
            int maxi = 0, maxj = 0, maxscore = -1;
            for (int i = 0; i <= 14; i++)
            {
                for (int j = 0; j <= 14; j++)
                {
                    if (chessBoard[i,j].color != 0)
                    {
                        Debug.Write("*");
                    }
                    if (chessBoard[i, j].color == 0)
                    {
                        int tempScore = getScore(i, j, pChess, aChess);
                        Debug.Write("("+tempScore+")");
                        if (maxscore < tempScore)
                        {
                            maxi = i; maxj = j;
                            maxscore = tempScore;
                        }
                    }
                }
                Debug.WriteLine(" ");
            }
            Debug.WriteLine(maxscore);
            return new List<int>{ maxi, maxj};
        }

        bool same(int[]pos, int[] sit)
        {
            int l = Math.Min(pos.Length, sit.Length);
            for(int i = 0; i < l; i++)
            {
                if (pos[i] != sit[i]) return false;
            }
            return true;
        }
    }
}
