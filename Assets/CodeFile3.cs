////using System;
////using System.Collections.Generic;

////public class Solution
////{
////    public int solution(string[] maps)
////    {
////        int result = -1;
////        var rooms = new Dictionary<(int, int), string>();
////        var roomInfo = new Dictionary<(int, int), int>();
////        for (int i = 0; i < maps.Length; i++)
////        {
////            for (int j = 0; j < maps[j].Length; j++)
////            {
////                var _room = maps[i].Substring(j, 1);
////                rooms.Add((i, j), (_room));
////                roomInfo.Add((i, j), 0);
////            }
////        }
////        Queue<(int, int)> queue = new Queue<(int, int)>();
////        var isLever = false;
////        var start = new Dictionary<(int, int), string>();
////        var lever = new Dictionary<(int, int), string>();
////        var end = new Dictionary<(int, int), string>();
////        foreach (var room in rooms)
////        {
////            if (room.Value == "S")
////            {
////                start.Add(room.Key, room.Value);
////                roomInfo[(room.Key.Item1, room.Key.Item2)] = 1;
////                queue.Enqueue(room.Key);
////            }
////            if (room.Value == "X")
////            {
////                roomInfo[(room.Key.Item1, room.Key.Item2)] = -1;
////            }

////            if (room.Value == "L")
////            {
////                lever.Add(room.Key, room.Value);
////            }
////            if (room.Value == "E")
////            {
////                end.Add(room.Key, room.Value);
////            }
////        }

////        while (queue.Count > 0)
////        {
////            var current = queue.Dequeue();
////            if (!roomInfo.TryGetValue((current.Item1, current.Item2 + 1), out var rCount))
////            {
////                rCount = -1;
////            }
////            if (!roomInfo.TryGetValue((current.Item1, current.Item2 - 1), out var lCount))
////            {
////                lCount = -1;
////            }
////            if (!roomInfo.TryGetValue((current.Item1 + 1, current.Item2), out var uCount))
////            {
////                uCount = -1;
////            }
////            if (!roomInfo.TryGetValue((current.Item1 - 1, current.Item2), out var dCount))
////            {
////                dCount = -1;
////            }
////            bool r = rCount != -1;
////            bool l = lCount != -1;
////            bool u = uCount != -1;
////            bool d = dCount != -1;

////            if (!r && !l && !u && !d)
////                return -1;

////            if (r)
////            {
////                if (l)
////                {

////                }
////                else if (u)
////                {

////                }
////                else if (d)
////                {

////                }
////            }

////        }



////        return result;
////    }

////    public (int, int) GetWayPoint((int, int) current, bool r, bool l, bool u, bool d)
////    {
////        var way = current;

////        return way;
////    }
////}




//using System;
//using System.Collections.Generic;

//public class Solution
//{
//    public int solution(string[] maps)
//    {
//        int rows = maps.Length;
//        int cols = maps[0].Length;


//        int[] start = { -1, -1 };
//        int[] lever = { -1, -1 };
//        int[] exit = { -1, -1 };

//        for (int i = 0; i < rows; i++)
//        {
//            for (int j = 0; j < cols; j++)
//            {
//                if (maps[i][j] == 'S')
//                {
//                    start[0] = i;
//                    start[1] = j;
//                }

//                if (maps[i][j] == 'L')
//                {
//                    lever[0] = i;
//                    lever[1] = j;
//                }

//                if (maps[i][j] == 'E')
//                {
//                    exit[0] = i;
//                    exit[1] = j;
//                }
//            }
//        }

        

//        int toLever = GetMovePoint(start[0], start[1], lever[0], lever[1], rows, cols, maps);
//        if (toLever == -1) return -1;

//        int toExit = GetMovePoint(lever[0], lever[1], exit[0], exit[1], rows, cols, maps);
//        if (toExit == -1) return -1;

//        return toLever + toExit;
//    }

//    int GetMovePoint(int startX, int startY, int targetX, int targetY, int rows, int cols, string[] maps)
//    {

//        int[] dx = { -1, 1, 0, 0 };
//        int[] dy = { 0, 0, -1, 1 };

//        var queue = new Queue<List<int>>();
//        bool[,] visited = new bool[rows, cols];
//        var _start = new List<int>();
//        _start.Add(startX);
//        _start.Add(startY);
//        _start.Add(0);
//        queue.Enqueue(_start);
//        visited[startX, startY] = true;

//        while (queue.Count > 0)
//        {
//            var info = new List<int>();
//            info.AddRange(queue.Dequeue());

//            if (info[0] == targetX && info[1] == targetY) return info[2];

//            for (int i = 0; i < 4; i++)
//            {
//                int nx = info[0] + dx[i];
//                int ny = info[1] + dy[i];

//                if (nx >= 0 && nx < rows && ny >= 0 && ny < cols &&
//                    !visited[nx, ny] && maps[nx][ny] != 'X')
//                {
//                    visited[nx, ny] = true;
//                    var _enqueue = new List<int>();
//                    _enqueue.Add(nx);
//                    _enqueue.Add(ny);
//                    _enqueue.Add(info[2] + 1);
//                    queue.Enqueue(_enqueue);
//                }
//            }
//        }

//        return -1;
//    }
//}

////using System;
////using System.Collections.Generic;

////public class Solution
////{
////    public int solution(string[] maps)
////    {
////        int rows = maps.Length;
////        int cols = maps[0].Length;

////        // 이동 방향 (상, 하, 좌, 우)
////        int[] dx = { -1, 1, 0, 0 };
////        int[] dy = { 0, 0, -1, 1 };

////        // 미로의 시작점, 레버, 출구 좌표 찾기
////        (int, int) start = (-1, -1);
////        (int, int) lever = (-1, -1);
////        (int, int) exit = (-1, -1);

////        for (int i = 0; i < rows; i++)
////        {
////            for (int j = 0; j < cols; j++)
////            {
////                if (maps[i][j] == 'S') start = (i, j);
////                if (maps[i][j] == 'L') lever = (i, j);
////                if (maps[i][j] == 'E') exit = (i, j);
////            }
////        }

////        // BFS 함수 정의 (Dictionary 사용)
////        int BFS((int, int) start, (int, int) target)
////        {
////            var queue = new Queue<(int, int)>();
////            var distances = new Dictionary<(int, int), int>();

////            // 시작점 초기화
////            queue.Enqueue(start);
////            distances[start] = 0;

////            while (queue.Count > 0)
////            {
////                var (x, y) = queue.Dequeue();

////                // 목표에 도달하면 거리 반환
////                if ((x, y) == target) return distances[(x, y)];

////                // 네 방향으로 이동
////                for (int i = 0; i < 4; i++)
////                {
////                    int nx = x + dx[i];
////                    int ny = y + dy[i];

////                    // 범위 안에 있고 방문하지 않았으며 통로일 경우 이동
////                    if (nx >= 0 && nx < rows && ny >= 0 && ny < cols &&
////                        !distances.ContainsKey((nx, ny)) && maps[nx][ny] != 'X')
////                    {
////                        distances[(nx, ny)] = distances[(x, y)] + 1;
////                        queue.Enqueue((nx, ny));
////                    }
////                }
////            }

////            // 도달할 수 없는 경우
////            return -1;
////        }

////        // 1. 시작점에서 레버까지 이동
////        int toLever = BFS(start, lever);
////        if (toLever == -1) return -1;

////        // 2. 레버에서 출구까지 이동
////        int toExit = BFS(lever, exit);
////        if (toExit == -1) return -1;

////        // 총 이동 시간 반환
////        return toLever + toExit;
////    }
////}
