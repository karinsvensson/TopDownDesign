using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LocalMaterialSelector)), CanEditMultipleObjects]
public class LocalMaterialSelectorEditor : Editor
{
    private bool changePossibleMaterials = false;

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Change Possible Materials"))
        {
            changePossibleMaterials = !changePossibleMaterials;
        }

        if (changePossibleMaterials)
        {
            base.OnInspectorGUI();
        }

        LocalMaterialSelector myTarget = (LocalMaterialSelector)target;

        if (myTarget.GetUsableMaterialsNames() == null)
        {
            GUILayout.Label("There are no usable materials for this object!");
            return;
        }

        EditorGUI.BeginChangeCheck();
        myTarget.currentMaterialIndex = EditorGUILayout.Popup(myTarget.currentMaterialIndex, myTarget.GetUsableMaterialsNames());
        if (EditorGUI.EndChangeCheck())
        {
            object[] myTargets = targets;
            for (int i = 0; i < myTargets.Length; i++)
            {
                LocalMaterialSelector target = (LocalMaterialSelector)myTargets[i];
                target.GetReferences();
                target.ApplyNewMaterial(myTarget.currentMaterialIndex);
            }
        }
    }
}
