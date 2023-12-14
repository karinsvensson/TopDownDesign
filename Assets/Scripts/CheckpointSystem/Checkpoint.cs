using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Vector3 spawnPoint = Vector3.zero;
    [Space]
    [SerializeField] float activationAreaRadius = 2f;
    [SerializeField] Vector3 activationAreaOffset = Vector3.zero;

    [SerializeField] LayerMask activationLayers;

    private void Start()
    {
        if (CheckpointManager.Instance == null)
        {
            new GameObject("CheckpointManager").AddComponent<CheckpointManager>();
        }

        if (spawnPoint == Vector3.zero)
        {
            Debug.LogWarning("No spawn point set for " + transform.name);
            spawnPoint = transform.position;
        }

        StartCoroutine(ActivationCheckRoutine());
    }

    public Vector3 GetSpawnPoint()
    {
        return transform.position + spawnPoint;
    }

    private IEnumerator ActivationCheckRoutine()
    {
        while (true)
        {
            if (Physics.OverlapSphere(transform.position + activationAreaOffset, activationAreaRadius, activationLayers).Length != 0)
            {
                CheckpointManager.Instance.SetSpawnPoint(transform.position + spawnPoint);
                Debug.Log("Checkpoint Activated");
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position + activationAreaOffset, activationAreaRadius);

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position + spawnPoint, 0.5f);
    }
}
