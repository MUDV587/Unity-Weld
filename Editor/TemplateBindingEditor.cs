using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(TemplateBinding))]
    class TemplateBindingEditor : BaseBindingEditor
    {
        private TemplateBinding targetScript;

        private bool viewModelPrefabModified;
        private SerializedProperty _templatesProperty;

        protected override  void OnEnabled()
        {
            targetScript = (TemplateBinding)target;
            _templatesProperty = serializedObject.FindProperty("_templates");
        }

        protected override void OnInspector()
        {
            UpdatePrefabModifiedProperties();

            BeginArea(new GUIContent("Template"));

            EditorStyles.label.fontStyle = viewModelPrefabModified 
                ? FontStyle.Bold 
                : DefaultFontStyle;

            ShowViewModelPropertyMenu(
                new GUIContent(
                    "Property", 
                    "Property on the View-Model to use for selecting Templates."
                ),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.ViewModelPropertyName = updatedValue,
                targetScript.ViewModelPropertyName,
                property => true
            );

            EndArea();

            EditorGUILayout.PropertyField(_templatesProperty, new GUIContent("Templates", "Templates for Collection"), true);
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
            property.Next(true);
            do
            {
                switch (property.name)
                {
                    case "viewModelPropertyName":
                        viewModelPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}
