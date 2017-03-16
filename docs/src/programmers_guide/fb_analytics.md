Firebase Analytics {#mechahamster_guide_analytics}
================

### Overview

MechaHamster makes use of [Firebase Analytics][] to track user
events and flow.  You can follow along and see what users
have been doing recently in the game, using the [Firebase Console][]


### Code Locations

Initialization for Firebase Analytics is handled in
`Hamster/Scripts/MainGame.cs`, during startup.

While the game is running, the
`Firebase.Analytics.FirebaseAnalytics.LogEvent()` function is invoked
at various points, to log an analytics event when various game events
occur.  (Saving maps, starting levels, uploading times, etc.)

### Viewing in the Console

From the [Firebase Console][], select your Firebase project, and click
"Analytics" from the menu on the left.

Normally Analytics messages are batched to save bandwidth, and may not
update for several hours.  During development though, [Analytics DebugView][]
can be enabled to allow realtime analytics feedback, which can be extremely
useful in debugging.


<br>

  [Firebase Analytics]: https://firebase.google.com/docs/analytics/
  [Firebase Console]: https://console.firebase.google.com/
  [Analytics DebugView]: https://support.google.com/firebase/answer/7201382?hl=en&utm_id=ad