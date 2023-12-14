using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalMaterialSelector : MonoBehaviour
{
    [HideInInspector] public int currentMaterialIndex;

    [SerializeField] Material[] usableMaterials = new Material[0];

    MeshRenderer meshRenderer;

    public void GetReferences()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public string[] GetUsableMaterialsNames()
    {
        if (usableMaterials.Length == 0) { return null; }

        string[] materialNames = new string[usableMaterials.Length];

        for (int i = 0; i < materialNames.Length; i++)
        {
            materialNames[i] = usableMaterials[i].name;
        }

        return materialNames;
    }

    public void ApplyNewMaterial(int materialIndex)
    {
        if (materialIndex < 0 || materialIndex > (usableMaterials.Length - 1))
        {
            Debug.LogWarning("Material-Index is out of range!");
            return;
        }

        if (meshRenderer.sharedMaterials.Length == 1)
        {
            meshRenderer.sharedMaterial = usableMaterials[materialIndex];
            return;
        }

        Material[] selectedMaterialArray = new Material[meshRenderer.sharedMaterials.Length];

        for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
        {
            selectedMaterialArray[i] = usableMaterials[materialIndex];
        }

        meshRenderer.sharedMaterials = selectedMaterialArray;
    }
}
