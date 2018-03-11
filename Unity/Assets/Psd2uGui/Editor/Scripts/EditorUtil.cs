using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using PhotoshopFile;

namespace Psd2uGui.Editor
{
    static class EditorUtil
    {
        public static string GetAssetPath(string savePath, string name)
        {
            return string.Format("{0}/{1}.png", savePath, name);
        }

        public static Dictionary<Layer, string> GetHierarchyPath(PsdFile psd)
        {
            var dic = new Dictionary<Layer, string>();
            var stack = new Stack<string>();

            foreach (var layer in Enumerable.Reverse(psd.Layers))
            {
                var sectionInfo = (LayerSectionInfo)layer.AdditionalInfo.SingleOrDefault(x => x is LayerSectionInfo);
                if (sectionInfo == null)
                {
                    if (!layer.Visible)
                    {
                        continue;
                    }
                    if ((int)layer.Rect.width == 0 || (int)layer.Rect.height == 0)
                    {
                        continue;
                    }
                    dic.Add(layer, string.Join("/", stack.Reverse().ToArray()));
                }
                else
                {
                    switch (sectionInfo.SectionType)
                    {
                        case LayerSectionType.Layer:
                        case LayerSectionType.OpenFolder:
                        case LayerSectionType.ClosedFolder:
                            stack.Push(layer.Name);
                            break;

                        case LayerSectionType.SectionDivider:
                            stack.Pop();
                            break;
                    }
                }
            }

            return dic.Reverse().ToDictionary(k => k.Key, v => v.Value);
        }

        public static Sprite SaveAsset(string path, Texture2D tex)
        {
            FileInfo file = new FileInfo(path);
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }

            byte[] buf = tex.EncodeToPNG();
            File.WriteAllBytes(path, buf);
            AssetDatabase.ImportAsset(path);

            AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = false;

            AssetDatabase.ImportAsset(path);
            return (Sprite)AssetDatabase.LoadAssetAtPath(path, typeof(Sprite));
        }

        public static Texture2D CreateTexture(Layer layer)
        {
            if (!layer.Visible)
            {
                return null;
            }
            if ((int)layer.Rect.width == 0 || (int)layer.Rect.height == 0)
            {
                return null;
            }

            Texture2D tex = new Texture2D((int)layer.Rect.width, (int)layer.Rect.height, TextureFormat.RGBA32, true);

            tex.SetPixels32(LayerToColors(layer));
            tex.Apply();
            return tex;
        }

        static Color32[] LayerToColors(Layer layer)
        {
            Color32[] pixels = new Color32[(int)layer.Rect.width * (int)layer.Rect.height];

            Channel red = layer.Channels.GetId(0);
            Channel green = layer.Channels.GetId(1);
            Channel blue = layer.Channels.GetId(2);
            Channel alpha = layer.AlphaChannel;

            for (int i = 0; i < pixels.Length; i++)
            {
                byte r = red.ImageData[i];
                byte g = green.ImageData[i];
                byte b = blue.ImageData[i];
                byte a = 255;
                if (alpha != null)
                {
                    a = alpha.ImageData[i];
                }
                int mod = i % (int)layer.Rect.width;
                int n = (((int)layer.Rect.width - mod - 1) + i) - mod;
                pixels[pixels.Length - n - 1] = new Color32(r, g, b, a);
            }
            return pixels;
        }

    }
}
