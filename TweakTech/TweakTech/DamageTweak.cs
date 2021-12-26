using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakTech
{
    internal class DamageTweak
    {
        internal float Multiplier = 1;

        internal ManDamage.DamageType Dealer = ManDamage.DamageType.Standard;
        internal ManDamage.DamageableType Taker = ManDamage.DamageableType.Standard;
        // Can add new DamageTypes but not DamageableTypes
        //   DamageableTypes are handled by their own respective "ModuleReinforced"

    }
}
