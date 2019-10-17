﻿using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EnemySpawnSystem : ComponentSystem
{
    private EntityQuery spawnerQuery;
    private EntityQuery playerQuery;

    private readonly List<float> time = new List<float>();

    protected override void OnCreate()
    {
        spawnerQuery = GetEntityQuery(
            ComponentType.ReadOnly<EnemySpawner>());
        playerQuery = GetEntityQuery(
            ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<PlayerData>(),
            ComponentType.ReadOnly<HealthData>());
    }

    protected override void OnUpdate()
    {
        GameObject player = null;
        var playerHp = 0;

        Entities.With(playerQuery).ForEach(
            (Entity entity, Transform transform, ref HealthData hp) =>
            {
                player = transform.gameObject;
                playerHp = hp.Value;
            });

        if (player == null || playerHp <= 0)
            return;

        var dt = Time.deltaTime;

        var spawner = spawnerQuery.ToComponentArray<EnemySpawner>();
        for (var i = 0; i < spawner.Length; i++)
        {
            if (time.Count < i + 1)
                time.Add(0f);

            time[i] += dt;

            if (time[i] >= spawner[i].SpawnTime)
            {
                Object.Instantiate(spawner[i].Enemy, spawner[i].transform.position, quaternion.identity);
                time[i] = 0f;
            }
        }
    }
}
