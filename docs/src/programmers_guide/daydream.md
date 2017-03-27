Google Daydream {#mechahamster_guide_daydream}
================

### Overview

MechaHamster features integration with [Google Daydream][], and can
be played in VR, using a Daydream headset.

### Activating VR Mode

To play the game in VR mode, launch the Daydream app, and launch
MechaHamster from the Daydream launcher.  MechaHamster checks on startup
to see if it is running in VR mode or not, and handles its initialization
accordingly.

Note:  Make sure that MechaHamster is not already running when you do this.
The mode-check is only performed when the game is launched.

Also note that some functions are disabled in VR mode, such as the level-editor.


### Code Locations

`Assets/Hamster/Scripts/VRSystemSetup.cs` contains the `VRSystemSetup` state,
which handles  most of the VR setup code.  On startup, the app checks if
it was launched from the VR launcher, and if so, it performs various startup
tasks differently.

The primary differences are:
* Whether a VR Camera prefab is added to the scene, to handle handles
  stereoscopic vision and undistortion.
* Whether a VR event system or regular event system is used.  The VR
  event system fires events from the VR Pointer to objects as clicks,
  allowing worldspace buttons and UI to function in VR!
* Whether or not to spawn a VR Controller prefab, to represent the
  controller's position to the user.


All VR prefabs are pulled from the [Google VR SDK for Unity][].

<br>

  [Google Daydream]: https://developers.google.com/vr/daydream/overview
  [Google VR SDK for Unity]: https://developers.google.com/vr/unity/