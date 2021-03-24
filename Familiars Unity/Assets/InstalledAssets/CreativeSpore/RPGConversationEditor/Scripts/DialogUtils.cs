// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.EventSystems;

namespace CreativeSpore.RPGConversationEditor
{
    public static class DialogColorUtils
    {
        public static Color GetColorWithHigherContrast(Color background, params Color[] colors)
        {
            Color ret = background;
            float lastContrastRatio = 0f;
            for(int i = 0; i < colors.Length; ++i)
            {
                float contrastRatio = ContrastRatio(background, colors[i]);
                if(contrastRatio > lastContrastRatio)
                {
                    lastContrastRatio = contrastRatio;
                    ret = colors[i];
                }
            }
            return ret;
        }

        //ref: https://www.w3.org/TR/2008/REC-WCAG20-20081211/#contrast-ratiodef
        // Contrast ratios can range from 1 to 21
        public static float ContrastRatio(Color c1, Color c2)
        {
            float l1 = ColorLuminance(c1);
            float l2 = ColorLuminance(c2);
            if (l1 > l2)
                return (l1 + 0.05f) / (l2 + 0.05f);
            else
                return (l2 + 0.05f) / (l1 + 0.05f);
        }

        public static float ColorLuminance(Color color)
        {
            float r = color.r <= 0.03928f ? color.r / 12.92f : Mathf.Pow((color.r + 0.055f) / 1.055f, 2.4f);
            float g = color.g <= 0.03928f ? color.g / 12.92f : Mathf.Pow((color.g + 0.055f) / 1.055f, 2.4f);
            float b = color.b <= 0.03928f ? color.b / 12.92f : Mathf.Pow((color.b + 0.055f) / 1.055f, 2.4f);
            return 0.2126f * r + 0.7152f * g + 0.0722f * b;
        }
    }

    public static class DialogMathUtils
    {
        public static Vector3 CalculateBezierTangent(float t, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent)
        {
            //dP(t) / dt = -3(1 - t) ^ 2 * P0 + 3(1 - t) ^ 2 * P1 - 6t(1 - t) * P1 - 3t ^ 2 * P2 + 6t(1 - t) * P2 + 3t ^ 2 * P3
            Vector3 p0 = startPosition;
            Vector3 p1 = startTangent;
            Vector3 p2 = endTangent;
            Vector3 p3 = endPosition;
            return -3f * Mathf.Pow((1f - t), 2f) * p0 + 3f*Mathf.Pow((1f - t), 2f) * p1 - 6f*t*(1f - t) * p1 - 3f*Mathf.Pow(t, 2) * p2 + 6f*t*(1f - t) * p2 + 3f*Mathf.Pow(t, 2f) * p3;
        }
    }

    public static class ReflectionUtils
    {
        public static void AutoFillComponentFields(Component source)
        {
            //Debug.Log("AutoFillComponentFields " + source);
            var fields = source.GetType().GetFields();
            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                if (typeof(Component).IsAssignableFrom(field.FieldType))
                {
                    Component fieldValue = field.GetValue(source) as Component;
                    if (field.GetValue(source) == null || !fieldValue)
                    {
                        var foundObject = source.GetComponentsInChildren(fieldType).FirstOrDefault(o => (o as Component).name.Equals(field.Name, System.StringComparison.InvariantCultureIgnoreCase));
                        if (foundObject != null)
                        {
                            Debug.Log("Found component with the same name of the field " + field.Name + " in gameObject " + (foundObject as Component).gameObject.transform.root.name, foundObject);
                            field.SetValue(source, foundObject);
                        }
                    }
                }
                else if (typeof(GameObject).IsAssignableFrom(field.FieldType))
                {
                    GameObject fieldValue = field.GetValue(source) as GameObject;
                    if (field.GetValue(source) == null || !fieldValue)
                    {
                        var foundObject = FindChildIgnoreCase(source.transform, field.Name);
                        if (foundObject != null)
                        {
                            Debug.Log("Found gameobject with the same name of the field " + field.Name + " in gameObject " + foundObject.gameObject.transform.root.name, foundObject.gameObject);
                            field.SetValue(source, foundObject.gameObject);
                        }
                    }
                }
            }
        }

