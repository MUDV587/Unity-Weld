using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(CollectionBinding))]
    class CollectionBindingEditor : BaseBindingEditor
    {
        private CollectionBinding _targetScript;
        private SerializedProperty _templateInitialPoolCountProperty;
        private SerializedProperty _itemsContainerProperty;
        private SerializedProperty _templatesProperty;
        
        private bool _viewModelPrefabModified;

        protected override void OnEnabled()
        {
            // Initialise everything
            _targetScript = (CollectionBinding)target;
            _templateInitialPoolCountProperty = serializedObject.FindProperty("_templateInitialPoolCount");
            _itemsContainerProperty = serializedObject.FindProperty("_itemsContainer");
            _templatesProperty = serializedObject.FindProperty("_templates");
        }

        protected override void OnInspector()
        {
            UpdatePrefabModifiedProperties();

            BeginArea(new GUIContent("View-Model"));

            EditorStyles.label.fontStyle = _viewModelPrefabModified ? FontStyle.Bold : DefaultFontStyle;
            ShowViewModelPropertyMenu(
                new GUIContent("Property", "Property on the View-Model to bind to."),
                TypeResolver.FindBindableCollectionProperties(_targetScript),
                updatedValue => _targetScript.ViewModelPropertyName = updatedValue,
                _targetScript.ViewModelPropertyName,
                property => true
            );

            EndArea();

            EditorGUILayout.Space();

            BeginArea(new GUIContent("Templates Settings"));
            
            EditorGUILayout.PropertyField(_templateInitialPoolCountProperty, new GUIContent("Initial Pool Count"));
            EditorGUILayout.PropertyField(_itemsContainerProperty, new GUIContent("Container"));

            EditorGUILayout.PropertyField(_templatesProperty, new GUIContent("Templates", "Templates for Collection"), true);

            EndArea();
        }

        /// <summary>
        /// Check whether each of the properties on the object have been changed from the value in the prefab.
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
                        _viewModelPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}
