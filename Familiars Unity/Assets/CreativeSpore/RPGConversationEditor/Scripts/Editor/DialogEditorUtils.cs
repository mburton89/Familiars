// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;

using Object = UnityEngine.Object;

namespace CreativeSpore.RPGConversationEditor
{
    public static class DialogEditorUtils
    {
        private static Vector2 s_contactPoint;

        public static bool LineRectIntersection(Vector2 start, Vector2 end, Rect rect, out Vector2 intersection)
        {
            if (LineIntersects(start, end, rect.min, new Vector2(rect.xMax, rect.yMin), out intersection))
                return true;
            else if (LineIntersects(start, end, rect.min, new Vector2(rect.xMin, rect.yMax), out intersection))
                return true;
            else if (LineIntersects(start, end, rect.max, new Vector2(rect.xMax, rect.yMin), out intersection))
                return true;
            else if (LineIntersects(start, end, rect.max, new Vector2(rect.xMin, rect.yMax), out intersection))
                return true;
            return false;
        }

        // a1 is line1 start, a2 is line1 end, b1 is line2 start, b2 is line2 end
        // ref: https://stackoverflow.com/questions/3746274/line-intersection-with-aabb-rectangle
        public static bool LineIntersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            Vector2 b = a2 - a1;
            Vector2 d = b2 - b1;
            float bDotDPerp = b.x * d.y - b.y * d.x;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (bDotDPerp == 0)
                return false;

            Vector2 c = b1 - a1;
            float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
            if (t < 0 || t > 1)
                return false;

            float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
            if (u < 0 || u > 1)
                return false;

            intersection = a1 + t * b;

            return true;
        }

        //ref:http://wiki.unity3d.com/index.php/Distance_from_a_point_to_a_rectangle
        //Modifyied to also return the contact point
        public static float DistancePointToRectangle(Vector2 point, Rect rect, out Vector2 contactPoint)
        {
            float dist = DistancePointToRectangle(point, rect);
            contactPoint = s_contactPoint;
            return dist;
        }
        public static float DistancePointToRectangle(Vector2 point, Rect rect)
        {
            //  Calculate a distance between a point and a rectangle.
            //  The area around/in the rectangle is defined in terms of
            //  several regions:
            //
            //  O--x
            //  |
            //  y
            //
            //
            //        I   |    II    |  III
            //      ======+==========+======   --yMin
            //       VIII |  IX (in) |  IV
            //      ======+==========+======   --yMax
            //       VII  |    VI    |   V
            //
            //
            //  Note that the +y direction is down because of Unity's GUI coordinates.

            if (point.x < rect.xMin)
            { // Region I, VIII, or VII
                if (point.y < rect.yMin)
                { // I
                    s_contactPoint = new Vector2(rect.xMin, rect.yMin);
                    return (s_contactPoint - point).magnitude;
                }
                else if (point.y > rect.yMax)
                { // VII
                    s_contactPoint = new Vector2(rect.xMin, rect.yMax);
                    return (s_contactPoint - point).magnitude;
                }
                else
                { // VIII
                    s_contactPoint = new Vector2(rect.xMin, point.y);
                    return rect.xMin - point.x;
                }
            }
            else if (point.x > rect.xMax)
            { // Region III, IV, or V
                if (point.y < rect.yMin)
                { // III
                    s_contactPoint = new Vector2(rect.xMax, rect.yMin);
                    return (s_contactPoint - point).magnitude;
                }
                else if (point.y > rect.yMax)
                { // V
                    s_contactPoint = new Vector2(rect.xMax, rect.yMax);
                    return (s_contactPoint - point).magnitude;
                }
                else
                { // IV
                    s_contactPoint = new Vector2(rect.xMax, point.y);
                    return point.x - rect.xMax;
                }
            }
            else
            { // Region II, IX, or VI
                if (point.y < rect.yMin)
                { // II
                    s_contactPoint = new Vector2(point.x, rect.yMin);
                    return rect.yMin - point.y;
                }
                else if (point.y > rect.yMax)
                { // VI
                    s_contactPoint = new Vector2(point.x, rect.yMax);
                    return point.y - rect.yMax;
                }
                else
                { // IX
                    s_contactPoint = rect.center;
                    return 0f;
                }
            }
        }

        public static void HandlesDrawArrowEnd(Vector3 position, float size, float width, Vector3 direction, Color color)
        {
            Color savedColor = Handles.color;
            Handles.color = color;            
            float arrowAngle = direction.y >= 0 ? Vector2.Angle(Vector2.right, direction) : -Vector2.Angle(Vector2.right, direction);
            Quaternion rot = Quaternion.Euler(0f, 0f, arrowAngle);
            Handles.DrawAAConvexPolygon(position, position + rot * new Vector2(-size, width / 2f), position + rot * new Vector2(-size, -width / 2f));
            Handles.color = savedColor;
        }

