using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockableDoor : Interactable
{
    private bool locked = true;

    protected override void Start()
    {
        base.Start();

        locked = true;
    }

    public void UnlockDoor()
    {
        locked = false;
    }

    protected override void OnInteraction()
    {
        if (locked) { return; }

        Destroy(gameObject);
    }
}
