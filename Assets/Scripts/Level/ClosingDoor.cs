using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosingDoor : Interactable
{
    MeshRenderer meshRenderer;
    BoxCollider boxCollider;

    protected override void Start()
    {
        base.Start();

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;

        boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = false;
    }

    protected override void OnInteraction()
    {
        meshRenderer.enabled = true;
        boxCollider.enabled = true;
    }
}
