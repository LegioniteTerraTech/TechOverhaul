using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakTech
{
    public enum DamageTypesExt
	{   //
		Standard,
		Bullet,
		Energy,
		Explosive,
		Impact,
		Fire,
		Cutting,
		Plasma,
		//(NEW)
	    Cyro,       
	    EMP,         
	}
	/* OG
	 * Standard,    - meh jack of all trades
	 * Bullet,      - Shield Counter
	 * Energy,      - Armor Counter
	 * Explosive,   - Ranged Universal Tech punisher
	 * Impact,      - Expensive Block Counter
	 * Fire,        - DoT dealer
	 * Cutting,     - Melee Light Tech punisher
	 * Plasma,      - Slow Tech punisher
	 * (NEW)
	 * Cyro,        - Same as Energy
	 * EMP,         - Specialized shield destroyer
	 * 
	 * CHANGES
	 * Standard,    - meh jack of all trades    (Changed to Bullet)
	 * Bullet,      - Shield/Slow/Ground Counter(General-Purpose)
	 * Energy,      - Armor/Fast/Air Counter    (Raider Weapon)
	 * Explosive,   - Slow/Base Tech Punisher   (Artillery (Slow))
	 * Impact,      - Melee Splash damage dealer(Push Effects)
	 * Fire,        - Expensive Tech Counter    (Damage Stacking)               [GRADIENT ORANGE OUTLINE]
	 * Cutting,     - Tanky Tech punisher       (Weakens block fragilities)     [SOLID RED OUTLINE]
	 * Plasma,      - Super Tech Punisher       (Damage dealt by PERCENT)
	 * (NEW)
	 * Cyro,        - Fast Tech punisher        (Slows hit blocks up to 50%)    [SOLID BLUE OUTLINE]
	 * EMP,         - Heavy Shield Spam Counter (Disables at half BLOCK health) [FLASHING YELLOW OUTLINE]
	 */
}
