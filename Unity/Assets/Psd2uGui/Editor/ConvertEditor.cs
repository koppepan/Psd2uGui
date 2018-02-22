using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using PhotoshopFile;

namespace Psd2uGui.Editor
{
    public class ConvertEditor : EditorWindow
    {
        static Dictionary<string, Layer[]> groups;

        [MenuItem("Convert/Convert")]
        static void Open()
        {
            var psd = new PsdFile(Application.dataPath + "/../../SamplePsd/Sample.psd", new LoadContext { Encoding = System.Text.Encoding.Default });
            groups = EditorUtil.GetLayerGroup(psd);

            var win = GetWindow<ConvertEditor>();
            win.Show();
        }

        private void OnGUI()
        {
            foreach (var group in groups)
            {
                foreach (var layer in group.Value)
                {
                    EditorGUILayout.LabelField(string.Format("{0}/{1}", group.Key, layer.Name));
                }
            }

            if (GUILayout.Button("Convert"))
            {
                var textures = new Dictionary<string, Texture2D>();
                foreach (var layer in groups.SelectMany(x => x.Value))
                {
                    if (layer.AdditionalInfo.Any(x => x is TextLayerInfo))
                    {
                        continue;
                    }

                    var path = string.Format("Assets/GUI/{0}.png", layer.Name);
                    if (!textures.ContainsKey(path))
                    {
                        textures.Add(path, EditorUtil.CreateTexture(layer));
                    }
                }
                EditorUtil.SaveAssets(textures);
            }
        }
    }
}
