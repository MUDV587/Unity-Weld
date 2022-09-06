using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(EventBinding))]
    public class EventBindingEditor : BaseBindingEditor
    {
        private EventBinding targetScript;

        // Whether or not the values on our target match its prefab.
        private bool viewEventPrefabModified;
        private bool viewModelMethodPrefabModified;

        protected override void OnEnabled()
        {
            targetScript = (EventBinding)target;
        }

        protected override void OnInspector()
        {
            UpdatePrefabModifiedProperties();

            BeginArea(new GUIContent("View"));

            EditorStyles.label.fontStyle = viewEventPrefabModified 
                ? FontStyle.Bold 
                : DefaultFontStyle;

            ShowEventMenu(
                new GUIContent("Event", "Event on the View to bind to."),
                UnityEventWatcher.GetBindableEvents(targetScript.gameObject)
                    .OrderBy(evt => evt.Name)
                    .ToArray(),
                updatedValue => targetScript.ViewEventName = updatedValue,
                targetScript.ViewEventName
            );

            EndArea();

            EditorGUILayout.Space();

            BeginArea(new GUIContent("View-Model"));

            EditorStyles.label.fontStyle = viewModelMethodPrefabModified 
                ? FontStyle.Bold 
                : DefaultFontStyle;

            ShowMethodMenu(new GUIContent("Method", "Method on the view - model to bind to."), targetScript, TypeResolver.FindBindableMethods(targetScript));

            EndArea();
        }

        /// <summary>
        /// Draws the dropdown for selecting a method from bindableViewModelMethods
        /// </summary>
        private void ShowMethodMenu(
            GUIContent label,
            EventBinding targetScript, 
            BindableMember<MethodInfo>[] bindableMethods
        )
        {
            InspectorUtils.DoPopup(
                new GUIContent(targetScript.ViewModelMethodName),
                label,
                m => m.ViewModelType + "/" + m.MemberName,
                m => true,
                m => m.ToString() == targetScript.ViewModelMethodName,
                m => UpdateProperty(
                    updatedValue => targetScript.ViewModelMethodName = updatedValue,
                    targetScript.ViewModelMethodName,
                    m.ToString(),
                    "Set bound view-model method"
                ),
                bindableMethods
                    .OrderBy(m => m.ViewModelTypeName)
                    .ThenBy(m => m.MemberName)
                    .ToArray()
            );
        }

        /// <summary>
        /// Check whether each of the properties on the object have been changed 
        /// from the value in the prefab.
        /// </summary>
        private void UpdatePrefabModifiedProperties()
        {
            var property = serializedObject.GetIterator();
            // Need to call Next(true) to get the first child. Once we have it, 
            // Next(false) will iterate through the properties.
            property.Next(true);
            do
            {
                switch (property.name)
                {
                    case "viewEventName":
                        viewEventPrefabModified = property.prefabOverride;
                        break;

                    case "viewModelMethodName":
                        viewModelMethodPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}
