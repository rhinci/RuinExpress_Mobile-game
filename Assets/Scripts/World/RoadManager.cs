using UnityEngine;
using System.Collections.Generic;

public class RoadManager : MonoBehaviour
{
    [Header("Сегменты дороги")]
    [SerializeField] private GameObject roadSegmentPrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float segmentLength = 20f;

    [Header("Препятствия")]
    [SerializeField] private GameObject smallObstaclePrefab;  // Пока куб 0.5 высотой
    [SerializeField] private GameObject bigObstaclePrefab;    // Пока куб 2 высотой
    [SerializeField] private float obstacleChance = 0.3f;     // 30% шанс спавна препятствия

    private List<GameObject> roadPool = new List<GameObject>();
    private float spawnZ = 0f;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Создаём пул сегментов
        for (int i = 0; i < poolSize; i++)
        {
            GameObject segment = Instantiate(roadSegmentPrefab, transform);
            segment.SetActive(false);
            roadPool.Add(segment);
        }

        // Спавним первые 3 сегмента
        for (int i = 0; i < 3; i++)
        {
            SpawnSegment();
        }
    }

    void Update()
    {
        // Если игрок прошёл больше половины текущего сегмента - спавним новый
        if (player != null && player.position.z > spawnZ - (segmentLength * poolSize / 2f))
        {
            SpawnSegment();
        }
    }

    void SpawnSegment()
    {
        // Берём неактивный сегмент из пула
        GameObject segment = roadPool.Find(s => !s.activeSelf);
        if (segment == null) return;

        // Позиционируем
        segment.transform.position = new Vector3(0, 0, spawnZ);
        segment.SetActive(true);

        // Генерируем препятствия на этом сегменте
        GenerateObstacles(segment);

        spawnZ += segmentLength;
    }

    void GenerateObstacles(GameObject segment)
    {
        // На каждом сегменте - 3 зоны для препятствий (по полосам)
        for (int lane = 0; lane < 3; lane++)
        {
            if (Random.value < obstacleChance)
            {
                // Решаем, какое препятствие ставить
                bool isBig = Random.value < 0.3f; // 30% больших препятствий
                GameObject obstaclePrefab = isBig ? bigObstaclePrefab : smallObstaclePrefab;

                // Создаём препятствие
                Vector3 position = GetPositionInLane(lane, segment);
                GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity, segment.transform);

                // Для больших препятствий - блокируем соседние полосы
                if (isBig)
                {
                    obstacle.transform.localScale = new Vector3(2.5f, 2f, 1f);
                }
            }
        }
    }

    Vector3 GetPositionInLane(int lane, GameObject segment)
    {
        float xPos = (lane - 1) * 1.5f; 
        float zOffset = Random.Range(2f, segmentLength - 2f);

        return new Vector3(xPos, 0.5f, segment.transform.position.z + zOffset);
    }
}