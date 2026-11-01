#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public static class ConfigurarInicioEstrada
{
    [MenuItem("Currais/Configurar inicio da Estrada")]
    public static void Configurar()
    {
        const string scenePath = "Assets/Scenes/Estrada.unity";
        const string prefabPath = "Assets/prefab/Canvas.prefab";

        Scene scene = EditorSceneManager.OpenScene(scenePath);
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player == null)
        {
            Debug.LogError("Estrada: Mari com PlayerMovement nao encontrada.");
            return;
        }

        GameObject ui = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindRecursive(root.transform, "DialogueUI");
            if (found != null)
            {
                ui = found.gameObject;
                break;
            }
        }

        if (ui == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("Nao foi encontrado o prefab Canvas em " + prefabPath);
                return;
            }

            GameObject canvas = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            canvas.name = "CanvasDialogoEstrada";

            // Este prefab tambem possui inventario e outras interfaces.
            // Mantemos apenas o modulo de dialogo para evitar duplicatas.
            foreach (Transform child in canvas.transform)
            {
                if (child.name != "DialogueUI")
                    child.gameObject.SetActive(false);
            }

            Transform dialogue = FindRecursive(canvas.transform, "DialogueUI");
            if (dialogue == null)
            {
                Debug.LogError("Prefab Canvas nao possui DialogueUI.");
                return;
            }
            ui = dialogue.gameObject;
        }

        Transform panel = FindRecursive(ui.transform, "DialoguePanel");
        Transform choices = FindRecursive(ui.transform, "ChoicesContainer");
        Transform name = FindRecursive(ui.transform, "CharacterName");
        Transform text = FindRecursive(ui.transform, "DialogueText");
        Transform portrait = FindRecursive(ui.transform, "Portrait");

        if (panel == null || name == null || text == null)
        {
            Debug.LogError("Canvas de dialogo incompleto: confira DialoguePanel, CharacterName e DialogueText.");
            return;
        }

        GameObject start = GameObject.Find("InicioJornada");
        if (start == null)
            start = new GameObject("InicioJornada");

        SceneManager.MoveGameObjectToScene(start, scene);
        DialogoInicialEstrada component = start.GetComponent<DialogoInicialEstrada>();
        if (component == null)
            component = start.AddComponent<DialogoInicialEstrada>();

        SerializedObject serialized = new SerializedObject(component);
        serialized.FindProperty("playerMovement").objectReferenceValue = player;
        serialized.FindProperty("dialogueUI").objectReferenceValue = ui;
        serialized.FindProperty("dialoguePanel").objectReferenceValue = panel.gameObject;
        serialized.FindProperty("choicesContainer").objectReferenceValue =
            choices != null ? choices.gameObject : null;
        serialized.FindProperty("characterNameText").objectReferenceValue =
            name.GetComponent<TMP_Text>();
        serialized.FindProperty("dialogueText").objectReferenceValue =
            text.GetComponent<TMP_Text>();
        serialized.FindProperty("portraitImage").objectReferenceValue =
            portrait != null ? portrait.GetComponent<Image>() : null;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Inicio da Estrada configurado e cena salva. Atribua os retratos opcionais no Inspector.");
    }

    private static Transform FindRecursive(Transform parent, string target)
    {
        if (parent.name == target)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindRecursive(child, target);
            if (result != null)
                return result;
        }

        return null;
    }
}
#endif
