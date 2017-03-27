Firebase Authentication {#mechahamster_guide_auth}
================

### Overview

MechaHamster uses [Firebase Auth][] to manage user identites, and
tie persistent user data to accounts.  Uses Firebase to assign
unique userIDs (tied to accounts), which can be used by the
[Firebase Realtime Database][] as a unique key for user data.


### Code Locations


When the game is launched, the Startup state
(`Scripts/Hamster/States/Startup.cs`) checks to see if the user
is currently signed in to a [Firebase Auth][] account.  (Sign-ins
persist across playsessions, so if they signed in once in the past,
they will still be signed in next time the app is launched.)

If the user is signed in, then user data is retrieved, based on
their Auth UserId.

If not, control switches over to a menu state
(`Scripts/Hamster/States/ChooseSignInMenu.cs`) to allow the user to
select a signin method.  (Or, if in VR mode, it defaults to an
anonymous signin)

### Viewing in the Console

From the [Firebase Console][], select your Firebase project, and click
"Authentication" from the menu on the left.  From here, you can view
all of the current users that have registered with your app, and the
authentication method they provided.  (email, anonymous, etc.)

<br>

  [Firebase Auth]: https://firebase.google.com/docs/auth/
  [Firebase Console]: https://console.firebase.google.com/
  [Analytics DebugView]: https://support.google.com/firebase/answer/7201382?hl=en&utm_id=ad
  [Firebase Realtime Database]: @ref mechahamster_guide_database