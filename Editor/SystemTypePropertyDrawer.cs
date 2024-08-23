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
        static List<string> allTypesCashedNames = null;
        static List<string> allTypesCashedAQNames = null;

        EditorPopupWithTextFilter filterPopupList = null;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
           // Debug.Log($"SystemTypePropertyDrawer Detected event: {Event.current.type}");
            SerializableSystemType seriTypeObj = (SerializableSystemType)property.GetValue();
            if (filterPopupList == null)
            {
                filterPopupList = new EditorPopupWithTextFilter();
                
                //SerializableSystemType seriTypeObj = (SerializableSystemType)GetValue(property);
                if (seriTypeObj.type != null)
                {
                    //EditorGUI.LabelField(position, "TypeName", seriTypeObj.type.Name);
                    filterPopupList.SetText(seriTypeObj.type.Name);
                }
                else
                {
                    filterPopupList.SetText("");
                }
            }

            if (allTypesCashedNames == null)
            {
                string limitToNamespace = GetLimitingNamespace(property);//new
                List<Type> typesFound = new List<Type>();
                allTypesCashedNames = new List<string>();
                allTypesCashedAQNames = new List<string>();
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {

                    foreach (Type t in asm.GetExportedTypes())
                    {
                        if(limitToNamespace==null || t.Namespace == limitToNamespace)//new
                            typesFound.Add(t);
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

            string typeFQName = "*";
            int selected = filterPopupList.Draw(position, allTypesCashedNames, label);

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

        /// <summary>
        /// Gets the limiting namespace from the property: 
        ///   -it gets the field and containing object type
        ///   - then useses that information to check if a EditorLimitSelectionToNamespaceAttribute has been applied to it.
        ///   -if so, it gets the EditorLimitSelectionToNamespaceAttribute's "Namespace" parameter and returns it.
        /// </summary>
        /// <param name="property">The serialized property being drawn.</param>
        /// <returns>The limiting namespace, or null if no namespace is specified. It also returns null if it is unable to find field from the property.</returns>
        public static string GetLimitingNamespace(SerializedProperty property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            // Get the field info and attribute
            FieldInfo fieldInfo = property.GetFieldInfo();// serializedObject.targetObject.GetType().GetField(property.propertyPath);
            if (fieldInfo == null) return null;

            EditorLimitSelectionToNamespaceAttribute attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(EditorLimitSelectionToNamespaceAttribute)) as EditorLimitSelectionToNamespaceAttribute;
            if(attribute==null) return null;
            return attribute.Namespace;
        }

    }
}