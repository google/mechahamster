Gameplay {#mechahamster_guide_gameplay}
================


# Main Game

The main gameplay for MechaHamster is rolling a ball around through
a set of mazes, trying to get the best time.  This is a
straightforward maze game - the player controls the hamster ball by
tilting the device (or when playing in Daydream mode, by tilting the
VR controller) and wins when they reach the goal tile.  Top scores
are saved to the [Firebase Realtime Database][] on level completion.

The game is instrumented with various [Firebase Analytics][] events,
and has values that can be tweaked via [Firebase Remote Config][].

<img src="gameplay1.png" style="height: 20em"/>

# Editor Mode

The game also contains a full level editor, which can be used to create
new levels, save them to the cloud via the [Firebase Realtime Database][],
and share them with friends.  It has a tool panel on the left side,
from which the user can select tiles to place, and various menu options
on the top that allow the user to clear, load, save, and share their maps.

<img src="editor.png" style="height: 20em"/>

<br>

[Firebase Realtime Database]: @ref mechahamster_guide_database
[Firebase Analytics]: @ref mechahamster_guide_analytics
[Firebase Remote Config]: @ref mechahamster_guide_remoteconfig