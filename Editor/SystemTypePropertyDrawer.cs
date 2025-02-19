using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using EyE.Unity;
//using EyE.Sys.Reflection;
namespace EyE.EditorUnity
{
    
    [CustomPropertyDrawer(typeof(SerializableSystemType))]
    public class SystemTypePropertyDrawer : PropertyDrawer
    {
        //parallel singleton lists - caches of type look ups
        static List<GUIContent> allTypesCashedNames = null;
        static List<string> allTypesCashedAQNames = null;
        List<GUIContent> limitedNamespaceTypesCashedNames = null;
        List<string> limitedNamespaceTypesCashedAQNames = null;
        int selectedIndex =-1;
        

        static Dictionary<string, EditorFilteredFoldoutList> filterPopupListByPropertyPath = new Dictionary<string, EditorFilteredFoldoutList>();
        EditorFilteredFoldoutList GetOrCreatePopup(SerializedProperty prop, string defaultFilterText)
        {
            EditorFilteredFoldoutList filterPopupList;
            string key = GenerateKeyID(prop);
          //  Debug.Log("Popup list count: " + filterPopupListByPropertyPath.Count);
            if (!filterPopupListByPropertyPath.TryGetValue(key, out filterPopupList))//lazy create
            {
               // Debug.Log("Creating new EditorPopupWithTextFilter for property: " + prop.propertyPath);
                filterPopupList = new EditorFilteredFoldoutList(key, defaultFilterText);
                filterPopupListByPropertyPath.Add(key, filterPopupList);
            }
            return filterPopupList;
        }
        string GenerateKeyID(SerializedProperty prop)
        {
            return prop.serializedObject.targetObject.GetInstanceID() + "_" + prop.propertyPath.GetHashCode();
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty typeNameProperty = property.FindPropertyRelative("typeName");
            Type foundType = Type.GetType(typeNameProperty.stringValue);
            string foundTypeShortName = "*";
            string typeFQName = "*";
            if (foundType != null)
            {
                typeFQName = foundType.FullName;
                label.tooltip = typeFQName;
                foundTypeShortName = foundType.Name;
            }
            EditorFilteredFoldoutList filterPopupList = GetOrCreatePopup(property, foundTypeShortName);

            if (allTypesCashedNames == null) // lazy load
            {
                //string limitToNamespace = GetLimitingNamespace();//new
                List<Type> typesFound = new List<Type>();
                allTypesCashedNames = new List<GUIContent>();
                allTypesCashedAQNames = new List<string>();
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {

                    foreach (Type t in asm.GetExportedTypes())
                    {
                        //if(limitToNamespace==null || t.Namespace == limitToNamespace)//new
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
                    allTypesCashedNames.Add(new GUIContent(t.Name,t.Namespace));
                    allTypesCashedAQNames.Add(t.AssemblyQualifiedName);
                }
            }// end init singlton

            if (limitedNamespaceTypesCashedNames == null)
            {
                limitedNamespaceTypesCashedNames = new List<GUIContent>();
                limitedNamespaceTypesCashedAQNames = new List<string>();
                string limitToNamespace = GetLimitingNamespace();
                if (string.IsNullOrEmpty(limitToNamespace))
                {
                    limitedNamespaceTypesCashedNames = allTypesCashedNames;
                    limitedNamespaceTypesCashedAQNames = allTypesCashedAQNames;
                }
                else
                {
                    foreach (string FQname in allTypesCashedAQNames)
                    {
                        Type t = Type.GetType(FQname);
                        if (t!=null && t.Namespace!=null && t.Namespace.Contains(limitToNamespace))
                        {
                            limitedNamespaceTypesCashedNames.Add(new GUIContent(t.Name, t.Namespace));
                            limitedNamespaceTypesCashedAQNames.Add(FQname);
                        }
                    }
                }
            }// init filtered list

            selectedIndex = filterPopupList.Draw(position, limitedNamespaceTypesCashedNames, label);

            if (selectedIndex != -1)
            {

                GUIContent selectedTypeName = limitedNamespaceTypesCashedNames[selectedIndex];
                int indexInAllTypesCashedNames = allTypesCashedNames.FindIndex((x) => { return x.text == selectedTypeName.text; });
                if (indexInAllTypesCashedNames == -1) 
                    Debug.LogError("Unexpected: unable to find " + selectedTypeName.text + " in allTypesCashedNames");
                string fqName = allTypesCashedAQNames[indexInAllTypesCashedNames];
                if (selectedTypeName != null && !string.IsNullOrEmpty(selectedTypeName.text))
                {
                 //   Debug.Log($"Assigning selection to: {typeNameProperty.propertyPath} selected = {fqName}");
                    typeNameProperty.stringValue = fqName;
                    //property.serializedObject.ApplyModifiedProperties();
                    selectedIndex = -1;
                }
            }

            position.height = EditorGUIUtility.singleLineHeight;
            position.y += filterPopupList.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;// EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty typeNameProperty = property.FindPropertyRelative("typeName");
            Type foundType = Type.GetType(typeNameProperty.stringValue);
            string defaultFilterValue = "*";
            if(foundType!=null) defaultFilterValue = foundType.Name;

            //float popupheight= EditorFilterableDropdown.FilterableDropdownHeight("SelectedType:", selectedIndex, limitedNamespaceTypesCashedNames, ref popupData);
            //height += popupheight;
            
            EditorFilteredFoldoutList filterPopupList = GetOrCreatePopup(property, defaultFilterValue);

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
        string GetLimitingNamespace()
        {
           // if (property == null) throw new ArgumentNullException(nameof(property));

            // Get the field info and attribute
            FieldInfo fieldInfo = this.fieldInfo;// property.fie.GetFieldInfo();// serializedObject.targetObject.GetType().GetField(property.propertyPath);
            if (fieldInfo == null) return null;

            EditorLimitSelectionToNamespaceAttribute attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(EditorLimitSelectionToNamespaceAttribute)) as EditorLimitSelectionToNamespaceAttribute;
            if(attribute==null) return null;
            if (string.IsNullOrEmpty(attribute.Namespace))
                return fieldInfo.DeclaringType.Namespace;
            return attribute.Namespace;
        }

    }


}