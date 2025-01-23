//using System;
//using System.Collections.Generic;

//public class Solution
//{
//    public int solution(int[,] quest)
//    {
//        int result = 0;
//        int exp = 0;
//        int count = quest.GetLength(0);

//        var questList = new List<KeyValuePair<int, int>>();
//        for (int i = 0; i < count; i++)
//        {
//            questList.Add(new KeyValuePair<int, int>(quest[i, 0], quest[i, 1]));
//        }
//        questList.Sort((a, b) =>
//        {
//            if (a.Key == b.Key)
//                return a.Value.CompareTo(b.Value);
//            return a.Key.CompareTo(b.Key);
//        });


//        foreach (var _quest in questList)
//        {
//            int requiredExp = _quest.Key;  
//            int rewardExp = _quest.Value;

//            if (exp >= requiredExp) 
//            {
//                exp += rewardExp;
//                result++;
//            }
//        }


//        return result;
//    }
//}