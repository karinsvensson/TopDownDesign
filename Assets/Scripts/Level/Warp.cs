using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warp : Interactable
{
    [Header("Warp")]
    [SerializeField] Warp receiver;
    [SerializeField] Vector3 warpPosition;

    public Vector3 GetWarpPosition()
    {
        return transform.position + warpPosition;
    }

    protected override void OnInteraction()
    {
        if (receiver != null)
        {
            Transform playerTransform = FindObjectOfType<Player>().transform;
            playerTransform.GetComponent<CharacterController>().enabled = false;
            playerTransform.position = receiver.GetWarpPosition();
            playerTransform.GetComponent<CharacterController>().enabled = true;
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position + warpPosition, 0.5f);
    }
}
