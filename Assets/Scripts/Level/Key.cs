using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Interactable
{
    [SerializeField] UnlockableDoor connectedDoor;

    protected override void Start()
    {
        base.Start();

        if (connectedDoor == null)
        {
            Debug.LogError("The variable connectedDoor has not been assigned! - " + transform.name);
        }
    }

    protected override void OnInteraction()
    {
        connectedDoor.UnlockDoor();
        Destroy(gameObject);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (connectedDoor == null) { return; }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, connectedDoor.transform.position);
    }
}
