using System;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(OneWayPropertyBinding))]
    class OneWayPropertyBindingEditor : BaseBindingEditor
    {
        private OneWayPropertyBinding targetScript;

        private AnimBool viewAdapterOptionsFade;

        // Whether each property in the target differs from the prefab it uses.
        private bool viewAdapterPrefabModified;
        private bool viewAdapterOptionsPrefabModified;
        private bool viewModelPropertyPrefabModified;
        private bool viewPropertyPrefabModified;

        protected override void OnEnabled()
        {
            // Initialise reference to target script
            targetScript = (OneWayPropertyBinding)target;

            viewAdapterOptionsFade = new AnimBool(ShouldShowAdapterOptions(targetScript.ViewAdapterId, out _));
            viewAdapterOptionsFade.valueChanged.AddListener(Repaint);
        }

        private void OnDisable()
        {
            viewAdapterOptionsFade.valueChanged.RemoveListener(Repaint);
        }

        protected override void OnInspector()
        {
            UpdatePrefabModifiedProperties();

            //var defaultLabelStyle = EditorStyles.label.fontStyle;

            BeginArea(new GUIContent("View"));

            EditorStyles.label.fontStyle = viewPropertyPrefabModified 
                ? FontStyle.Bold 
                : DefaultFontStyle;

            Type viewPropertyType;
            ShowViewPropertyMenu(
                new GUIContent("Property", "Property on the View to bind to"),
                PropertyFinder.GetBindableProperties(targetScript.gameObject),
                updatedValue => targetScript.ViewPropertyName = updatedValue,
                targetScript.ViewPropertyName,
                out viewPropertyType
            );

            // Don't let the user set anything else until they've chosen a view property.
            var guiPreviouslyEnabled = GUI.enabled;
            if (string.IsNullOrEmpty(targetScript.ViewPropertyName))
            {
                GUI.enabled = false;
            }

            var viewAdapterTypeNames = TypeResolver.GetAdapterIds(
                o => viewPropertyType == null || o.OutType == viewPropertyType);

            EditorStyles.label.fontStyle = viewAdapterPrefabModified 
                ? FontStyle.Bold 
                : DefaultFontStyle;

            ShowAdapterMenu(
                new GUIContent(
                    "Adapter", 
                    "Adapter that converts values sent from the View-Model to the View."
                ),
                viewAdapterTypeNames,
                targetScript.ViewAdapterId,
                newValue =>
                {
                    // Get rid of old adapter options if we changed the type of the adapter.
                    if (newValue != targetScript.ViewAdapterId)
                    {
                        Undo.RecordObject(targetScript, "Set view adapter Options");
                        targetScript.ViewAdapterOptions = null;
                    }

                    UpdateProperty(
                        updatedValue => targetScript.ViewAdapterId = updatedValue,
                        targetScript.ViewAdapterId,
                        newValue,
                        "Set View Adapter"
                    );
                }
            );

            Type adapterType;
            viewAdapterOptionsFade.target = ShouldShowAdapterOptions(
                targetScript.ViewAdapterId, 
                out adapterType
            );

            EditorStyles.label.fontStyle = viewAdapterOptionsPrefabModified 
                ? FontStyle.Bold 
                : DefaultFontStyle;

            ShowAdapterOptionsMenu(
                "Options", 
                adapterType, 
                options => targetScript.ViewAdapterOptions = options,
                targetScript.ViewAdapterOptions,
                viewAdapterOptionsFade.faded
            );

            EndArea();

            EditorGUILayout.Space();

            BeginArea(new GUIContent("View-Model"));

            EditorStyles.label.fontStyle = viewModelPropertyPrefabModified 
                ? FontStyle.Bold 
                : DefaultFontStyle;

            var adaptedViewPropertyType = AdaptTypeBackward(
                viewPropertyType, 
                targetScript.ViewAdapterId
            );
            ShowViewModelPropertyMenu(
                new GUIContent(
                    "Property", 
                    "Property on the View-Model to bind To."
                ),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.ViewModelPropertyName = updatedValue,
                targetScript.ViewModelPropertyName,
                property => property.PropertyType == adaptedViewPropertyType
            );

            EndArea();

            GUI.enabled = guiPreviouslyEnabled;

            //EditorStyles.label.fontStyle = DefaultFontStyle;
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
                    case "viewAdapterTypeName":
                        viewAdapterPrefabModified = property.prefabOverride;
                        break;

                    case "viewAdapterOptions":
                        viewAdapterOptionsPrefabModified = property.prefabOverride;
                        break;

                    case "viewModelPropertyName":
                        viewModelPropertyPrefabModified = property.prefabOverride;
                        break;

                    case "viewPropertyName":
                        viewPropertyPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}
