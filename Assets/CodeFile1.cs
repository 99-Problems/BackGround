////using System;
////using System.Collections.Generic;
////using System.Linq;

////public class Solution
////{
////    public int solution(int n, int[,] edge)
////    {
////        int answer = 0;
////        var nodes = new Dictionary<int, int[,]>();
////        for (int i = 1; i <= edge.GetLength(0); i++)
////        {
////            nodes.Add(i, edge);
////        }
       
////        return answer;
////    }
////}


//using System;
//using System.Collections.Generic;

//public class Solution
//{
//    public int solution(int n, int[,] edge)
//    {
//        int answer = 0;

//        // 인접 리스트 생성
//        List<int>[] nodes = new List<int>[n + 1];
//        for (int i = 1; i <= n; i++)
//        {
//            nodes[i] = new List<int>();
//        }

//        for (int i = 0; i < edge.GetLength(0); i++)
//        {
//            int from = edge[i, 0];
//            int to = edge[i, 1];
//            nodes[from].Add(to);
//            nodes[to].Add(from);
//        }

//        // 최단 거리 배열 초기화
//        int[] distances = new int[n + 1];
//        Array.Fill(distances, -1);
//        distances[1] = 0;

//        // BFS로 최단 거리 계산
//        Queue<int> queue = new Queue<int>();
//        queue.Enqueue(1);

//        while (queue.Count > 0)
//        {
//            int current = queue.Dequeue();
//            Console.WriteLine($"current: {current}");

//            foreach (int neighbor in nodes[current])
//            {
//                if (distances[neighbor] == -1) // 방문하지 않은 노드
//                {
//                    distances[neighbor] = distances[current] + 1;
//                    queue.Enqueue(neighbor);
//                    Console.WriteLine($"n : {neighbor} dn: {distances[neighbor]}");
//                }
//            }
//        }

//        // 가장 먼 거리와 그 거리의 노드 개수 찾기
//        int maxDistance = 0;
 

//        foreach (var distance in distances)
//        {
//            Console.WriteLine($"d : {distance}");
//            if (distance > maxDistance)
//            {
//                maxDistance = distance;
//                answer = 1;
//                Console.WriteLine($"a1");
//            }
//            else if (distance == maxDistance)
//            {
//                answer++;
//                Console.WriteLine($"a++");
//            }
//        }

//        return answer;
//    }
//}
