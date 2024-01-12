#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ugames.Modules.ObjectReferenceMoudle
{

    /// <summary>
    /// Display a Scene Reference object in the editor.
    /// If scene is valid, provides basic buttons to interact with the scene's role in Build Settings.
    /// </summary>
    [CustomPropertyDrawer(typeof(ObjectReference))]
    public class ObjectReferencePropertyDrawer : PropertyDrawer
    {
        private const string sceneAssetPropertyString = "objectAsset";
        private const string scenePathPropertyString = "objectPath";

        private static readonly RectOffset boxPadding = EditorStyles.helpBox.padding;

        private const float PAD_SIZE = 2f;
        private const float FOOTER_HEIGHT = 10f;

        private static readonly float lineHeight = EditorGUIUtility.singleLineHeight;
        private static readonly float paddedLine = lineHeight + PAD_SIZE;

        /// <summary>
        /// Drawing the 'SceneReference' property
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Move this up
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            {
                property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, lineHeight),
                    property.isExpanded, label);

                if (property.isExpanded)
                {
                    position.height -= lineHeight;
                    position.y += lineHeight;

                    var sceneAssetProperty = GetSceneAssetProperty(property);

                    position.height -= FOOTER_HEIGHT;
                    GUI.Box(EditorGUI.IndentedRect(position), GUIContent.none, EditorStyles.helpBox);
                    position = boxPadding.Remove(position);
                    position.height = lineHeight;

                    label.tooltip =
                        "The actual Object reference.\nOn serialize this is also stored as the asset's path.";

                    EditorGUI.BeginChangeCheck();
                    {
                        // removed the label here since we already have it in the foldout before
                        sceneAssetProperty.objectReferenceValue = EditorGUI.ObjectField(position,
                            sceneAssetProperty.objectReferenceValue, typeof(Object), false);
                    }
                    var objectReferenceValue = sceneAssetProperty.objectReferenceValue;
                    if (EditorGUI.EndChangeCheck())
                    {
                        // If no valid scene asset was selected, reset the stored path accordingly
                        if (objectReferenceValue == null) GetObjectPathProperty(property).stringValue = string.Empty;
                    }

                    position.y += paddedLine;

                    /*if (!buildScene.assetGUID.Empty())
                    {
                        // Draw the Build Settings Info of the selected Scene
                        DrawSceneInfoGUI(position, buildScene, sceneControlID + 1);
                    }*/

                }
            }
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Ensure that what we draw in OnGUI always has the room it needs
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var sceneAssetProperty = GetSceneAssetProperty(property);
            // Add an additional line and check if property.isExpanded
            var lines = 2; // = property.isExpanded ? sceneAssetProperty.objectReferenceValue != null ? 3 : 2 : 1;

            return boxPadding.vertical + lineHeight * lines + PAD_SIZE * (lines - 1) + FOOTER_HEIGHT;
        }

        /// <summary>
        /// Draws info box of the provided scene
        /// </summary>
        private static SerializedProperty GetSceneAssetProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative(sceneAssetPropertyString);
        }

        private static SerializedProperty GetObjectPathProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative(scenePathPropertyString);
        }
    }
}
#endif