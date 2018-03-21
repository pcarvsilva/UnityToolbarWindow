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
        if (selectedMethod == null) return;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        if (Event.current != null && Event.current.type == EventType.MouseDown)
        {
            if (Event.current.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit);
                List<object> parameters = new List<object>();
                parameters.Add(hit);
                selectedMethod.Invoke(selectedAgent, parameters.ToArray());
            }
            else
            {
                ToolbarWindow.selectedMethod = null;
                ToolbarWindow.selectedAgent = null;
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
        List<MethodInfo> methods =
            editor.target.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m =>
                        m.GetCustomAttributes(typeof(ToolbarAttribute), false).Length == 1 &&
                        m.GetParameters().Length == 1 &&
                        !m.ContainsGenericParameters
                ).ToList();

        if (methods.Count == 0) return;
        EditorGUILayout.InspectorTitlebar(false, editor.target);
        EditorGUILayout.BeginHorizontal();
        foreach (MethodInfo method in methods)
        {        
            if (EditorGUILayout.ToggleLeft(ObjectNames.NicifyVariableName(method.Name), method == ToolbarWindow.selectedMethod))
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
