using System;
using ugames.Modules.Reference.AssetReference;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


namespace ugames.Modules.ObjectReferenceMoudle
{
    [Serializable, Obsolete("Use " + nameof(AssetRef))]
    public class ObjectReference : ISerializationCallbackReceiver
    {
        // This should only ever be set during serialization/deserialization!
        [SerializeField] private string objectPath = string.Empty;
#if UNITY_EDITOR
        // What we use in editor to select the scene
        [SerializeField] private Object objectAsset;
        private bool IsValidSceneAsset
        {
            get
            {
                if (!objectAsset) return false;

                return objectAsset is Object;
            }
        }
#endif

       

        // Use this when you want to actually have the scene path
        public string ObjectPath
        {
            get
            {
#if UNITY_EDITOR
                // In editor we always use the asset's path
                return GetObjectPathFromAsset();
#else
            // At runtime we rely on the stored path value which we assume was serialized correctly at build time.
            // See OnBeforeSerialize and OnAfterDeserialize
            return objectPath;
#endif
            }
            set
            {
                objectPath = value;
#if UNITY_EDITOR
                objectAsset = GetObjectAssetFromPath();
#endif
            }
        }

        public string PrefabPathInResources
        {
            get
            {
                string[] pathWithoutResources = ObjectPath.Split(new[] {"Resources/"}, StringSplitOptions.None);
                if (pathWithoutResources.Length >= 2)
                {
                    string path = pathWithoutResources[1];
                    string[] pathWithoutPrefab = path.Split(new[] {".prefab"}, StringSplitOptions.None);
                    if (pathWithoutPrefab.Length >= 1)
                    {
                        return pathWithoutPrefab[0];
                    }

                    return string.Empty;
                }

                return string.Empty;
            }
        }

        public string ScriptableObjectPathInResources
        {
            get
            {
                string[] pathWithoutResources = ObjectPath.Split(new[] {"Resources/"}, StringSplitOptions.None);
                if (pathWithoutResources.Length >= 2)
                {
                    string path = pathWithoutResources[1];
                    string[] pathWithoutPrefab = path.Split(new[] {".asset"}, StringSplitOptions.None);
                    if (pathWithoutPrefab.Length >= 1)
                    {
                        return pathWithoutPrefab[0];
                    }

                    return string.Empty;
                }

                return string.Empty;
            }
        }

        public static implicit operator string(ObjectReference objectReference)
        {
            return objectReference.ObjectPath;
        }

        // Called to prepare this data for serialization. Stubbed out when not in editor.
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            HandleBeforeSerialize();
#endif
        }

        // Called to set up data for deserialization. Stubbed out when not in editor.
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            // We sadly cannot touch assetdatabase during serialization, so defer by a bit.
            EditorApplication.update += HandleAfterDeserialize;
#endif
        }



#if UNITY_EDITOR
        private Object GetObjectAssetFromPath()
        {
            return string.IsNullOrEmpty(objectPath) ? null : AssetDatabase.LoadAssetAtPath<Object>(objectPath);
        }

        private string GetObjectPathFromAsset()
        {
            return objectAsset == null ? string.Empty : AssetDatabase.GetAssetPath(objectAsset);
        }

        private void HandleBeforeSerialize()
        {
            // Asset is invalid but have Path to try and recover from
            if (IsValidSceneAsset == false && string.IsNullOrEmpty(objectPath) == false)
            {
                objectAsset = GetObjectAssetFromPath();
                if (objectAsset == null) objectPath = string.Empty;

                EditorSceneManager.MarkAllScenesDirty();
            }
            // Asset takes precendence and overwrites Path
            else
            {
                objectPath = GetObjectPathFromAsset();
            }
        }

        private void HandleAfterDeserialize()
        {
            EditorApplication.update -= HandleAfterDeserialize;
            if (IsValidSceneAsset) return;
            if (string.IsNullOrEmpty(objectPath)) return;

            objectAsset = GetObjectAssetFromPath();

            if (!objectAsset) objectPath = string.Empty;

            if (!Application.isPlaying) EditorSceneManager.MarkAllScenesDirty();
        }
#endif
    }
}
