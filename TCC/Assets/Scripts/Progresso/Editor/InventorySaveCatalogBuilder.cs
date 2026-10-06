using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Mantém os itens atuais e os próximos itens disponíveis no executável do jogo.
[InitializeOnLoad]
public class InventorySaveCatalogBuilder : AssetPostprocessor
{
    private const string CatalogPath = "Assets/Resources/InventorySaveCatalog.asset";
    private static bool queued;

    static InventorySaveCatalogBuilder() => QueueRefresh();

    private static void OnPostprocessAllAssets(string[] imported, string[] deleted,
        string[] moved, string[] movedFrom)
    {
        if (imported.Concat(deleted).Concat(moved).Concat(movedFrom)
            .Any(path => path != CatalogPath && path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)))
            QueueRefresh();
    }

    private static void QueueRefresh()
    {
        if (queued) return;
        queued = true;
        EditorApplication.delayCall += Refresh;
    }

    [MenuItem("Tools/Inventário/Atualizar catálogo de salvamento")]
    private static void Refresh()
    {
        queued = false;
        InventorySaveCatalog catalog = AssetDatabase.LoadAssetAtPath<InventorySaveCatalog>(CatalogPath);
        if (catalog == null) return;
        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets" });
        Array.Sort(guids, StringComparer.Ordinal);
        SerializedObject serialized = new SerializedObject(catalog);
        SerializedProperty entries = serialized.FindProperty("entries");
        bool same = entries.arraySize == guids.Length;
        if (same)
        {
            for (int i = 0; i < guids.Length; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (entry.FindPropertyRelative("id").stringValue != guids[i] ||
                    entry.FindPropertyRelative("item").objectReferenceValue != item)
                {
                    same = false;
                    break;
                }
            }
        }
        if (same) return;
        entries.arraySize = guids.Length;
        for (int i = 0; i < guids.Length; i++)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("id").stringValue = guids[i];
            entry.FindPropertyRelative("item").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guids[i]));
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssetIfDirty(catalog);
    }
}
