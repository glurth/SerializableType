using UnityEngine;
using System;
/// <summary>
/// This class facilitates serialization of System.Type objects in Unity by converting them to strings (using AssemblyQualifiedName) during serialization and back to types during deserialization.
/// </summary>
[System.Serializable]
public class SerializableSystemType : ISerializationCallbackReceiver
{
    /// <summary>
    /// The actual System.Type object being wrapped.
    /// </summary>
    public Type type;

    /// <summary>
    /// String representation of the type object (AssemblyQualifiedName). 
    /// Used for serialization purposes.
    /// </summary>
    [SerializeField]
    public string typeName;

    /// <summary>
    /// Constructor that initializes the class with a given System.Type object.
    /// </summary>
    /// <param name="type">The System.Type object to wrap.</param>
    public SerializableSystemType(Type type)
    {
        this.type = type;
    }

    /// <summary>
    /// Implicitly converts to a System.Type, so it can be used directly as one.
    /// </summary>
    /// <param name="st"></param>
    public static implicit operator Type(SerializableSystemType st) => st.type;

    /// <summary>
    /// This method is called before serialization. It converts the `type` object to its assembly qualified name and stores it in `typeName`.
    /// </summary>
    public void OnBeforeSerialize()
    {
        if (type != null)
        {
            typeName = type.AssemblyQualifiedName;
            // Debug.Log("Success serializing : '" + typeName + "'"); // Commented out for production use
        }
        else
        {
            if (typeName == null)
            {
                Debug.LogWarning("Unable to Serialize type:  Type member is null, and typeName is null"); // Commented out for production use
                // typeName = "null";
            }
            else if (Type.GetType(typeName) == null)
            {
                Debug.LogWarning("Unable to Serialize type:  Type member is null, and unable to find valid Type of name '" + typeName + "'"); // Commented out for production use
                // typeName = "null";
            }
        }
    }

    /// <summary>
    /// This method is called after deserialization. It attempts to convert the stored `typeName` back to a `Type` object.
    /// </summary>
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

