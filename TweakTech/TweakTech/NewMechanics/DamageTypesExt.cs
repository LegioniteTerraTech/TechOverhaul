﻿using System;
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
	    Cryo,       
	    EMP,
		Scramble,
		Hack,
		Acid,
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
	 * Cryo,        - Same as Energy
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
	 * Cryo,        - Fast Tech punisher        (Slows hit blocks up to 50%)    [SOLID BLUE OUTLINE]
	 * EMP,         - Heavy Shield Spam Counter (Disables at half BLOCK health) [FLASHING YELLOW OUTLINE]
	 * Scramble,	- SuperHeavy Weapon Counter (Only the first strike is really powerful, but enough to mess over high-end weapons) [Flashing White]
	 * Hack,		- Block detachment method   (Only works against blocks with modules in them) [DARK GREEN]
	 */
}
