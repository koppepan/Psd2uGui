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
            var groupName = path.Contains("/") ? path.Split('/').Last() : path;

            if (Regex.IsMatch(groupName.ToLower(), param.buttonKey))
            {
                CreateButtonComponent(path, groupName, ref layers);
            }

            foreach (var layer in layers)
            {
                if (Regex.IsMatch(layer.Name.ToLower(), param.labelKey))
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

        void CreateButtonComponent(string path, string groupName, ref List<Layer> layers)
        {
            var buttons = layers.Where(x => Regex.IsMatch(x.Name.ToLower(), param.buttonKey)).ToArray();
            if (!buttons.Any())
            {
                return;
            }

            var pressd = buttons.FirstOrDefault(x => Regex.IsMatch(x.Name.ToLower(), param.buttonPressedKey));
            var highlighted = buttons.FirstOrDefault(x => Regex.IsMatch(x.Name.ToLower(), param.buttonHighlightedKye));
            var disabled = buttons.FirstOrDefault(x => Regex.IsMatch(x.Name.ToLower(), param.buttonDisabledKey));

            var normal = buttons.FirstOrDefault(x =>
            {
                return x != pressd && x != highlighted && x != disabled;
            });


            var component = new ButtonLayerComponent(
                groupName,
                path.Remove(path.Length - groupName.Length),
                normal.Rect,
                GetOrDefaultSprite(normal),
                GetOrDefaultSprite(pressd),
                GetOrDefaultSprite(highlighted),
                GetOrDefaultSprite(disabled));

            components.Add(component);

            foreach (var b in buttons)
            {
                layers.Remove(b);
            }
        }
    }
}
