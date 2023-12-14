using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class CheckpointManager : SingletonPersistent<CheckpointManager>
{
    private Vector3 activeSpawnPoint = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
        activeSpawnPoint = FindObjectOfType<Player>().transform.position;
    }

    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        activeSpawnPoint = newSpawnPoint;
    }

    public Vector3 GetSpawnPoint()
    {
        return activeSpawnPoint;
    }
}
