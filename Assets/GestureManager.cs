using UnityEngine;
using System.Collections.Generic;

public class GestureManager
{
    public event System.EventHandler<GestureCommitEvent> Committed;

    public const int Directions = 8;
    public const float CircleAngle = Mathf.PI * 2f;
    public const float SectorAngle = CircleAngle / Directions; // 每一个方向占用的弧度

    private int _slice = 100;     // 映射表精确度(圆分割次数)
    private float _slicedAngle;   // 切割后的单位弧度
    private int[] _angleVector;   // 弧度->方向映射表
    private Vector2 _lastCapture;

    private List<int> _moves;
    private List<GesturePrefab> _prefabs;

    public GestureManager(int slice)
    {
        if (slice <= Directions * 2)
        {
            Debug.LogWarning("切割数量不能低于" + (Directions * 2));
            slice = Directions * 2;
        }

        _slice = slice;
        _moves = new List<int>(50);
        _prefabs = new List<GesturePrefab>();

        _slicedAngle = CircleAngle / _slice; // 圆切割单位弧度
        _angleVector = new int[_slice];

        for (var it = 0; it < _slice; ++it)
        {
            var atAngle = _slicedAngle * it;
            var direction = (int)((atAngle + SectorAngle * .5f) / SectorAngle) % Directions;

            _angleVector[it] = direction;
        }
    }

    public void AddGesture(string text, int[] data)
    {
        _prefabs.Add(new GesturePrefab() { text = text, data = data });
    }

    public void Begin(Vector2 point)
    {
        _lastCapture = point;
        _moves.Clear();
    }

    public void Update(Vector2 point)
    {
        if (_lastCapture != point)
        {
            var delta = point - _lastCapture;

            var angle = Mathf.Atan2(delta.y, delta.x);

            if (angle < 0f)
                angle += CircleAngle;

            var index = (int)(angle / _slicedAngle);
            var direction = _angleVector[index];

            if (_moves.Count == 0 || direction != _moves[_moves.Count - 1])
                _moves.Add(direction);

            _lastCapture = point;
        }
    }

    public void End(Vector2 point)
    {
        Update(point);

        Debug.Log(string.Format("输入序列:{0}", StringArray(_moves.ToArray())));

        if (_moves.Count > 0)
        {
            var size = _prefabs.Count;

            var index = 0;
            var prefab = _prefabs[index];
            var distance = LevenshteinDistance(_moves, prefab.data);

            var best = prefab;
            var bestDistance = distance;

            for (index = 1; index < size; ++index)
            {
                prefab = _prefabs[index];
                distance = LevenshteinDistance(_moves, prefab.data);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = prefab;
                }

                Debug.Log(string.Format("  >> 匹配目标:{0} 距离: {1} 标准序列: {2}", prefab.text, distance, StringArray(prefab.data)));
            }

            Debug.Log(string.Format("最佳匹配目标:{0}", best.text));

            if (Committed != null)
                Committed(this, new GestureCommitEvent() { prefab = best });
        }
        else
        {
            //Debug.LogError("无法识别");
            if (Committed != null)
                Committed(this, GestureCommitEvent.Empty);
        }
    }

    string StringArray(int[] data)
    {
        var sb = new System.Text.StringBuilder();

        foreach (var i in data)
        {
            sb.Append(i);
        }
        return sb.ToString();
    }

    static int LevenshteinDistance(List<int> s, int[] t)
    {
        // degenerate cases
        //if (s == t) return 0;
        var s_size = s.Count;
        var t_size = t.Length;

        if (s_size == t_size)
        {
            var match = true;

            for (var index = 0; index < s_size; ++index)
            {
                match = s[index] == t[index];
                if (!match)
                    break;
            }
            if (match)
                return -1;
        }

        if (s_size == 0) return t_size;
        if (t_size == 0) return s_size;

        // create two work vectors of integer distances
        int[] v0 = new int[t_size + 1];
        int[] v1 = new int[t_size + 1];

        // initialize v0 (the previous row of distances)
        // this row is A[0][i]: edit distance for an empty s
        // the distance is just the number of characters to delete from t
        for (int i = 0; i < v0.Length; i++)
            v0[i] = i;

        for (int i = 0; i < s_size; i++)
        {
            // calculate v1 (current row distances) from the previous row v0

            // first element of v1 is A[i+1][0]
            //   edit distance is delete (i+1) chars from s to match empty t
            v1[0] = i + 1;

            // use formula to fill in the rest of the row
            for (int j = 0; j < t_size; j++)
            {
                var cost = (s[i] == t[j]) ? 0 : 1;
                v1[j + 1] = Mathf.Min(v1[j] + 1, v0[j + 1] + 1, v0[j] + cost);
            }

            // copy v1 (current row) to v0 (previous row) for next interation
            for (int j = 0; j < v0.Length; j++)
                v0[j] = v1[j];
        }

        return v1[t_size];
    }
}
