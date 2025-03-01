using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(ToggleActiveBinding))]
    public class ToggleActiveBindingEditor : BaseBindingEditor
    {
        private ToggleActiveBinding targetScript;

        private AnimBool viewAdapterOptionsFade;

        private bool viewAdapterPrefabModified;
        private bool viewAdapterOptionsPrefabModified;
        private bool viewModelPropertyPrefabModified;

        protected override void OnEnabled()
        {
            targetScript = (ToggleActiveBinding)target;

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


            BeginArea(new GUIContent("View"));

            var viewPropertyType = typeof(bool);

            var viewAdapterTypeNames = TypeResolver.GetAdapterIds(o => o.OutType == viewPropertyType);

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
                        Undo.RecordObject(targetScript, "Set view adapter options");
                        targetScript.ViewAdapterOptions = null;
                    }

                    UpdateProperty(
                        updatedValue => targetScript.ViewAdapterId = updatedValue,
                        targetScript.ViewAdapterId,
                        newValue,
                        "Set view adapter"
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
                    "Property on the View-Model to bind to."
                ),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.ViewModelPropertyName = updatedValue,
                targetScript.ViewModelPropertyName,
                property => property.PropertyType == adaptedViewPropertyType
            );

            EndArea();
        }

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
                }
            }
            while (property.Next(false));
        }
    }
}
