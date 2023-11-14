using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
//using EyE.Sys.Reflection;
namespace EyE.EditorUnity.Extensions
{


    [CustomPropertyDrawer(typeof(SerializableSystemType))]
    public class SystemTypePropertyDrawer : PropertyDrawer
    {
        List<string> allTypesCashedNames = null;
        List<string> allTypesCashedAQNames = null;

        EditorPopupWithTextFilter filterPopupList = null;
        /*static int CompareTypeNames(Type t1, Type t2)
        {
            return t1.Name.CompareTo(t2.Name);
        }*/
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (filterPopupList == null)
                filterPopupList = new EditorPopupWithTextFilter();

            if (allTypesCashedNames == null)
            {
                List<Type> typesFound = new List<Type>();
                allTypesCashedNames = new List<string>();
                allTypesCashedAQNames = new List<string>();
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {

                    foreach (Type t in asm.GetExportedTypes())
                    {
                        typesFound.Add(t);
                        //  allTypesCashedNames.Add(t.Name);
                        // allTypesCashedAQNames.Add(t.AssemblyQualifiedName);
                    }
                }
                typesFound.Sort(
                    delegate (Type p1, Type p2)
                    {
                        return p1.Name.CompareTo(p2.Name);
                    });
                //allTypesCashedNames.Sort();
                foreach (Type t in typesFound)
                {
                    allTypesCashedNames.Add(t.Name);
                    allTypesCashedAQNames.Add(t.AssemblyQualifiedName);
                }
            }

            SerializableSystemType seriTypeObj = (SerializableSystemType)property.GetValue();
            //SerializableSystemType seriTypeObj = (SerializableSystemType)GetValue(property);
            if (seriTypeObj.type != null)
            {
                //EditorGUI.LabelField(position, "TypeName", seriTypeObj.type.Name);
                filterPopupList.SetText(seriTypeObj.type.Name);
            }


            string typeFQName = "*";
            int selected = filterPopupList.Draw(position, allTypesCashedNames, label);
            //int selected = filterPopupList.DrawByWindow(position, allTypesCashedNames);

            if (selected != -1)
            {
                
                string selectedTypeName = allTypesCashedAQNames[selected];

                if (selectedTypeName != null && selectedTypeName != "")
                {
                    //Debug.Log("Searching for type: " + selectedTypeName);
                    seriTypeObj.type = Type.GetType(selectedTypeName);
                }
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            if (seriTypeObj.type != null)
            {
                typeFQName = seriTypeObj.type.FullName;
            }

            position.height = EditorGUIUtility.singleLineHeight;
            position.y += filterPopupList.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            //position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            GUI.enabled = false;
            EditorGUI.TextField(position, "FQ Name", typeFQName);
            GUI.enabled = true;

        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (filterPopupList != null)
                height += filterPopupList.GetHeight();
            return height;
        }

    }
}