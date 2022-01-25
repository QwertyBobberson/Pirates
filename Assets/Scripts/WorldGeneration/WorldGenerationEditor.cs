using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldGeneration))]
public class WorldGenerationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WorldGeneration worldGen = (WorldGeneration)target;

        if(WorldGeneration.singleton == null)
        {
            WorldGeneration.singleton = worldGen;
        }

        if(GUILayout.Button("Randomize Seed"))
        {
            worldGen.seed = Random.Range(int.MinValue, int.MaxValue);
            Random.InitState(worldGen.seed);
            worldGen.offset.x = Random.Range(-100000, 100000);
            worldGen.offset.y = Random.Range(-100000, 100000);
        }
    
        EditorGUILayout.BeginHorizontal();

        
        if(GUILayout.Button("Regenerate Test World"))
        {
            worldGen.InitialLoad();
        }

        if(GUILayout.Button("Destroy Test World"))
        {
            worldGen.DestroyWorld();
        }

        EditorGUILayout.EndHorizontal();

    }
}