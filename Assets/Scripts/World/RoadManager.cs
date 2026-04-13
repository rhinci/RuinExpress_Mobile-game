using UnityEngine;
using System.Collections.Generic;

public class RoadManager : MonoBehaviour
{
    [Header("Сегменты дороги")]
    [SerializeField] private GameObject roadSegmentPrefab;
    [SerializeField] private int poolSize = 15;
    [SerializeField] private float segmentLength = 20f;

    [Header("Препятствия")]
    [SerializeField] private GameObject smallObstaclePrefab;
    [SerializeField] private GameObject bigObstaclePrefab;
    [SerializeField] private float obstacleChance = 0.3f;
    [SerializeField] private float safeDistance = 50f;

    private List<GameObject> roadPool = new List<GameObject>();
    private float lastSpawnZ = 0f;
    private Transform player;
    private float totalSpawnedDistance = 0f;
    private bool safeZonePassed = false;

    [Header("Настройки генерации")]
    [SerializeField] private int segmentsAhead = 2; // Должно быть сгенерировано впереди

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject segment = Instantiate(roadSegmentPrefab, transform);
            segment.SetActive(false);
            roadPool.Add(segment);
        }

        // Спавним начальные сегменты (без препятствий)
        for (int i = 0; i < segmentsAhead + 1; i++)
        {
            SpawnSegment(true);
        }
    }

    void Update()
    {
        if (player == null) return;

        // Считаем, сколько активных сегментов впереди игрока
        int activeSegmentsAhead = 0;
        foreach (var segment in roadPool)
        {
            if (segment.activeSelf && segment.transform.position.z > player.position.z)
            {
                activeSegmentsAhead++;
            }
        }

        // Если впереди меньше чем segmentsAhead, спавним новый
        if (activeSegmentsAhead < segmentsAhead)
        {
            SpawnSegment();
        }

        // Удаляем сегменты позади игрока
        foreach (var segment in roadPool)
        {
            if (segment.activeSelf && segment.transform.position.z < player.position.z - segmentLength)
            {
                // Удаляем препятствия
                foreach (Transform child in segment.transform)
                {
                    if (child.CompareTag("Obstacle"))
                        Destroy(child.gameObject);
                }
                segment.SetActive(false);
            }
        }
    }

    void SpawnSegment(bool isSafe = false)
    {
        GameObject segment = roadPool.Find(s => !s.activeSelf);
        if (segment == null)
        {
            // Если пул закончился - расширяем
            Debug.LogWarning("Пул сегментов закончился, расширяем...");
            GameObject newSegment = Instantiate(roadSegmentPrefab, transform);
            newSegment.SetActive(false);
            roadPool.Add(newSegment);
            segment = newSegment;
        }

        segment.transform.position = new Vector3(0, 0, lastSpawnZ);
        segment.SetActive(true);

        // Проверяем безопасную зону
        bool shouldBeSafe = !safeZonePassed && (totalSpawnedDistance < safeDistance);

        if (!shouldBeSafe && !isSafe)
        {
            GenerateObstacles(segment);
        }

        lastSpawnZ += segmentLength;
        totalSpawnedDistance += segmentLength;

        if (totalSpawnedDistance >= safeDistance)
        {
            safeZonePassed = true;
        }
    }

    void GenerateObstacles(GameObject segment)
    {
        bool[] lanesOccupied = new bool[3];
        bool hasBigObstacle = false;

        for (int lane = 0; lane < 3; lane++)
        {
            if (Random.value < obstacleChance)
            {
                bool isBig = Random.value < 0.3f;

                if (isBig)
                {
                    if (hasBigObstacle) continue;

                    int occupiedCount = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        if (lanesOccupied[i]) occupiedCount++;
                    }

                    if (occupiedCount >= 2) continue;

                    hasBigObstacle = true;
                    lanesOccupied[lane] = true;
                    SpawnObstacle(lane, segment, true);
                }
                else
                {
                    if (!lanesOccupied[lane])
                    {
                        lanesOccupied[lane] = true;
                        SpawnObstacle(lane, segment, false);
                    }
                }
            }
        }
    }

    void SpawnObstacle(int lane, GameObject segment, bool isBig)
    {
        GameObject obstaclePrefab = isBig ? bigObstaclePrefab : smallObstaclePrefab;
        Vector3 position = GetPositionInLane(lane, segment);
        GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity, segment.transform);
        obstacle.tag = "Obstacle";

        if (isBig)
        {
            obstacle.transform.localScale = new Vector3(1.5f, 2f, 1f);
        }
    }

    Vector3 GetPositionInLane(int lane, GameObject segment)
    {
        float xPos = (lane - 1) * 1.5f;
        float zOffset = Random.Range(2f, segmentLength - 2f);
        return new Vector3(xPos, 0.5f, segment.transform.position.z + zOffset);
    }
}