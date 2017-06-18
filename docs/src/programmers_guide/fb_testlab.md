Firebase Test Lab {#mechahamster_guide_testlab}
================

### Overview

MechaHamster integrates [Firebase Test Lab For Game Loop][] to allow
automated testing on Android.  New .apk files are uploaded to
the Firebase Test Console and are automatically tested
on an array of physical devices.

Firebase provides support for several different types
of testing.  MechaHamster implements a "Game-Loop" scenario,
which means that it has a custom code path that will be run on
hardware during tests.

### Code Explanation:

When the app is launched through a
`com.google.intent.action.TEST_LOOP` intent, it follows a
slightly modified program flow.  Instead of presenting the
user with login options or a main menu, it skips past most
of the UI, and jumps straight into a game level.  It then
plays through the level using prerecorded game data, and when
the level is complete, it exits the app.  (Signaling the test
framework that the test is complete.)

The Firebase Test Lab lets us easily run this test on multiple
hardware devices and provides log output, framerate
data, and other useful metrics.  This gives an easy way to
checking build quality and quickly identifying errors, crashes,
or other problems across multiple hardware profiles.

The code to handle automatic testing is found in several
places throughout MechaHamster:

* `Plugins/Android/AndroidManifest.xml` - Contains the necessary
additions to the manifest for the app to listen for and respond
to the `com.google.intent.action.TEST_LOOP` intent.

* `Assets/Hamster/Scripts/States/Startup.cs` - Checks to see if
the app is running inside a testlab scenario, and if so,
skips the menus and jumps straight into a game loop.

* `Assets/FirebaseTestLab/AndroidTestLabManager.cs` - The actual logic
for running the test loops, as well as handling the reading
and writing of log files.


### Running the tests:

First, build an .apk for mechahamster, by running an android build.

From the [Firebase Console][], select Test Lab from the left,
and "Run your first test."  Select "Game Loop", and upload your .apk.
Select one or more devices to run the tests on, and when the test
results are complete, the results will be emailed to
whichever account you have associated with your firebase project.

<br>

  [Firebase Test Lab For Game Loop]: https://firebase.google.com/docs/test-lab/game-loop
  [Firebase Console]: https://console.firebase.google.com/

