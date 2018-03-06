using UnityEngine;

namespace Psd2uGui.Editor
{
    class ConvertParameter : ScriptableObject
    {
        public string textureSavePath = "Assets/GUI";
        public Font defaultFont = null;

        public string buttonKey = ".*button.*";
        public string buttonPressedKey = ".*pressed";
        public string buttonHighlightedKye = ".*highlighted";
        public string buttonDisabledKey = ".*disabled";

        public string labelKey = "label_.*";
    }
}
