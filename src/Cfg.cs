#if BEPINEX
using BepInEx.Configuration;

#elif MELONLOADER
using MelonLoader;

#endif

namespace BrittleHolds {
    public class Cfg {
#if BEPINEX
        public ConfigEntry<int> maxHp;

#elif MELONLOADER
        public MelonPreferences_Entry<int> maxHp;

#endif
    }
}
