using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CatmullRomSpline : MonoBehaviour
{
    [ReadOnly]
    public Transform[] controlPointsList;

    public bool isLooping = true;

    public float offset;
    public int missionIconOffset = 10;
    public int startOffset = -5;
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            controlPointsList = gameObject.Children().OfComponent<Transform>().ToArray();
#endif
        Gizmos.color = Color.white;
        List<Vector3> paths = new List<Vector3>();
        List<Vector3> point = new List<Vector3>();
        for (int i = 0; i < controlPointsList.Length; i++)
        {
            if ((i == 0 || i == controlPointsList.Length - 2 || i == controlPointsList.Length - 1))
            {
                continue;
            }

            DisplayCatmullRomSpline(i, 0, ref paths, ref point);
        }

        Vector3 prePos = Vector3.negativeInfinity;
        foreach (var mit in paths)
        {
            if (prePos != Vector3.negativeInfinity)
            {
                Gizmos.DrawLine(prePos, mit);
            }

            prePos = mit;
        }

        int j = startOffset;
        foreach (var mit in point)
        {
            if (j % missionIconOffset == 0)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.white;
            }

            Gizmos.DrawSphere(mit, 3);
            j++;
        }

        paths.Clear();
        point.Clear();
        for (int i = 0; i < controlPointsList.Length; i++)
        {
            if ((i == 0 || i == controlPointsList.Length - 2 || i == controlPointsList.Length - 1))
            {
                continue;
            }

            DisplayCatmullRomSpline(i, 720, ref paths, ref point);
        }

        prePos = Vector3.negativeInfinity;
        foreach (var mit in paths)
        {
            if (prePos != Vector3.negativeInfinity)
            {
                Gizmos.DrawLine(prePos, mit);
            }

            prePos = mit;
        }

        foreach (var mit in point)
        {
            if (j % missionIconOffset == 0)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.white;
            }
            Gizmos.DrawSphere(mit, 3);
            j++;
        }
    }

    public void GetPath(ref List<Vector3> paths, ref List<Vector3> point)
    {
        for (int i = 0; i < controlPointsList.Length; i++)
        {
            if ((i == 0 || i == controlPointsList.Length - 2 || i == controlPointsList.Length - 1))
            {
                continue;
            }

            DisplayCatmullRomSpline(i, 0, ref paths, ref point);
        }
    }

    [Button]
    public void Generate()
    {
    }

    void DisplayCatmullRomSpline(int pos, int i1, ref List<Vector3> path, ref List<Vector3> point)
    {
        Vector3 p0 = controlPointsList[ClampListPos(pos - 1)].position;
        Vector3 p1 = controlPointsList[pos].position;
        Vector3 p2 = controlPointsList[ClampListPos(pos + 1)].position;
        Vector3 p3 = controlPointsList[ClampListPos(pos + 2)].position;

        p0.y += i1;
        p1.y += i1;
        p2.y += i1;
        p3.y += i1;
        Vector3 lastPos = p1;
        path.Add(lastPos);
        float resolution = 0.1f;

        int loops = Mathf.FloorToInt(1f / resolution);

        float distance = 0;
        for (int i = 1; i <= loops; i++)
        {
            float t = i * resolution;

            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

            distance += Vector3.Distance(lastPos, newPos);
            lastPos = newPos;
            path.Add(newPos);
        }

        lastPos = Vector3.positiveInfinity;
        loops = (int) (distance / offset);
        resolution = 1.0f / loops;
        for (int i = 1; i <= loops; i++)
        {
            float t = i * resolution;
            float P = 0;
            float b = 0;
            float c = 1;
            float d = 1;
            if (t < d / 2)
            {
                P = outSine(t * 2, b, c / 2, d);
            }
            else
            {
                P = inSine((t * 2) - d, b + c / 2, c / 2, d);
            }

            Vector3 newPos = GetCatmullRomPosition(P, p0, p1, p2, p3);
            if (Vector3.Distance(lastPos, newPos) > offset)
            {
                lastPos = newPos;
                point.Add(lastPos);
            }
        }
    }

    private float inSine(float t, float b, float c, float d)
    {
        return -c * Mathf.Cos(t / d * (Mathf.PI / 2)) + c + b;
    }

    private float outSine(float t, float b, float c, float d)
    {
        return c * Mathf.Sin(t / d * (Mathf.PI / 2)) + b;
    }

    public float inCubic(float t, float b, float c, float d)
    {
        return c * Mathf.Pow(t / d, 3) + b;
    }


    public float outCubic(float t, float b, float c, float d)
    {
        return c * (Mathf.Pow(t / d - 1, 3) + 1) + b;
    }

    int ClampListPos(int pos)
    {
        if (pos < 0)
        {
            pos = controlPointsList.Length - 1;
        }

        if (pos > controlPointsList.Length)
        {
            pos = 1;
        }
        else if (pos > controlPointsList.Length - 1)
        {
            pos = 0;
        }

        return pos;
    }

    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;
    }
}