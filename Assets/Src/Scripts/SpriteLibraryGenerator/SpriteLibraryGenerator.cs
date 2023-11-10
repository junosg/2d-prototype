using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.U2D.Animation;
using System.Linq;

[System.Serializable]
public class Action
{
    public string label;
    public string sourcePath;
    public SpriteLibraryCategory[] categories;
}

[System.Serializable]

public class SpriteLibraryCategory
{
    public string name;
    public LabelIndexPair[] labels;
}

[System.Serializable]
public class LabelIndexPair
{
    public string label;
    public int index;
}

public class SpriteLibraryGenerator : EditorWindow
{
    string savingDirectory = "Assets/";
    string filename = string.Empty;

    Vector2 scrollPosition = Vector2.zero;

    public Action[] actions;
    //public SpriteLibraryCategory[] categories;

    SerializedObject serializedObject;

    [MenuItem("Tools/SpriteLibraryGenerator")]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow<SpriteLibraryGenerator>();
        wnd.titleContent = new GUIContent("Sprite Library Generator");

        // Limit size of the window
        wnd.minSize = new Vector2(450, 200);
        wnd.maxSize = new Vector2(1920, 720);
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
    }

    Rect EditorArea(int uniformPadding)
    {
        RectOffset padding = new(uniformPadding, uniformPadding, uniformPadding, uniformPadding);
        Rect area = new(padding.right, padding.top, position.width - (padding.right + padding.left), position.height - (padding.top + padding.bottom));
        return area;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(EditorArea(10));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, true, true, GUILayout.Width(600), GUILayout.Height(800));

        GUILayout.Label("Saving Path");
        savingDirectory = GUILayout.TextField(savingDirectory);

        CreateListUI("actions");

        if (GUILayout.Button("Generate"))
        {
            CreateSpriteLibraryAsset(savingDirectory);
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void CreateListUI(string property)
    {
        serializedObject.Update();
        SerializedProperty serializedProperty = serializedObject.FindProperty(property);
        EditorGUILayout.PropertyField(serializedProperty, true);
        serializedObject.ApplyModifiedProperties();
    }

    List<Sprite> GetSprites(string path)
    {
        filename = AssetDatabase.LoadAssetAtPath<Texture2D>(path).name;
        List<Sprite> spriteList = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToList();
        
        return spriteList;
    }

    void CreateSpriteLibraryAsset(string savingDirectory)
    {
        SpriteLibraryAsset spriteLibraryAsset = CreateInstance<SpriteLibraryAsset>();

        foreach (Action action in actions)
        {
            List<Sprite> spriteList = GetSprites(action.sourcePath);

            foreach (SpriteLibraryCategory category in action.categories)
            {
                foreach (LabelIndexPair pair in category.labels)
                {
                    if (spriteList.Count == 0) return;

                    spriteLibraryAsset.AddCategoryLabel(spriteList[pair.index], category.name, pair.label);
                }
            }
        }

        AssetDatabase.CreateAsset(spriteLibraryAsset, savingDirectory + "/" + filename + ".asset");
    }
}
