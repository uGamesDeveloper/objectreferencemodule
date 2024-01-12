#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ugames.Modules.Reference.AssetReference
{
    [Serializable]
    public class AssetRef : ISerializationCallbackReceiver
    {
        [SerializeField] private string _assetPath = string.Empty;
        
#if UNITY_EDITOR
        [SerializeField] private Object _assetRef;
        
        private bool AssetIsValid
        {
            get
            {
                if (_assetRef == null) 
                    return false;
                return _assetRef is Object;
            }
        }
#endif

        public AssetRef(string path)
        {
            _assetPath = path;
#if UNITY_EDITOR
            _assetRef = GetObjectAssetFromPath();
#endif
        }

        public AssetRef()
        {
            
        }
 


#if UNITY_EDITOR
        public AssetRef(Object asset)
        {
            _assetRef = asset;
            _assetPath = GetObjectPathFromAsset();
        }

        public void SetGUID(string guid)
        {
            _assetPath = AssetDatabase.GUIDToAssetPath(guid);
            _assetRef = GetObjectAssetFromPath();
        }
#endif

        public string ObjectPath
        {
            get
            {
#if UNITY_EDITOR
                // In editor we always use the asset's path
                return GetObjectPathFromAsset();
#else
                return _assetPath;
#endif
            }
            set
            {
                _assetPath = value;
#if UNITY_EDITOR
                _assetRef = GetObjectAssetFromPath();
#endif
            }
        }
        
        public string PrefabPath
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
        

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            HandleBeforeSerialize();
#endif
        }

        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            EditorApplication.update += HandleAfterDeserialize;
#endif
        }
        

        
#if UNITY_EDITOR
        
        private void HandleBeforeSerialize()
        {
            if (AssetIsValid == false)
                _assetPath = string.Empty;
            
            if (AssetIsValid == false && string.IsNullOrEmpty(_assetPath) == false)
            {
                _assetRef = GetObjectAssetFromPath();
                if (_assetRef == null) _assetPath = string.Empty;

                EditorSceneManager.MarkAllScenesDirty();
            }
            else
            {
                _assetPath = GetObjectPathFromAsset();
            }
        }
        
        private void HandleAfterDeserialize()
        {
            EditorApplication.update -= HandleAfterDeserialize;
            if (AssetIsValid) return;
            if (string.IsNullOrEmpty(_assetPath)) return;

            _assetRef = GetObjectAssetFromPath();

            if (!_assetRef) _assetPath = string.Empty;

            if (!Application.isPlaying) EditorSceneManager.MarkAllScenesDirty();

        }
        
        private Object GetObjectAssetFromPath()
        {
            return string.IsNullOrEmpty(_assetPath) ? null : AssetDatabase.LoadAssetAtPath<Object>(_assetPath);
        }
        

        private string GetObjectPathFromAsset()
        {
            return _assetRef == null ? string.Empty : AssetDatabase.GetAssetPath(_assetRef);
        }

#endif
    }
}