        public static Transform FindChildIgnoreCase(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; ++i)
            {
                Transform child = parent.transform.GetChild(i);
                if (child.name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    return child;
                }
                else if( child.childCount > 0 )
                {
                    Transform child2 = FindChildIgnoreCase(child, name);
                    if (child2)
                        return child2;
                }
            }
            return null;
        }
    }

    public static class UIUtils
    {
        private static List<GameObject> s_gameObjectList = new List<GameObject>();

        //Note: this method is based on ReorderableList.DoAddButton in UnityEditor.dll
        public static void DoAddItemToList(IList list)
        {
            Type elementType = list.GetType().GetElementType();
            if (elementType == typeof(string))
            {
                list.Add(string.Empty);
            }
            else if (elementType != null && elementType.GetConstructor(Type.EmptyTypes) == null)
            {
                Debug.LogError("Cannot add element. Type " + elementType.ToString() + " has no default constructor. Implement a default constructor or implement your own add behaviour.");
            }
            else if (list.GetType().GetGenericArguments()[0] != null)
            {
                list.Add(Activator.CreateInstance(list.GetType().GetGenericArguments()[0]));
            }
            else if (elementType != null)
            {
                list.Add(Activator.CreateInstance(elementType));
            }
            else
            {
                Debug.LogError("Cannot add element of type Null.");
            }
        }


        /// <summary>
        /// This generic code, removes or adds items as children of a grid object depending of itemCounter value. The object template used to create new objects is the first child item.
        /// So one item is always needed.
        /// </summary>
        /// <param name="gridObj"></param>
        /// <param name="itemCounter"></param>
        public static void ResizeGridItems(GameObject gridObj, int itemCounter, bool disableInactive = true)
        {
            //if (gridObj.transform.childCount != itemCounter)
            {
                s_gameObjectList.Clear();
                for (int i = 0; i < gridObj.transform.childCount; ++i)
                {
                    GameObject childItem = gridObj.transform.GetChild(i).gameObject;
                    childItem.SetActive(true);
                    s_gameObjectList.Add(childItem);
                }
                if (s_gameObjectList.Count == 0)
                {
                    Debug.LogError(" List should have at least an element! ");
                }
                else
                {
                    s_gameObjectList[0].SetActive(true); // in case it was deactivated ( see while below )
                    //Deactivate / Destroy inactive items
                    while (s_gameObjectList.Count > itemCounter)
                    {
                        // hide the first item, we need at least one in the least to be user as prefab for creation of items when itemCounter > 0
                        if (s_gameObjectList.Count == 1)
                        {
                            s_gameObjectList[0].SetActive(false);
                            break;
                        }

                        if (disableInactive)
                        {
                            s_gameObjectList[s_gameObjectList.Count - 1].SetActive(false);
                        }
                        else
                        {
                            if (Application.isPlaying)
                                GameObject.Destroy(s_gameObjectList[s_gameObjectList.Count - 1]);
                            else
                                GameObject.DestroyImmediate(s_gameObjectList[s_gameObjectList.Count - 1]);
                        }
                        s_gameObjectList.RemoveAt(s_gameObjectList.Count - 1);
                    }
                    //Create needed gameObjects
                    while (s_gameObjectList.Count < itemCounter)
                    {
                        GameObject obj = GameObject.Instantiate(s_gameObjectList[0]);
                        obj.transform.SetParent(gridObj.transform, false);
                        s_gameObjectList.Add(obj);
                    }                    
                }
            }
        }

        public static void InitializeEventSystem()
        {
            EventSystem eventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject gameObject = new GameObject("EventSystem");
                eventSystem = gameObject.AddComponent<EventSystem>();
                gameObject.AddComponent<StandaloneInputModule>();
            }
        }
    }
}
