using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using PhotoshopFile;

namespace Psd2uGui.Editor
{
    class SaveAssetEditor : IDisposable
    {
        private class TempLayer
        {
            public Layer layer;
            public Texture2D originTexture;
            public Sprite existTexture;

            public bool overWrite;
        }

        string assetSavePath;
        List<TempLayer> layers = new List<TempLayer>();
        Vector2 scrollPosition;

        public SaveAssetEditor(PsdFile psd, string assetSavePath)
        {
            this.assetSavePath = assetSavePath;

            foreach(var layer in psd.Layers)
            {
                if(layer.AdditionalInfo.Any(x => x is LayerSectionInfo))
                {
                    continue;
                }
                if (layer.AdditionalInfo.Any(x => x is TextLayerInfo))
                {
                    continue;
                }
                if(layer.Rect.width <= 0 || layer.Rect.height <= 0)
                {
                    continue;
                }
                if (layers.Any(x => x.layer.Name == layer.Name))
                {
                    continue;
                }
                
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(EditorUtil.GetAssetPath(assetSavePath, layer.Name)); 
                layers.Add(new TempLayer
                {
                    layer = layer,
                    originTexture = EditorUtil.CreateTexture(layer),
                    existTexture = sprite,

                    overWrite = sprite == null,
                });
            }

            layers = layers.OrderBy(x => x.layer.Name).ToList();
        }

        public void Dispose()
        {
            foreach(var tmp in layers)
            {
                GameObject.DestroyImmediate(tmp.originTexture);
            }
            layers.Clear();
        }

        public void Draw()
        {
            using (var scope = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                foreach (var tmp in layers)
                {
                    EditorGUILayout.ToggleLeft(tmp.layer.Name, tmp.overWrite);
                }

                scrollPosition = scope.scrollPosition;
            }
        }

        public void Save()
        {
            foreach(var tmp in layers)
            {
                if(tmp.overWrite)
                {
                    var path = EditorUtil.GetAssetPath(assetSavePath, tmp.layer.Name);
                    EditorUtil.SaveAsset(path, tmp.originTexture);
                }
            }
        }
    }
}
