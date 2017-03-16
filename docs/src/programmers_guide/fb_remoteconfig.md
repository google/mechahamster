Firebase Remote Config {#mechahamster_guide_remoteconfig}
================

### Overview

MechaHamster implements the [Firebase Remote Config][] API to
allow for changes to program data after launch.  Values are
registered and stored on the Firebase Server, but can be
changed through the [Firebase Console][], and published
changes will be reflected next time the app is launched.


### Code Locations

In `Assets/Hamster/Scripts/MainGame.cs`, remotely configurable
values are assigned defaults on startup, and current values are
downloaded from the server via `FirebaseRemoteConfig.FetchAsync()`.

At runtime, scattered throughout the code, various values are
retrieved via `FirebaseRemoteConfig.GetValue()` calls.

### Viewing in the Console

From the [Firebase Console][], select your Firebase project, and click
"Remote Config" from the menu on the left.  This gives a list of all
values that have been set up to be remotely configurable.

If you haven't set up any values yet, this list will be empty.

Here is a list of values that the game checks, as well as their
default values:


| Parameter Key              | Value |                                   |
|----------------------------|-------|-----------------------------------|
| `physics_gravity`          |  -20  | *The force of gravity in the game.* |
| `acceleration_tile_force`  | 24    | *How fast acceleration-tiles push the player.* |
| `sand_tile_drag`           | 5     | *The ammount of drag applied from quicksand tiles.* |
| `jump_tile_velocity`       | 8     | *The vertical force applied by jump tiles.* |
| `mine_tile_force`          | 10    | *The force applied by exploding mines.* |
| `mine_tile_radius`         | 2     | *The explosion radius for exploding mines.* |
| `mine_tile_upwards_mod`    | 0.2   | *The `upwardsModifier` on [AddExplosionForce][] calls triggered by mines.* |
| `spikes_tile_force`        | 10    | *The force applied when colliding with spikes.* |
| `spikes_tile_radius`       | 1     | *The radius of the spike trigger zone.* |
| `spikes_tile_upwards_mod`  | -0.5  | *The `upwardsModifier` on [AddExplosionForce][] calls triggered by spikes.* |
| `VR_height_scale`          | 0.68  | *The vertical position of the camera, in VR mode.* |
 

Any keys that are unset will simply use the default values set in 
`Assets/Hamster/Scripts/MainGame.cs`

<br>

  [Firebase Remote Config]: https://firebase.google.com/docs/remote-config/
  [Firebase Console]: https://console.firebase.google.com/
  [Analytics DebugView]: https://support.google.com/firebase/answer/7201382?hl=en&utm_id=ad
  [Firebase Realtime Database]: @ref mechahamster_guide_database
  [AddExplosionForce]: https://docs.unity3d.com/ScriptReference/Rigidbody.AddExplosionForce.html