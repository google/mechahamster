Firebase Cloud Messaging {#mechahamster_guide_messaging}
================

### Overview

MechaHamster uses [Firebase Cloud Messaging][] to receive messages
from the publisher of the app.  (You!)  Notifications show up
on the alert bar of any phone with Firebase, and when clicked,
launch MechaHamster, and provide it with a custom payload
of data.

In MechaHamster, these are used to send new maps to the player,
after the app has been launched.  Notifications add a new
entry to the player's bonus_maps field in the
[Firebase Realtime Database][], which allow them to access
new maps.

### Code Locations

Notifications are handled via a listener in the Main Menu state,
(`Assets/Hamster/Scripts/States/MainMenu.cs`).  The listener
is registered whenever the MainMenu state is active, and unregistered
whenever it is exited or suspended.  Since the listener will fire
if there is a pending event (that fired while there was no listener)
this ensures that notification events are only handled while
the game is in a state that is prepared to deal with it.
(The main menu, in MechaHamster's case.)

### Viewing in the Console

From the [Firebase Console][], select your Firebase project, and click
"Notifications" from the menu on the left.  This gives you a list of
any notifications that have been sent, and gives you the option
to duplicate them and resend existing ones without resending
the same data.

To send a notification for MechaHamster, make sure to give it the
following custom data fields:

| Field name | Data |
|------------|------|
| `type` | `bonus_map` |
| `map_id` | *A bonus map ID* |
| `map_name` | *The name for the map to appear in menu* |

The value for `map_id` needs to be the map_id for an entry in the
[Firebase Realtime Database][] table "Bonus Maps".  The easiest
way to populate this table is to run MechaHamster through the
Unity editor, launch the level editor, and save a map using
the developer button "Bonus Map".  (Only visible while running
through the editor - this is a debug button intended to make it
easy to author bonus maps.)

If type is anything other than `bonus_map`, then the notification
will still be received, but MechaHamster will throw it out because
it won't know how to parse the data.

(See `Assets\Hamster\Scripts\States\MessageReceived.cs` for the
message handling code.)

<br>

  [Firebase Cloud Messaging]: https://firebase.google.com/docs/cloud-messaging/
  [Firebase Console]: https://console.firebase.google.com/
  [Analytics DebugView]: https://support.google.com/firebase/answer/7201382?hl=en&utm_id=ad
  [Firebase Realtime Database]: @ref mechahamster_guide_database