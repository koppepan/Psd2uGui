using UnityEngine;

namespace Psd2uGui.Editor
{
    class ConvertParameter : ScriptableObject
    {
        public string textureSavePath = "Assets/GUI";
        public Font defaultFont = null;

        public string labelKey = "label_.*";

        [Header("button")]
        public string buttonKey = ".*button.*";
        public string buttonPressedKey = ".*pressed";
        public string buttonHighlightedKye = ".*highlighted";
        public string buttonDisabledKey = ".*disabled";

        [Header("toggle")]
        public string toggleKey = ".*toggle.*";
        public string toggleBackground = ".*toggle_background";
        public string toggleCheckmark = ".*toggle_check";
    }
}
