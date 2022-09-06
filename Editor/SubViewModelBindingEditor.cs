using UnityEngine;
using UnityEditor;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;
using System.Linq;
using System.Reflection;

namespace UnityWeld_Editor
{
    /// <summary>
    /// Inspector window for SubViewModelBinding
    /// </summary>
    [CustomEditor(typeof(SubViewModelBinding))]
    public class SubViewModelBindingEditor : BaseBindingEditor
    {
        private SubViewModelBinding targetScript;

        /// <summary>
        /// Whether or not the value on our target matches its prefab.
        /// </summary>
        private bool propertyPrefabModified;

        protected override  void OnEnabled()
        {
            targetScript = (SubViewModelBinding)target;
        }

        protected override void OnInspector()
        {
            UpdatePrefabModifiedProperties();

            BeginArea(new GUIContent("Sub View-Model"));

            var bindableProperties = FindBindableProperties();

            EditorStyles.label.fontStyle = propertyPrefabModified 
                ? FontStyle.Bold 
                : DefaultFontStyle;

            ShowViewModelPropertyMenu(
                new GUIContent(
                    "Property", 
                    "The Property on the top level View-Model containing the Sub View-Model"
                ),
                bindableProperties,
                updatedValue => 
                {
                    targetScript.ViewModelPropertyName = updatedValue;

                    targetScript.ViewModelTypeName = bindableProperties
                        .Single(prop => prop.ToString() == updatedValue)
                        .Member.PropertyType.ToString();
                },
                targetScript.ViewModelPropertyName,
                p => true
            );

            EndArea();
        }

        private BindableMember<PropertyInfo>[] FindBindableProperties()
        {
            return TypeResolver.FindBindableProperties(targetScript)
                .Where(prop => prop.Member.PropertyType.HasBindingAttribute()
                )
                .ToArray();
        }

        /// <summary>
        /// Check whether each of the properties on the object have been changed 
        /// from the value in the prefab.
        /// </summary>
        private void UpdatePrefabModifiedProperties()
        {
            var property = serializedObject.GetIterator();
            // Need to call Next(true) to get the first child. Once we have it, Next(false)
            // will iterate through the properties.

            propertyPrefabModified = false;
            property.Next(true);
            do
            {
                switch (property.name)
                {
                    case "viewModelPropertyName": 
                    case "viewModelTypeName":
                        propertyPrefabModified = property.prefabOverride 
                            || propertyPrefabModified;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}