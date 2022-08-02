Overview

This repository contains several in-game scripts for the Space Engineers video game, as well as several mixins that are common to several scripts.

Scripts

ImprovedRotorThrusterScript

This script offers standard vector-thrust functionality: when building a ship, simply mount thrusters on rotors and the script will automatically control the rotors and the thrusters based on ship controller input. This script offers several advantages over many other vector thrust scripts available on the steam workshop:
1. The script remembers what mode it was in when saving and loading. Other available vector thrust scripts revert to standby mode (turning off all thrusters) when the game is saved and reloaded, which is obviously disastrous when mid flight.
2. The script attempts to maximize thrust, rather than trying to achieve a hardcoded constant acceleration. This means if you build a ship with lots of excess thrust, you'll be able to utilize all the thrust with this script. Other vector thrust scripts I've encountered have tried to achieve a certain constant acceleration (in one case, based on the planetary gravity the ship was operating in!) meaning you don't always utilize all available thrust.
3. This script does not detect thrusters through connectors, meaning it is safe to use even if your ship is docked to a station with other vector thrust-enabled ships attached.

Usage

Simply place a programmable block on your ship with this script. To set the flight mode of the script, run it with one of the following arguments:

%park: By default, the script will be in "park" mode. In this mode, the thrusters turn off and the rotors are locked.
%hover: In "hover" mode, the ship will operate with full inertial dampeners.
%cruise: In "cruise" mode, the ship will attempt to maintain a constant forward speed.
%drift: In "drift" mode, the ship will attempt to maintain constant motion in whatever direction it was moving in when the flight control input was ceased.

