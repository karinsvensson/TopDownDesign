using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] protected float interactionCheckInterval = 0.5f;

    [SerializeField] protected Vector3 interactionAreaOffset = Vector3.zero;
    [SerializeField] protected Vector3 interactionAreaSize = new Vector3(1, 1, 1);

    protected virtual void Start()
    {
        StartCoroutine(CheckForPlayerRoutine());
    }

    protected virtual void OnInteraction()
    {

    }

    private IEnumerator CheckForPlayerRoutine()
    {
        while (true)
        {
            Collider[] hitColliders = Physics.OverlapBox(transform.position + interactionAreaOffset, interactionAreaSize / 2, Quaternion.identity);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].tag == "Player")
                {
                    OnInteraction();
                }
            }

            yield return new WaitForSeconds(interactionCheckInterval);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position + interactionAreaOffset, interactionAreaSize);
    }
}
