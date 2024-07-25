using System;
using UnityEngine;

/// <summary>
/// Attribute to specify a namespace that limits the types available for selection in the Unity inspector.
/// </summary>
/// <example>
/// <code>
/// using UnityEngine;
///
/// public class ExampleBehaviour : MonoBehaviour
/// {
///     /// <summary>
///     /// Type limited to the "System.Collections" namespace.
///     /// </summary>
///     [EditorLimitSelectionToNamespace("System.Collections")]
///     public SerializableType limitedType;
///
///     /// <summary>
///     /// Generic type with no namespace limitation.
///     /// </summary>
///     public SerializableType genericType;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class EditorLimitSelectionToNamespaceAttribute : PropertyAttribute
{
    /// <summary>
    /// Gets the namespace used to limit the type selection.
    /// </summary>
    public string Namespace { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EditorLimitSelectionToNamespaceAttribute"/> class.
    /// </summary>
    /// <param name="ns">The namespace to limit the type selection.</param>
    public EditorLimitSelectionToNamespaceAttribute(string limitToNamespace)
    {
        Namespace = limitToNamespace;
    }
}
