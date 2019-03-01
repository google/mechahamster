Firebase Crashlytics {#mechahamster_guide_crashlytics}
================

### Overview

MechaHamster uses [Firebase Crashlytics (Beta)][] to capture in
app crashes, so that developers can more easily diagnose issues
and prioritize fixes.

### Code Locations


When the game is launched, the MainGame.StartGame state
(`Assets/Hamster/Scripts/MainGame.cs`) initializes Crashltyics
by accessing FirebaseApp.DefaultInstance. The instantiation of the
DefaultInstance will automatically start the platform level SDK in
Android or iOS.

One can manually trigger a crash in the game by navigating to the
SettingsMenu and clicking the `Self Destruct!` button three times. This will
throw one of six exceptions and exercise many of the Crashlytics APIs
like logging a message, capturing the user's id (if logged in), and
adding a custom key/value pair for context.

The code for the SelfDestruct functionality lives largely in:
    * the MeltdownMenu
(`Assets/Hamster/Scripts/States/MeltdownMenu.cs`)
    * the SelfDestructMenu
(`Assets/Hamster/Scripts/States/SelfDestructMenu.cs`)
    * the PseudoRandomExceptionChooser
(`Assets/Hamster/Scripts/States/Exceptions/PseudoRandomExceptionChooser.cs`).

To learn more about all of the available Crashlytics APIs check out the [Firebase Crashlytics API][]
.

### Viewing in the Console

From the [Firebase Console][], select your Firebase project, and click
"Crashlytics" from the menu on the left.  From here, you can view
all of the current issues that have been experienced by mechahamster users.

<br>

  [Firebase Crashlytics (Beta)]: https://firebase.google.com/docs/crashlytics/
  [Firebase Crashlytics API]: https://firebase.google.com/docs/reference/unity/class/firebase/crashlytics/crashlytics
  [Firebase Console]: https://console.firebase.google.com/