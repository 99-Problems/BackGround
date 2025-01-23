//using System;
//using System.Collections.Generic;

//public class Solution
//{
//    public int solution(int[,] field, int farmSize)
//    {
//        int result = 0;

//        int rows = field.GetLength(0);
//        int cols = field.GetLength(1);

//        int maxStones = rows*cols; // 전체 영역 max

//        var minStones = maxStones; 

//        for (int i = 0; i <= rows - farmSize; i++)
//        {
//            for (int j = 0; j <= cols - farmSize; j++)
//            {
//                int stones = 0;
//                bool isMushroom = false;

//                for (int row = 0; row < farmSize; row++)
//                {
//                    for (int col = 0; col < farmSize; col++)
//                    {
//                        int curCell = field[i + row, j + col];

//                        if (curCell == 2) 
//                        {
//                            isMushroom = true;
//                            break;
//                        }
//                        else if (curCell == 1) 
//                        {
//                            stones++;
//                        }
//                    }
//                    if (isMushroom) break;
//                }

//                if (!isMushroom)
//                {
//                    minStones = Math.Min(minStones, stones);
//                }
//            }
//        }

//        result = minStones == maxStones ? -1 : minStones;
//        return result;
//    }
//}
