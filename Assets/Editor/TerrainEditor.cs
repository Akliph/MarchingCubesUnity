using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(World))]
public class TerrainEditor : Editor
{
    

    public override void OnInspectorGUI()
    {
        World world = (World)target;

        if (DrawDefaultInspector())
            if (world.autoUpdate)
            {
                world.UpdateMap();
            }
                
    }
}
