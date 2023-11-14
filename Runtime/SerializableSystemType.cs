using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[System.Serializable]
public class SerializableSystemType : ISerializationCallbackReceiver
{
    public Type type;

    //[HideInInspector]
    [SerializeField]
    public string typeName;

    public SerializableSystemType(Type type)
    {
        this.type = type;
    }

    public void OnBeforeSerialize()
    {
        if (type != null)
        {
            typeName = type.AssemblyQualifiedName;
           // Debug.Log("Success serializing : '" + typeName + "'");
        }
        else
        {
            if (typeName == null)
            {
              //  Debug.LogWarning("Unable to Serialize type:  Type member is null, and typeName is null");
               // typeName = "null";
            }
            if(Type.GetType(typeName) == null)
            {
            //    Debug.LogWarning("Unable to Serialize type:  Type member is null, and unable to find valid Type of name '" + typeName + "'");
               // typeName = "null";
            }
        }

    }
    public void OnAfterDeserialize()
    {
        if (typeName == null)
        {
            Debug.LogWarning("SerializableTypeInfo Error: Unable to Deserialize type- typeName is null");
        }
        type = Type.GetType(typeName);
        if (type == null)
        {
            Debug.LogWarning("SerializableTypeInfo Error: Unable to Deserialize type '" + typeName + "'");
            return;
        }

    }
    // Override Equals method
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Type objType = obj.GetType();
        if (objType == typeof(System.Type))
        {
            return type == (Type)obj;
        }
        if (GetType() != obj.GetType())
        {
            return false;
        }

        SerializableSystemType other = (SerializableSystemType)obj;
        return type == other.type;
    }

    // Override GetHashCode method
    public override int GetHashCode()
    {
        return type.GetHashCode();
    }
}
