//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;

//public class Solution
//{
//    public const double BallDiameter = 2;
//    public int solution(double[,] objectBallPosList, double[] hitVector)
//    {
//        int result = -1;

//        int ballCount = objectBallPosList.GetLength(0);
//        double cueX = 0;
//        double cueY = 0; 
//        double directionX = hitVector[0];
//        double directionY = hitVector[1];

//        double magnitude = Math.Sqrt(directionX * directionX + directionY * directionY);
//        directionX /= magnitude;
//        directionY /= magnitude;
//        for (int time = 0; time < 100; time++)
//        {

//            cueX += directionX * 0.2f;
//            cueY += directionY * 0.2f;

//            for (int i = 0; i < ballCount; i++)
//            {
//                double ballX = objectBallPosList[i, 0] - cueX;
//                double ballY = objectBallPosList[i, 1] - cueY;

//                double distance = Math.Abs(directionY * ballX - directionX * ballY) /
//                             Math.Sqrt(directionX * directionX + directionY * directionY);
//                if (distance > BallDiameter * 2)
//                    continue;

//                double distanceSquared = (ballX * ballX) + (ballY * ballY);


//                if (distanceSquared < BallDiameter * BallDiameter)
//                {
//                    result = i;
//                    break;
//                }
               
//            }
//            if (result != -1)
//                break;

//        }
        

//        return result;
//    }
//}
