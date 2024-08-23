using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace EyE.EditorUnity.Extensions
{
    /// \ingroup UnityEyETools
    /// \ingroup UnityEyEToolsEditor
    /// <summary>
    /// Contains extension functions for SerializedProperty to allow direct access to their values via reflection.
    /// </summary>
    internal static class SerializedPropertyExtensions
    {

        #region SerializedProperty functions

        /// <summary>
        /// Given a Unity SerialiazatbleProperty path, get the field it points to.
        /// </summary>
        /// <param name="type">type of object to look in at the specified serializedProperty path</param>
        /// <param name="path">Unity SerialiazatbleProperty path of the field in representedBy the SerialziedProperty</param>
        /// <returns>AfieldInfo instance that contains details about the found field.  Null, if a field could not be found at the path.</returns>
        public static System.Reflection.FieldInfo GetFieldViaPath(this System.Type type, string path)
        {
            FieldInfo fieldAtPath;
            System.Type arrayElementType;
            GetSetValueViaPath(null, path, out fieldAtPath, out arrayElementType, type);

            return fieldAtPath;

        }


        /// <summary>
        /// Given a Unity SerialiazatbleProperty path, and an object reference, get the value in the object's field referenced by the path.
        /// </summary>
        /// <param name="rootObject">object that contains the field from which to get the value.</param>
        /// <param name="path">Unity SerialiazatbleProperty path of the field to extract the value from.</param>
        /// <returns>the value found in the field.</returns>
        public static object GetValueViaPath(this object rootObject, string path)
        {
            FieldInfo fieldAtPath;
            System.Type arrayElementType;
            return GetSetValueViaPath(rootObject, path, out fieldAtPath, out arrayElementType, null);
        }

        /// <summary>
        /// Given a Unity SerialiazatbleProperty path, and an object reference, set the value in the object's field referenced by the path to the value specified in ValueToAssign.
        /// </summary>
        /// <param name="rootObject">object that contains the field whose value will be assigned.</param>
        /// <param name="path">Unity SerialiazatbleProperty path of the field to assign the value to.</param>
        /// <param name="ValueToAssign">The value that will be assigned to the field.  Passing an incorrect type of value will raise an exception.</param>
        public static void SetValueViaPath(this object rootObject, string path, object ValueToAssign)
        {
            FieldInfo fieldAtPath;
            System.Type arrayElementType;
            GetSetValueViaPath(rootObject, path, out fieldAtPath, out arrayElementType, null, ValueToAssign);
        }

        /// <summary>
        /// Simply a Unique Type
        /// </summary>
        class DefaultObject { };
        /// <summary>
        /// Default parameter is a constant value that cannot possibly referenced by user by users of this class.
        /// </summary>
        const DefaultObject defaultParamValue = default(DefaultObject);

        /// <summary>
        /// Internal function used to perform the reflection and path parsing used to find the field, and assign/extract it's value.
        /// </summary>
        /// <param name="rootObject">if null- will not return a value, nor assign the optionalValueToAssign. But, even if rootObject is null, will assign the fieldAtPath out param</param>
        /// <param name="path"></param>
        /// <param name="fieldAtPath">will be assigned a FIELDInfo that represents the field at the specified path.  
        /// If the path represents an element of an array, the fieldAtPath will reference the array field itself, and the out param arrayElementType will be set to the appropriate type value. </param>
        /// <param name="arrayElementType">will only be assigned a non-null value if the path references an array element.</param>
        /// <param name="rootObjectType">ignored unless rootObject is null</param>
        /// <param name="optionalValueToAssign">If not specified by the user, the function will not assign it's value to the field.  null is a valid assignment value.  Requires rootObject be non-null</param>
        /// <returns>the value found in rootObject's field specified by the path.</returns>
        private static object GetSetValueViaPath(object rootObject, string path, out FieldInfo fieldAtPath, out System.Type arrayElementType, System.Type rootObjectType, object optionalValueToAssign = defaultParamValue)
        {
            System.Type type = rootObjectType;
            arrayElementType = null;
            if (rootObject != null)
                type = rootObject.GetType();
            fieldAtPath = type.GetField(path, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldAtPath != null)// found it directly with path- out param already assigned a valid field
            {
                //     Debug.Log("Found field in one.  Parent Type: " + type + "  fieldName:" + fieldAtPath.Name + " full path:" + path);
                if (rootObject == null) return null;
                if (optionalValueToAssign != defaultParamValue)
                    fieldAtPath.SetValue(rootObject, optionalValueToAssign);
                return fieldAtPath.GetValue(rootObject);
            }
            string[] perDot = path.Split('.');
            object returnValue = null;
            System.Type parentType = type;
            object parentObject = rootObject;
            object currentObject = rootObject;
            //Debug.Log("START Getting field.  Parent Type: " + parentType + " full path:" + path + " parentObject: " + parentObject);
            for (int i = 0; i < perDot.Length; i++)
            {
                currentObject = parentObject;
                string fieldName = perDot[i];
                //Debug.Log("LOOP: "+i+" of "+perDot.Length+" Getting field.  Parent Type: " + parentType + "  fieldName:" + fieldName + " full path:" +path + " parentObject: " + parentObject);
                if (fieldName == "Array")//if an array element is referenced
                {
                    //  fieldAtPath = null; //a null value for fieldAtPath indicates this is an array element, which does not have a field info
                    System.Type elementType = null;
                    if (parentType.IsArray)
                        elementType = parentType.GetElementType();
                    else
                    {
                        if(parentType.GetGenericTypeDefinition() == typeof(List<>))
                        //if (typeof(IList<>).IsAssignableFrom(parentType))
                        {
                            elementType = parentType.GetGenericArguments()[0];
                        }
                        else
                        {
                            //CatDebug.LogWarning(UnityToolsCatDebug.EditorToolsCategoryID, "GetSetValueViaPath function:  Failed to get element type for parentType array: " + parentType.ToString());
                            Debug.Log("Failed to get element type for parentType array: " + parentType);
                        }
                    }


                    string indexStr = perDot[++i];
                    if (parentObject != null)
                    {
                        //next perDot contains index, in brackets prefaced by "data"

                        indexStr = indexStr.Substring(5, indexStr.Length - 6);// remove brackets and "data" keyword
                        int index = System.Convert.ToInt32(indexStr);

                        object array = parentObject;
                        if (((IList)array).Count <= index)
                        {
                            //CatDebug.LogWarning(UnityToolsCatDebug.EditorToolsCategoryID, "GetSetValueViaPath function:  Array object " + parentObject + "    Element index: " + index + " index is out of range");
                            Debug.LogWarning("GetSetValueViaPath function:  Array object " + parentObject + "    Element index: " + index + " index is out of range");
                            return null;
                        }
                        else
                        {
                            parentObject = ((IList)array)[index];
                            if (parentObject == null)
                            {
                                //CatDebug.LogWarning(UnityToolsCatDebug.EditorToolsCategoryID, "GetSetValueViaPath function: Array " + parentObject + ",  Element at index: " + index + " is null");
                                Debug.LogWarning("GetSetValueViaPath function: Array " + parentObject + ",  Element at index: " + index + " is null");
                                return null;
                            }
                        }
                        parentType = elementType;
                    }

                    if (i + 1 >= perDot.Length) //if path ends with next perDot
                    {
                        if (optionalValueToAssign != defaultParamValue)
                            parentObject = optionalValueToAssign;
                        arrayElementType = elementType;
                        return parentObject;
                    }

                }
                else//normal- non array field
                {

                    fieldAtPath = parentType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldAtPath != null)
                    {

                        if (parentObject != null)
                        {
                            parentObject = fieldAtPath.GetValue(parentObject);
                        }
                        parentType = fieldAtPath.FieldType;
                    }
                    else
                    {
                        Debug.Log("Failed to find field:" + fieldName + " in parent type:" + parentType);
                        // returnValue = null;
                    }
                }//normal- non array field
            }// end loop fields in path
            if (fieldAtPath != null && currentObject != null)
            {
                if (optionalValueToAssign != defaultParamValue)
                    fieldAtPath.SetValue(currentObject, optionalValueToAssign); //parentObject, optionalValueToAssign);// currentObject, optionalValueToAssign);
                returnValue = fieldAtPath.GetValue(currentObject);// parentObject);// currentObject);
            }
            // else returnValue = null;

            return returnValue;
        }

        static Dictionary<SerializedPropertyType, System.Type> TypeBySerializePropertyType = new Dictionary<SerializedPropertyType, System.Type>()
        {
            { SerializedPropertyType.AnimationCurve, typeof(AnimationCurve) },
            //{ SerializedPropertyType.ArraySize, typeof(float) },
            { SerializedPropertyType.Boolean, typeof(bool) },
            { SerializedPropertyType.Bounds, typeof(Bounds) },
            { SerializedPropertyType.BoundsInt, typeof(BoundsInt) },
            { SerializedPropertyType.Character, typeof(char) },
            { SerializedPropertyType.Color, typeof(Color) },
            //{ SerializedPropertyType.Enum, typeof(Enum) },
            { SerializedPropertyType.ExposedReference, typeof(float) },
            //{ SerializedPropertyType.FixedBufferSize, typeof(FixedBufferSize) },
            { SerializedPropertyType.Float, typeof(float) },
            //{ SerializedPropertyType.Generic, typeof(float) },
            { SerializedPropertyType.Gradient, typeof(Gradient) },
            { SerializedPropertyType.Integer, typeof(int) },
            { SerializedPropertyType.LayerMask, typeof(LayerMask) },
            //{ SerializedPropertyType.ObjectReference, typeof(Object) },
            { SerializedPropertyType.Quaternion, typeof(Quaternion) },
            { SerializedPropertyType.Rect, typeof(Rect) },
            { SerializedPropertyType.RectInt, typeof(RectInt) },
            { SerializedPropertyType.String, typeof(string) },
            { SerializedPropertyType.Vector2, typeof(Vector2) },
            { SerializedPropertyType.Vector2Int, typeof(Vector2Int) },
            { SerializedPropertyType.Vector3, typeof(Vector3) },
            { SerializedPropertyType.Vector3Int, typeof(Vector3Int) },
            { SerializedPropertyType.Vector4, typeof(Vector4) },

        };

        public static FieldInfo GetFieldInfo(this SerializedProperty property)
        {
            System.Type type = property.serializedObject.targetObject.GetType();
            FieldInfo fieldAtPath = GetFieldViaPath(type, property.propertyPath);
            //FieldInfo fieldAtPath = type.GetField(property.propertyPath, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return fieldAtPath;
            
        }

        /// <summary>
        /// This SerializedPropety extension function takes an instance of a Serialized Property and returns the System.Type of the field it represents.
        /// If the property is an array or list element, the type returned will be the type of the elements in the array.
        /// </summary>
        /// <param name="property">The function will extract the System.Type of this SerializedProperty.</param>
        /// <returns>The System.Type that represents the type of the field. null in the event of a failure to find the field.</returns>
        public static System.Type GetFieldType(this SerializedProperty property)
        {

            System.Type value;
            if (TypeBySerializePropertyType.TryGetValue(property.propertyType, out value))
            {
                return value;
            }

            System.Type type = property.serializedObject.targetObject.GetType();
            //arrayElementType = null;
            FieldInfo fieldAtPath = type.GetField(property.propertyPath, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldAtPath != null)// found it directly with path- out param already assigned a valid field
            {
                //     Debug.Log("Found field in one.  Parent Type: " + type + "  fieldName:" + fieldAtPath.Name + " full path:" + path);
                return fieldAtPath.FieldType;
            }
            string[] perDot = property.propertyPath.Split('.');

            //Debug.Log("START Getting field.  Parent Type: " + parentType + " full path:" + path + " parentObject: " + parentObject);
            for (int i = 0; i < perDot.Length; i++)
            {
                string fieldName = perDot[i];
                //Debug.Log("LOOP: "+i+" of "+perDot.Length+" Getting field.  Parent Type: " + parentType + "  fieldName:" + fieldName + " full path:" +path + " parentObject: " + parentObject);
                if (fieldName == "Array")//if an array element is referenced
                {
                    if (type.IsArray)
                        type = type.GetElementType();
                    else
                    {
                        if (typeof(ICollection).IsAssignableFrom(type))
                        {
                            type = type.GetGenericArguments()[0];
                        }
                        else
                        {
                            //CatDebug.LogWarning(UnityToolsCatDebug.EditorToolsCategoryID, "GetSetValueViaPath function:  Failed to get element type for a serializedProperty reference by path (" + property.propertyPath + "), because it is not actually an array type, nor is it assignable to an ICollection. Actual type found:" + type.ToString());
                            Debug.LogWarning("GetSetValueViaPath function:  Failed to get element type for a serializedProperty reference by path (" + property.propertyPath + "), because it is not actually an array type, nor is it assignable to an ICollection. Actual type found:" + type.ToString());
                            return null;
                        }
                    }
                    i++; //next perDot contains index, in brackets prefaced by "data"
                    if (i + 1 >= perDot.Length) //if path ends with next perDot
                    {
                        return type;
                    }
                }
                else//normal- non array field
                {
                    fieldAtPath = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldAtPath != null)
                    {
                        type = fieldAtPath.FieldType;
                    }
                    else
                    {
                        //CatDebug.LogWarning(UnityToolsCatDebug.EditorToolsCategoryID, "Failed to find field:" + fieldName + " in parent type:" + type);
                        Debug.LogWarning("Failed to find field:" + fieldName + " in parent type:" + type);
                        return null;
                    }
                }//normal- non array field
            }// end loop fields in path
            return type;
        }

        /// <summary>
        /// This SerializedPropety extension function takes an instance of a Serialized Property and returns the runtime contents of the field it represents.
        /// </summary>
        /// <param name="property">The function will extract the runtime contents of this SerializedProperty.</param>
        /// <param name="displayWarnings">When True, and warnings generated when attempting to get value referenced by the SerializedProperty, will be displayed in the console.  Defaults to False (failure without notification).</param>
        /// <returns>The value contained in the filed.  This value can be cast to the appropriate class in order to access it's members.</returns>
        public static object GetValue(this SerializedProperty property, bool displayWarnings = false)
        {
            object parentObject = property.serializedObject.targetObject;

            string path = property.propertyPath;
            return parentObject.GetValueViaPath(path);// GetValueByPath(parentObject, path, property.name);
        }

        /// <summary>
        /// This SerializedPropety extension function takes an instance of a Serialized Property and assigns a new value to the runtime contents of the field it represents.
        /// </summary>
        /// <param name="property">The function will assign a value to the runtime contents of this SerializedProperty.</param>
        /// <param name="value">This is the object derived value that will be stored in the runtime field.</param>
        /// <param name="displayWarnings">When True, and warnings generated when attempting to set the value referenced by the SerializedProperty, will be displayed in the console.  Defaults to False (failure without notification).</param>
        public static void SetValue(this SerializedProperty property, object value, bool displayWarnings = false)
        {
            property.serializedObject.targetObject.SetValueViaPath(property.propertyPath, value);
            return;
        }


        /// <summary>
        /// Extension function for SerializedProperties.  If the SerializedProperty is a type of Scriptable Object, the normal FindPropertyRelative function does not work right. 
        /// This version checks for that case, and provides necessary workarounds, transparently.
        /// </summary>
        /// <param name="sp">This is the serialixedProperty from which we want to get a relative/sub property.</param>
        /// <param name="name">string that represents the field name of the relative/sub property.</param>
        /// <param name="objectToApplyChanges">This ref parameter, upon exit, will either be null, or contain a reference to a SerializedObject that contains the property.  It is this SerializedObject to which changes must be applied, by the user of this function, in order for them to be serialized properly.</param>
        /// <returns>The serializedProperty found with the field name, relative to the provided SerializedProperty.  Null if no such field could be found.</returns>
        static public SerializedProperty FindPropertyRelativeFix(this SerializedProperty sp, string name, ref SerializedObject objectToApplyChanges)
        {
            SerializedProperty result;
            if (typeof(ScriptableObject).IsAssignableFrom(sp.GetFieldType()))
            {
                if (sp.objectReferenceValue == null) return null;
                if (objectToApplyChanges == null)
                    objectToApplyChanges = new SerializedObject(sp.objectReferenceValue);
                result = objectToApplyChanges.FindProperty(name);
            }
            else
            {
                objectToApplyChanges = null;
                result = sp.FindPropertyRelative(name);
            }
            return result;
        }
        #endregion

    }

}