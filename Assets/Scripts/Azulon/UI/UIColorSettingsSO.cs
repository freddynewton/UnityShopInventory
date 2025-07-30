using Azulon.Data;
using UnityEngine;

namespace Azulon.UI
{
    [CreateAssetMenu(fileName = "UIColorSettings", menuName = "Azulon/UI/Color Settings", order = 1)]
    public class UIColorSettingsSO : ScriptableObject
    {
        [Header("Core UI Colors")]
        public Color primaryColor = Color.white;
        public Color accentColor = Color.gray;
        public Color disabledColor = Color.gray3;
        public Color errorColor = Color.darkRed;
    }
}
