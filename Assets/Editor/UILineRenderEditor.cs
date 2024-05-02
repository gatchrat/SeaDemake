using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UILineRenderer))]
public class UILineRenderEditor : Editor {

    private bool addMode = false;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button(addMode ? "Stop adding points" : "Add points in SceneView")) {
            addMode = !addMode;
        }
        if (addMode) {
            if (!SceneViewInFocus()) {
                Event e = Event.current;
                //deactivate by empty click in inspector (qol)
                if (e.type == EventType.MouseDown && e.button == 0) {
                    addMode = false;
                }
            }
        }
    }

    // Check if the scene view is in focus
    private bool SceneViewInFocus() {
        if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType().ToString() == "UnityEditor.SceneView") {
            return true;
        }
        return false;
    }
    private void OnSceneGUI() {
        UILineRenderer script = (UILineRenderer)target;
        if (addMode && SceneViewInFocus()) {
            // Check for mouse click events
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0) // Left mouse button
            {
                // Prevent scene view from selecting objects
                e.Use();
                //get world position, which is also the position on the canvas
                Vector2 mousePosition = Event.current.mousePosition;
                Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                //Remedy offsets added by whatever object this is assigned to
                RectTransform thisRec = ((UILineRenderer)target).gameObject.GetComponent<RectTransform>();
                Vector3[] v = new Vector3[4];
                thisRec.GetWorldCorners(v);
                script.AddCoordinate(new Vector2(ray.origin.x - v[0].x, ray.origin.y - v[0].y));
            }
        }
    }

}
