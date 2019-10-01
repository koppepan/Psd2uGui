using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using PhotoshopFile;

namespace Psd2uGui.Editor
{
    class Converter
    {
        ConvertParameter param;
        Font fontData;

        Sprite[] sprites;
        List<LayerComponent> components;

        public Converter(ConvertParameter param, Sprite[] sprites, Font fontData)
        {
            this.param = param;
            this.sprites = sprites;
            this.fontData = fontData;
        }

        Sprite GetOrDefaultSprite(Layer layer)
        {
            if (layer == null)
            {
                return null;
            }
            return sprites.FirstOrDefault(x => string.Equals(x.name, layer.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        public List<LayerComponent> ConvertLayerComponents(Dictionary<Layer, string> layers)
        {
            components = new List<LayerComponent>();

            var layerGroups = new Dictionary<string, List<Layer>>();
            foreach (var layer in layers)
            {
                var key = layer.Value;
                if (!layerGroups.ContainsKey(key))
                {
                    layerGroups.Add(key, new List<Layer>());
                }
                layerGroups[key].Add(layer.Key);
            }

            foreach (var pair in layerGroups)
            {
                ConvertLayers(pair.Key, new List<Layer>(pair.Value));
            }

            return components;
        }

        void ConvertLayers(string path, List<Layer> layers)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var groupName = path.Contains("/") ? path.Split('/').Last() : path;
                var groupPath = path.Length > 0 ? path.Remove(path.Length - groupName.Length) : path;

                if (Regex.IsMatch(groupName.ToLower(), param.Button.Pattern))
                {
                    var list = layers.Where(x => Regex.IsMatch(x.Name.ToLower(), param.Button.Pattern)).ToList();
                    if (list.Any())
                    {
                        components.Add(new ButtonLayerComponent(groupName, groupPath, list, param.Button, GetOrDefaultSprite));
                        list.ForEach(x => layers.Remove(x));
                    }
                }
                if (Regex.IsMatch(groupName.ToLower(), param.Toggle.Pattern))
                {
                    var list = layers.Where(x => Regex.IsMatch(x.Name.ToLower(), param.Toggle.Pattern)).ToList();
                    if (list.Any())
                    {
                        components.Add(new ToggleLayerComponent(groupName, groupPath, list, param.Toggle, GetOrDefaultSprite));
                        list.ForEach(x => layers.Remove(x));
                    }
                }
            }

            foreach (var layer in layers)
            {
                //if (Regex.IsMatch(layer.Name.ToLower(), param.labelKey))
                if(layer.AdditionalInfo.Any(info => info is TextLayerInfo))
                {
                    components.Add(new TextLayerComponent(layer.Name, path, layer, fontData));
                }
                else
                {
                    var sprite = GetOrDefaultSprite(layer);
                    components.Add(new ImageLayerComponent(layer.Name, path, layer.Rect, sprite));
                }
            }
        }
    }
}
