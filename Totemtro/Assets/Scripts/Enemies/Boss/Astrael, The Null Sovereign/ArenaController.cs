using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArenaController : MonoBehaviour
{
    public GameObject arenaGuardianPrefab;

    [Header("Arena Settings")]
    public int guardianCount = 30;
    public float radius = 10f;
    public float minRadius = 5f;
    public float shrinkSpeed = 0.2f;
    public float rotationSpeed = 10f;

    [Header("Shape Timing")]
    public float shapeChangeInterval = 10f;
    public float enragedShapeInterval = 5f;

    [Header("Projectile Settings")]
    public float projectileSpeed = 12f;
    public float projectileChance = 0.1f;

    enum ArenaShape
    {
        Hexagon,
        Circle,
        Triangle,
        Square
    }

    ArenaShape currentShape = ArenaShape.Hexagon;

    List<ArenaGuardian> guardians = new List<ArenaGuardian>();
    bool enraged = false;

    Transform boss;

    public void Initialize(Transform bossTransform)
    {
        boss = bossTransform;
    }

    void Start()
    {
        SpawnGuardians();
        StartCoroutine(ShapeLoop());
    }

    void Update()
    {
        RotateArena();
        ShrinkArena();

        if (boss == null)
        {
            Destroy(gameObject);
            return;
        }

    }

    void SpawnGuardians()
    {
        for (int i = 0; i < guardianCount; i++)
        {
            GameObject g = Instantiate(
                arenaGuardianPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );

            ArenaGuardian ag = g.GetComponent<ArenaGuardian>();
            guardians.Add(ag);
        }
        Debug.Log("Spawning arena guardians: " + guardianCount);

        ArrangeShape(currentShape);
    }

    void RotateArena()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.unscaledDeltaTime);
    }

    void ShrinkArena()
    {
        if (radius > minRadius)
            radius -= shrinkSpeed * Time.unscaledDeltaTime;
    }

    IEnumerator ShapeLoop()
    {
        while (true)
        {
            float interval =
                enraged ? enragedShapeInterval : shapeChangeInterval;

            yield return new WaitForSeconds(interval);

            currentShape =
                (ArenaShape)(((int)currentShape + 1) % 4);

            ArrangeShape(currentShape);
        }
    }

    void ArrangeShape(ArenaShape shape)
    {
        for (int i = guardians.Count - 1; i >= 0; i--)
        {
            if (guardians[i] == null)
            {
                guardians.RemoveAt(i);
                continue;
            }

            Vector2 target = GetPositionOnShape(shape, i);
            guardians[i].MoveTo(target);
        }
    }


    Vector2 GetPositionOnShape(ArenaShape shape, int index)
    {
        float angle =
            (float)index / guardianCount * Mathf.PI * 2f;

        switch (shape)
        {
            case ArenaShape.Circle:
                return transform.position +
                       new Vector3(
                           Mathf.Cos(angle),
                           Mathf.Sin(angle)
                       ) * radius;

            case ArenaShape.Hexagon:
                return GetPolygonPoint(6, index);

            case ArenaShape.Triangle:
                return GetPolygonPoint(3, index);

            case ArenaShape.Square:
                return GetPolygonPoint(4, index);
        }

        return transform.position;
    }

    Vector2 GetPolygonPoint(int sides, int index)
    {
        float step = guardianCount / (float)sides;
        int side = Mathf.FloorToInt(index / step);

        float sideProgress =
            (index % step) / step;

        float angle1 =
            side * Mathf.PI * 2f / sides;

        float angle2 =
            (side + 1) * Mathf.PI * 2f / sides;

        Vector2 p1 =
            new Vector2(
                Mathf.Cos(angle1),
                Mathf.Sin(angle1)
            ) * radius;

        Vector2 p2 =
            new Vector2(
                Mathf.Cos(angle2),
                Mathf.Sin(angle2)
            ) * radius;

        return transform.position +
               Vector3.Lerp(p1, p2, sideProgress);
    }

    public void SetEnraged(bool state)
    {
        enraged = state;
    }

    public void StartCollapse()
    {
        StartCoroutine(CollapseRoutine());
    }

    IEnumerator CollapseRoutine()
    {
        float collapseSpeed = 5f;

        while (radius > 0.5f)
        {
            radius -= collapseSpeed * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public void RemoveGuardian(ArenaGuardian guardian)
    {
        guardians.Remove(guardian);
    }
}
