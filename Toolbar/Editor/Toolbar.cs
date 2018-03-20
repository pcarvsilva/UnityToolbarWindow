using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

public class ToolbarWindow : EditorWindow
{

    GameObject selected;
    List<Component> cs;
    public static MethodInfo selectedMethod;
    public static Component selectedAgent;

    [MenuItem("Window/Toolbar")]
    static void Init()
    {
        ToolbarWindow window = (ToolbarWindow)EditorWindow.GetWindow(typeof(ToolbarWindow));
        window.cs = new List<Component>();
        window.Show();
    }

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += SceneGUI;
    }

    void SceneGUI(SceneView sceneView)
    {
        if (Event.current != null && Event.current.type == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            if (selectedMethod != null)
            {
                List<object> parameters = new List<object>();
                parameters.Add(hit);
                selectedMethod.Invoke(selectedAgent,parameters.ToArray());
            }
        }
    }

    void OnGUI()
    {
        foreach (Component target in cs)
        {
            Editor e = Editor.CreateEditor(target);
            if (e != null)
                e.OnToolbarWindowGUI();
        }
    }

    void OnSelectionChange()
    {
        if (Selection.activeGameObject != selected && Selection.activeGameObject != null && Selection.activeGameObject.GetComponents<MonoBehaviour>() != null)
        {
            selected = Selection.activeGameObject;
            cs = selected.GetComponents(typeof(MonoBehaviour)).ToList();
            selectedMethod = null;
            selectedAgent = null;
            Repaint();
        }
    }
    
}


internal static class ToolbarWindowExtensionMethods
{
    public static void DrawDefaultToolbarWindowGUI(this Editor editor)
    {

        EditorGUILayout.InspectorTitlebar(false, editor.target);
        List<MethodInfo> methods =
            editor.target.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m =>
                        m.GetCustomAttributes(typeof(ToolbarAttribute), false).Length == 1 &&
                        m.GetParameters().Length == 1 &&
                        !m.ContainsGenericParameters
                ).ToList();

        EditorGUILayout.BeginHorizontal();
        foreach (MethodInfo method in methods)
        {
            string buttonText = ObjectNames.NicifyVariableName(method.Name);

            if (EditorGUILayout.ToggleLeft(buttonText, method == ToolbarWindow.selectedMethod))
            {
                ToolbarWindow.selectedMethod = method;
                ToolbarWindow.selectedAgent = editor.target as Component;
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void OnToolbarWindowGUI(this Editor editor)
    {
        foreach (MethodInfo myArrayMethodInfo in editor.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (myArrayMethodInfo.Name == "OnToolbarGUI")
            {
                myArrayMethodInfo.Invoke(editor, null);
                return;
            }
        }
        editor.DrawDefaultToolbarWindowGUI();
    }
}
