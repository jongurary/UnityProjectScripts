using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants{

	public const int BUTTON_WIDTH_DIVISOR = 32; //screen.width divides by this number to get button width
	public const int BUTTON_HEIGHT_DIVISOR = 16; //screen.height divides by this number to get button height
	public const int CURSOR_SIZE_DIVISOR = 32; //master size constant for the cursor
	public const int FONT_SIZE_DIVISOR = 64; //master size for fonts

	public const int VOLTAGE_DEFAULT = 100;

	public const int MAX_LINKANIMATION_PARTICLES = 50; //max number of particles to spawn in link animations
	public const int MAX_LINKANIMATION_ANIMATEDWATTS = 2000; //beyond this link bandwidth, the link will merely spawn the max particles

	public const float MAX_INEFFICIENCY = .20f; //worse-case loss in a transmission wire over distance, as a percentage of total power transmitted

	public const int IGNORE_RAYCAST_LAYERMASK = ~((1 << 14)| (1<<2));


	//list of objects in the buildable list
	public const int radioscopicGenerator = 5;
	public const int solarGenerator = 6;
	public const int windGenerator = 7;
	public const int geoGenerator = 8;

	public const int nuclearReactor = 12;
	
	public const int transmissionLine = 15;
	public const int substation = 16;
	public const int capacitor = 17;
	public const int battery = 18;
	public const int wireless = 19;

	public const int shieldGenerator = 20;
	
	public const int gatlingTurret = 25;
	public const int flakTurret = 26;
	public const int cannonTurret = 27;
	public const int laserTurret = 30;
	public const int plasmaTurret = 31;

	public const int droneBay = 35;
	public const int missileLauncher = 39;
	public const int airbase = 40;
	
	public const int metalMine = 45;
	public const int metalRefine = 46;
	public const int uraniumEnricher = 47;
	public const int fuelSynth = 48;
	public const int orbitalLauncher = 50;
	public const int ammoSupplier = 51;
	
	public const int GENERIC_MAX_WIND_STRENGTH = 500; //the typical "max" value of wind for purposes of wind power generation

	public const int MISSILE_MAX_HEIGHT = 75; //how high missiles climb before coming back down

	//TODO potentially move these to a variable class, where they change by level
	public const float WIND_UPDATE_INTERVAL = 60f; //how often the wind changes
	public const float DUST_STORM_SPAWN_CHANCE = 2f; //odds from 0-100 of a dust storm spawning
	public const float DUST_STORM_END_CHANCE = 35f; //odds from 0-100 of a dust storm ending if already in progress

	public const float REFINERY_TICK_RATE = 1f;
	public const float AMMO_REFINE_TICK_RATE = 1f;

	public const float TEXT_DEFAULT_SCROLL_TIME=.022f; //Time between characters rendered to display, lower is faster
	public const float TEXT_DEFAULT_END_TIME = TEXT_DEFAULT_SCROLL_TIME * 15 + 1f; //Time text stays on screen after all text is displayed. Lower is faster

}
