using UnityEngine;
using System.Collections.Generic;

public class RoadManager : MonoBehaviour
{
    [Header("Сегменты дороги")]
    [SerializeField] private GameObject roadSegmentPrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float segmentLength = 20f;

    [Header("Препятствия")]
    [SerializeField] private GameObject smallObstaclePrefab;
    [SerializeField] private GameObject bigObstaclePrefab;
    [SerializeField] private float obstacleChance = 0.3f;
    [SerializeField] private float safeDistance = 5f; // Безопасная дистанция без препятствий

    private List<GameObject> roadPool = new List<GameObject>();
    private float lastSpawnZ = 0f;
    private Transform player;
    private float nextSpawnThreshold = 0f;
    private float totalSpawnedDistance = 0f; // Считаем, сколько метров дороги уже создано
    private bool safeZonePassed = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject segment = Instantiate(roadSegmentPrefab, transform);
            segment.SetActive(false);
            roadPool.Add(segment);
        }

        // Спавним первые 3 сегмента без препятствий
        for (int i = 0; i < 3; i++)
        {
            SpawnSegment(true); // true = безопасный сегмент без препятствий
        }

        nextSpawnThreshold = segmentLength * 2;
    }

    void Update()
    {
        if (player == null) return;

        if (player.position.z > nextSpawnThreshold)
        {
            SpawnSegment();
            nextSpawnThreshold += segmentLength;
        }

        // Удаляем сегменты позади игрока
        foreach (var segment in roadPool)
        {
            if (segment.activeSelf && segment.transform.position.z < player.position.z - segmentLength * 2)
            {
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
        if (segment == null) return;

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
        // Массив для отслеживания занятых полос в этом сегменте
        bool[] lanesOccupied = new bool[3];
        bool hasBigObstacle = false;

        // Сначала проверяем, не пытаемся ли мы заблокировать все полосы
        for (int lane = 0; lane < 3; lane++)
        {
            if (Random.value < obstacleChance)
            {
                bool isBig = Random.value < 0.3f;

                // Если это большое препятствие
                if (isBig)
                {
                    // Проверяем, не будет ли оно блокировать единственный проход
                    if (hasBigObstacle)
                    {
                        // Уже есть большое препятствие в этом сегменте - пропускаем
                        continue;
                    }

                    // Проверяем, не займёт ли оно все полосы
                    int occupiedCount = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        if (lanesOccupied[i]) occupiedCount++;
                    }

                    // Если осталось только 1 свободная полоса - не ставим большое
                    if (occupiedCount >= 2)
                    {
                        continue;
                    }

                    hasBigObstacle = true;
                    lanesOccupied[lane] = true;
                    SpawnObstacle(lane, segment, true);
                }
                else
                {
                    // Маленькое препятствие можно ставить, если полоса свободна
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