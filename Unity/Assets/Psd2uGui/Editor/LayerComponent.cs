using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PhotoshopFile;

namespace Psd2uGui.Editor
{
    enum ComponentType
    {
        Unknown,
        Image,
        Text,
        Button,
    };

    internal abstract class LayerComponent
    {
        public string Name
        {
            get;
            private set;
        }
        public string Path
        {
            get;
            private set;
        }
        public ComponentType ComponentType
        {
            get;
            private set;
        }
        public Rect Rect
        {
            get;
            protected set;
        }
        

        public LayerComponent(string name, string path, ComponentType type)
        {
            Name = name;
            Path = path;
            ComponentType = type;
        }
    }

    internal class TextLayerComponent : LayerComponent
    {
        Layer layer;

        public TextLayerInfo TextInfo
        {
            get
            {
                return (TextLayerInfo)layer.AdditionalInfo.FirstOrDefault(x => x is TextLayerInfo);
            }
        }

        public TextLayerComponent(string name, string path, Layer layer) : base(name, path, ComponentType.Text)
        {
            Rect = layer.Rect;
            this.layer = layer;
        }
    }

    internal class ImageLayerComponent : LayerComponent
    {
        public Layer Layer
        {
            get;
            private set;
        }

        public ImageLayerComponent(string name, string path, Layer layer) : base(name, path, ComponentType.Image)
        {
            Rect = layer.Rect;
            Layer = layer;
        }
    }

    internal class ButtonLayerComponent : LayerComponent
    {
        public Layer[] Layers
        {
            get;
            private set;
        }

        public ButtonLayerComponent(string name, string path, Layer[] layers) : base(name, path, ComponentType.Button)
        {
            Rect = layers.First().Rect;
            Layers = layers;
        }
    }
}
