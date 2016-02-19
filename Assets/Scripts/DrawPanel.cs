using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class DrawPanel : MonoBehaviour
{
    public float eps = 0.05f;
    public float verticesAccuracy = 0.1f;
    public float successPercent = 80;

    private float _checkingDistance = 50;

    public List<Pattern> patterns;
    private Pattern _currentPattern;

    public GameObject pointer;
    public GameObject tail;
    

    private IEnumerator _pointerMoving;

    public bool debug;

    [HideInInspector] public List<Vector2> drawnPoints;
    private List<GameObject> _debugDrawnPoints;

    public static event Action<Sprite> PatternChanged;
    public static event Action<bool> DrawFinished;
    public static event Action RoundDone; 

    void Start()
    {
        pointer.SetActive(false);
        tail.SetActive(false);

        ChosePattern();
    }

    public void OnDown()
    {
        MovingPointer(true);

    }

    public void OnUp()
    {
        MovingPointer(false);
    }

    void MovingPointer(bool val)
    {
        pointer.SetActive(val);
        tail.SetActive(val);

        if (val)
        {
            if (_pointerMoving == null)
            {
                _pointerMoving = PointerMoving();
                StartCoroutine(_pointerMoving);
            }
        }
        else
        {
            if (_pointerMoving != null)
            {
                StopCoroutine(_pointerMoving);
                _pointerMoving = null;

                CheckPattern();
            }
        }

    }

    IEnumerator PointerMoving()
    {
        drawnPoints = new List<Vector2>();
        if (debug)
        {
            if (_debugDrawnPoints != null && _debugDrawnPoints.Count > 0)
            {
                foreach (GameObject debugDrawnPoint in _debugDrawnPoints)
                {
                    Destroy(debugDrawnPoint);
                }
            }
            _debugDrawnPoints = new List<GameObject>();
        }
        while (true)
        {
            drawnPoints.Add(Input.mousePosition);

            pointer.transform.position = Input.mousePosition;
            float z = tail.transform.position.z;
            tail.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tail.transform.position = new Vector3(tail.transform.position.x, tail.transform.position.y,z);
            yield return null;
        }
    }

    //Вычисляем центры масс исходного многоугольника и нарисованного.
    //    Вычисляем ограничивающий прямоугольних для них.

    //    Приводим многоугольники к одному размеру и в одну точку.

    //    Проверяем была ли проведена линия вблизи мнимой вершины.
    //    Проверяем насколько точно проведены линии вдоль мнимых граней.


    private void CheckPattern()
    {
        List<Vector2> pattern = _currentPattern.vertices;
        
        Vector2 patternCenter = FindCentroid(pattern);

        Rect patternBounds = Bounds(pattern);

        _checkingDistance = verticesAccuracy * (patternBounds.width + patternBounds.height)/2;

        Vector2 drawCenter = FindCentroid(drawnPoints);

        Rect drawBounds = Bounds(drawnPoints);

        float scaleW = patternBounds.width/drawBounds.width;
        float scaleH = patternBounds.height/drawBounds.height;

        float scale = (scaleH + scaleW)/2;

        float dx = patternCenter.x - drawCenter.x;
        float dy = patternCenter.y - drawCenter.y;

        List<Vector2> scaledMovedDraw = new List<Vector2>();

        scaledMovedDraw = ScalePattern(drawnPoints, scale);

        Vector2 scaledDrawCenter = FindCentroid(scaledMovedDraw);

        float sdx = drawCenter.x - scaledDrawCenter.x;
        float sdy = drawCenter.y - scaledDrawCenter.y;

        scaledMovedDraw = MovePattern(scaledMovedDraw, sdx, sdy);

        


        List<Vector2> movedPattern = MovePattern(pattern, -dx, -dy);
        
        List<Vector2> lowerPattern = ScaledMovedPattern(movedPattern, -eps);
        
        List<Vector2> higherPattern = ScaledMovedPattern(movedPattern, eps);


        if (!CheckVertices(movedPattern, scaledMovedDraw))
        {
            SendResultMessage(false);
            return;
        }


        List<Vector2> insPoints = CheckPointsAgainstPattern(higherPattern, scaledMovedDraw, true);

        List<Vector2> badPoints = CheckPointsAgainstPattern(lowerPattern, scaledMovedDraw, true);

        List<Vector2> resultPoints = insPoints.Except(badPoints).ToList();

        if (resultPoints.Count/(float) scaledMovedDraw.Count >= successPercent/100)
        {
            SendResultMessage(true);
            NextRound();
        }
        else
        {
            SendResultMessage(false);
        }

    }

    public Vector2 FindCentroid(List<Vector2> points)
    {
        List< Vector2 > pts = new List<Vector2>();

        foreach (Vector2 point in points)
        {
            pts.Add(point);
        }

        pts.Add(pts[0]);

        int num_points = points.Count;
        

        // Find the centroid.
        float X = 0;
        float Y = 0;
        float second_factor;
        for (int i = 0; i < num_points; i++)
        {
            second_factor =
                pts[i].x * pts[i + 1].y -
                pts[i + 1].x * pts[i].y;
            X += (pts[i].x + pts[i + 1].x) * second_factor;
            Y += (pts[i].y + pts[i + 1].y) * second_factor;
        }

        // Divide by 6 times the polygon's area.
        float polygon_area = PolygonArea(ref points);
        X /= (6 * polygon_area);
        Y /= (6 * polygon_area);

        // If the values are negative, the polygon is
        // oriented counterclockwise so reverse the signs.
        if (X < 0)
        {
            X = -X;
            Y = -Y;
        }

        return new Vector2(X, Y);
    }

    public float PolygonArea(ref List<Vector2> points)
    {
        List<Vector2> pts = new List<Vector2>();

        foreach (Vector2 point in points)
        {
            pts.Add(point);
        }
        pts.Add(pts[0]);
        
        var area = Math.Abs(pts.Take(pts.Count - 1)
           .Select((p, i) => (pts[i + 1].x - p.x) * (pts[i + 1].y + p.y))
           .Sum() / 2);

        return area;
    }

    private Rect Bounds(List<Vector2> points)
    {
        float x = points[0].x;
        float y = points[0].y;

        float maxX = points[0].x;
        float maxY = points[0].y;

        float width;
        float height;

        foreach (Vector2 point in points)
        {
            if (point.x < x)
                x = point.x;

            if (point.y < y)
                y = point.y;

            if (point.x > maxX)
                maxX = point.x;

            if (point.y > maxY)
                maxY = point.y;
        }

        width = maxX - x;
        height = maxY - y;

        Rect rect = new Rect(x, y, width, height);

        return rect;
    }

    void DebugDrawPattern(List<Vector2> points, GameObject prefab)
    {
        foreach (Vector2 point in points)
        {
            GameObject go = (GameObject)Instantiate(prefab, point, Quaternion.identity);
            go.transform.SetParent(transform);
            _debugDrawnPoints.Add(go);
        }
    }

    List<Vector2> ScalePattern(List<Vector2> points, float coef)
    {
        List<Vector2> newList = new List<Vector2>();

        foreach (Vector2 point in points)
        {
            newList.Add(point*coef);
        }

        return newList;
    }

    List<Vector2> MovePattern(List<Vector2> points, float dx, float dy)
    {
        List<Vector2> newList = new List<Vector2>();

        foreach (Vector2 point in points)
        {
            newList.Add(point + new Vector2(dx, dy));
        }

        return newList;
    }

    List<Vector2> ScaledMovedPattern(List<Vector2> startPoints, float percent)
    {
        List<Vector2> scaledPattern = ScalePattern(startPoints, 1 + percent);

        Vector2 startCentr = FindCentroid(startPoints);
        Vector2 scaledCentr = FindCentroid(scaledPattern);

        float dx = startCentr.x - scaledCentr.x;
        float dy = startCentr.y - scaledCentr.y;

        List<Vector2> movedPattern = MovePattern(scaledPattern, dx, dy);

        return movedPattern;
    }

    private List<Vector2> CheckPointsAgainstPattern(List<Vector2> pattern, List<Vector2> pointsToCheck, bool checkInside)
    {
        List<Vector2> tPattern = new List<Vector2>();

        foreach (Vector2 point in pattern)
        {
            tPattern.Add(point);
        }
        tPattern.Add(pattern[0]);

        List<Vector2> curPointsTocheck = new List<Vector2>();
        foreach (Vector2 point in pointsToCheck)
        {
            curPointsTocheck.Add(point);
        }

        for (int i = 0; i < tPattern.Count - 1; i++)
        {
            List<Vector2> goodPoints = new List<Vector2>();

            foreach (Vector2 point in curPointsTocheck)
            {
                float val = (point.y - tPattern[i].y)*(tPattern[i + 1].x - tPattern[i].x) -
                            (point.x - tPattern[i].x)*(tPattern[i + 1].y - tPattern[i].y);

                if (checkInside)
                    if (val <= 0)
                    {
                        goodPoints.Add(point);
                    }
                if (!checkInside)
                {
                    if (val >= 0)
                    {
                        goodPoints.Add(point);
                    }
                }
            }

            curPointsTocheck = goodPoints;
        }

        return curPointsTocheck;
    }

    private bool CheckVertices(List<Vector2> pattern, List<Vector2> pointsToCheck)
    {
        int i = 0;

        foreach (var vert in pattern)
        {
            foreach (Vector2 point in pointsToCheck)
            {
                if (Vector2.Distance(vert, point) < _checkingDistance)
                {
                    ++i;
                    break;
                }
            }
        }

        return i == pattern.Count;
    }

    void ChosePattern()
    {
        
        int index = Random.Range(0, patterns.Count);

        if (_currentPattern == patterns[index])
        {
            while (_currentPattern == patterns[index])
            {
                index = Random.Range(0, patterns.Count);
            }
        }

        _currentPattern = patterns[index];

        eps = _currentPattern.eps;
        verticesAccuracy = _currentPattern.verticesAccuracy;
        successPercent = _currentPattern.successPercent;

        if (PatternChanged!=null)
        {
            PatternChanged(_currentPattern.sprite);
        }
    }

    void NextRound()
    {
        if (RoundDone!=null)
        {
            RoundDone();
        }
        ChosePattern();
    }

    void SendResultMessage(bool val)
    {
        if (DrawFinished!=null)
        {
            DrawFinished(val);
        }
    }
}