        public static string ScrollableTextAreaInternal(Rect position, string text, ref Vector2 scrollPosition, GUIStyle style)
        {
            MethodInfo methodInfo = typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Rect), typeof(string), typeof(Vector2).MakeByRefType(), typeof(GUIStyle) }, null);

            if (methodInfo != null)
            {
                object[] parameters = new object[] { position, text, scrollPosition, style };
                string s = methodInfo.Invoke(null, parameters) as string;
                if (Event.current.type == EventType.Used && Event.current.clickCount < 2)
                {
                    SetSelectAllOnMouseUp(false); //fix issue in SceneView when EditorGUI text labels always select all text                    
                }                
                scrollPosition = (Vector2)parameters[2];
                return s;
            }
            return "";
        }

        public static bool GetSelectAllOnMouseUp()
        {
            var prop = typeof(EditorGUI).GetField("s_SelectAllOnMouseUp", BindingFlags.Static | BindingFlags.NonPublic);
            return (bool)prop.GetValue(null);
        }

        public static void SetSelectAllOnMouseUp(bool value)
        {
            var prop = typeof(EditorGUI).GetField("s_SelectAllOnMouseUp", BindingFlags.Static | BindingFlags.NonPublic);
            prop.SetValue(null, value);
        }

        public static TextEditor GetEditorGUIRecycledTextEditor()
        {
            return typeof(EditorGUI).GetField("s_RecycledEditor", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as TextEditor;
        }        

        public static GameObject MenuOptions_AddText(MenuCommand menuCommand)
        {
            var ass = Assembly.GetAssembly(typeof(UnityEditor.UI.TextEditor));
            var type = ass.GetType("UnityEditor.UI.MenuOptions");
            var prop = type.GetMethod("AddText", BindingFlags.Static | BindingFlags.Public);
            prop.Invoke(null, new object[] { menuCommand });
            return Selection.activeGameObject;
        }

        public static float HandleScaleWithScreenSize( CanvasScaler canvasScaler, Vector2 vector)
        {
            /*
            Vector2 vector = new Vector2((float)Screen.width, (float)Screen.height);
            if (Screen.fullScreen && this.m_Canvas.targetDisplay < Display.displays.Length)
            {
                Display display = Display.displays[this.m_Canvas.targetDisplay];
                vector = new Vector2((float)display.renderingWidth, (float)display.renderingHeight);
            }*/
            float scaleFactor = 0f;
            switch (canvasScaler.screenMatchMode)
            {
                case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                    {
                        float a = Mathf.Log(vector.x / canvasScaler.referenceResolution.x, 2f);
                        float b = Mathf.Log(vector.y / canvasScaler.referenceResolution.y, 2f);
                        float p = Mathf.Lerp(a, b, canvasScaler.matchWidthOrHeight);
                        scaleFactor = Mathf.Pow(2f, p);
                        break;
                    }
                case CanvasScaler.ScreenMatchMode.Expand:
                    scaleFactor = Mathf.Min(vector.x / canvasScaler.referenceResolution.x, vector.y / canvasScaler.referenceResolution.y);
                    break;
                case CanvasScaler.ScreenMatchMode.Shrink:
                    scaleFactor = Mathf.Max(vector.x / canvasScaler.referenceResolution.x, vector.y / canvasScaler.referenceResolution.y);
                    break;
            }
            return scaleFactor;
        }

        public static T CreateAssetInSelectedDirectory<T>(string name = null) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            if (string.IsNullOrEmpty(name))
                name = "New " + typeof(T).Name + ".asset";
            ProjectWindowUtil.CreateAsset(asset, name);
            return asset;
        }

        public static T CreateAssetWithSaveFilePanel<T>() where T : ScriptableObject
        {
            string path = EditorUtility.SaveFilePanelInProject("Save "+ typeof(T).Name + " file...", "New " + typeof(T).Name + ".asset", "asset", "", GetActiveFolderPath());
            if (!string.IsNullOrEmpty(path))
            {
                T asset = ScriptableObject.CreateInstance<T>();
                T asset2 = AssetDatabase.LoadMainAssetAtPath(path) as T;
                if (asset2)
                {
                    EditorUtility.CopySerialized(asset, asset2);
                    AssetDatabase.SaveAssets();
                    Object.DestroyImmediate(asset);
                    return asset2;
                }
                AssetDatabase.CreateAsset(asset, path);
                EditorUtility.SetDirty(asset); //Will make any change to asset to be saved when after closing the project
                return asset;
            }
            return null;
        }

        public static string GetActiveFolderPath()
        {
            var prop = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            return prop.Invoke(null, null) as string;            
        }

        public static Component CreateGameObjectWithComponent(System.Type componentType)
        {
            if (componentType.IsSubclassOf(typeof(Component)))
            {
                string componentName = componentType.Name;
                string name = GameObjectUtility.GetUniqueNameForSibling(Selection.activeTransform, componentName);
                GameObject go = new GameObject(name);
                Component comp = go.AddComponent(componentType);
                go.transform.SetParent(Selection.activeTransform);
                go.transform.localPosition = Vector3.zero;
                Selection.activeGameObject = go;
                Undo.RegisterCreatedObjectUndo((UnityEngine.Object)go, "Create " + componentName);
                return comp;
            }
            return null;
        }

        public static T CreateGameObjectWithComponent<T>() where T : Component
        {
            return CreateGameObjectWithComponent(typeof(T)) as T;
        }
    }

    /// <summary>
    /// This class was created using GUIScaleUtility as reference:
    /// https://gist.github.com/Seneral/2c8e7dfe712b9f53c60f80722fbce5bd#file-guiscaleutility-cs
    /// </summary>
    public static class GUIScaleUtils
    {
        private static List<List<Rect>> s_rectStackGroups = new List<List<Rect>>();
        private static List<Rect> s_currentRectStack = new List<Rect>();

        private static Func<Rect> s_getTopRectDelegate;

        public static Rect GetTopRect()
        {
            if (s_getTopRectDelegate == null)
            {
                Assembly UnityEngine = Assembly.GetAssembly(typeof(UnityEngine.GUI));
                Type GUIClipType = UnityEngine.GetType("UnityEngine.GUIClip", true);
                MethodInfo GetTopRect = GUIClipType.GetMethod("GetTopRect", BindingFlags.Static | BindingFlags.NonPublic);
                s_getTopRectDelegate = (Func<Rect>)Delegate.CreateDelegate(typeof(Func<Rect>), GetTopRect);
            }
            return (Rect)s_getTopRectDelegate.Invoke();
        }

        public static void BeginNoClip()
        {
            // Record and close all clips one by one, from bottom to top, until we hit the 'origin'
            List<Rect> rectStackGroup = new List<Rect>();
            Rect topMostClip = GetTopRect();
            while (topMostClip != new Rect(-10000, -10000, 40000, 40000))
            {
                rectStackGroup.Add(topMostClip);
                GUI.EndClip();
                topMostClip = GetTopRect();
            }
            // Store the clips appropriately
            rectStackGroup.Reverse();
            s_rectStackGroups.Add(rectStackGroup);
            s_currentRectStack.AddRange(rectStackGroup);
        }

        public static void MoveClipsUp(int count)
        {
            // Record and close all clips one by one, from bottom to top, until reached the count or hit the 'origin'
            List<Rect> rectStackGroup = new List<Rect>();
            Rect topMostClip = GetTopRect();
            while (topMostClip != new Rect(-10000, -10000, 40000, 40000) && count > 0)
            {
                rectStackGroup.Add(topMostClip);
                GUI.EndClip();
                topMostClip = GetTopRect();
                count--;
            }
            // Store the clips appropriately
            rectStackGroup.Reverse();
            s_rectStackGroups.Add(rectStackGroup);
            s_currentRectStack.AddRange(rectStackGroup);
        }

        public static void RestoreClips()
        {
            if (s_rectStackGroups.Count == 0)
            {
                Debug.LogError("GUIClipHierarchy: BeginNoClip/MoveClipsUp - RestoreClips count not balanced!");
                return;
            }

            // Read and restore clips one by one, from top to bottom
            List<Rect> rectStackGroup = s_rectStackGroups[s_rectStackGroups.Count - 1];
            for (int clipCnt = 0; clipCnt < rectStackGroup.Count; clipCnt++)
            {
                // NOTE: setting resetOffset in BegingClip to true in the last clip fix the rect tool handle position of the selected UI element.
                GUI.BeginClip(rectStackGroup[clipCnt], Vector2.zero, Vector2.zero, clipCnt == rectStackGroup.Count - 1);
                s_currentRectStack.RemoveAt(s_currentRectStack.Count - 1);
            }
            s_rectStackGroups.RemoveAt(s_rectStackGroups.Count - 1);
        }
    }
}
