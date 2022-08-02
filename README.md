<h2>Overview</h2>

This repository contains several in-game scripts for the Space Engineers video game, as well as several mixins that are common to several scripts.

<h2>Scripts</h2>

<h3>ImprovedRotorThrusterScript</h3>

This script offers standard vector-thrust functionality: when building a ship, simply mount thrusters on rotors and the script will automatically control the rotors and the thrusters based on ship controller input. This script offers several advantages over many other vector thrust scripts available on the steam workshop:
1. The script remembers what mode it was in when saving and loading. Other available vector thrust scripts revert to standby mode (turning off all thrusters) when the game is saved and reloaded, which is obviously disastrous when mid flight.
2. The script attempts to maximize thrust, rather than trying to achieve a hardcoded constant acceleration. This means if you build a ship with lots of excess thrust, you'll be able to utilize all the thrust with this script. Other vector thrust scripts I've encountered have tried to achieve a certain constant acceleration (in one case, based on the planetary gravity the ship was operating in!) meaning you don't always utilize all available thrust.
3. This script does not detect thrusters through connectors, meaning it is safe to use even if your ship is docked to a station with other vector thrust-enabled ships attached.

<h4>Usage</h4>

Simply place a programmable block on your ship with this script. To set the flight mode of the script, run it with one of the following arguments:

%park: By default, the script will be in "park" mode. In this mode, the thrusters turn off and the rotors are locked.<br>
%hover: In "hover" mode, the ship will operate with full inertial dampeners.<br>
%cruise: In "cruise" mode, the ship will attempt to maintain a constant forward speed.<br>
%drift: In "drift" mode, the ship will attempt to maintain constant motion in whatever direction it was moving in when the flight control input was ceased.<br>


<h3>MissileDetonatorScript</h3>

This simple script automatically detonates all warheads on a spacecraft when it detects a sudden deceleration.
While designing a large ICBM missile to play in my world with the assertive drones mod, I found that it was difficult to reliably detonate warheads using sensors. One issue with sensors is that they detect voxels from really far out, making them unreliable for taking out stations that are embedded in voxels. Also, setting the correct sensor range was tricky. This script solves this problem by detonating the warheads when a sudden deceleration is detected: I've found this to be much more reliable than using sensors.

<h4>Usage</h4>

Simply place a programmable block on your ICBM with this script loaded. The script will automatically detonate all warheads on the spacecraft when a sharp deceleration is detected. 

You can arm and disarm the script by running it with the %arm and %disarm commands, respectively. By default, the script starts in the "armed" state. This can be changed by setting the "StartArmed" constant.

You can adjust the threshhold at which the missile detonates by setting the "TriggerThreshhold" constant.

Warning! Do NOT turn the programmable block off and on; this can lead to it erroneosly detecting a false high deceleration, which could lead to an explosion! Use the %arm and %disarm commands to enable or disable the script, respectively.

<h2>Mixins</h2>

<h3>RxSpaceEngineers</h3>
This mixin is a (vastly stripped-down) version of .NET's rx observables. This is required because the IObservable&ltT&gt and IObserver&ltT&gt interfaces are not on the Space Engineers whitelist, and therefore cannot be used by in-game scripts.

<h3>Algorithms</h3>
This mixin contains a simplified implementation of the Simplex algorithm for solving linear equations. The implementation has been optimized to avoid creating excess garbage, which is important for in-game script performance. 
