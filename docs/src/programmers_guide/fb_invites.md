Firebase Invites {#mechahamster_guide_invites}
================

### Overview

MechaHamster uses the [Firebase Invites][] API to send
[Dynamic Links][] to other players, in the form of map invites.
They take the form of a custom URL that, when followed,
launches MechaHamster with a custom data payload.
(Or, if MechaHamster isn't installed, prompts
the user to install it.)


### Code Locations

When the game is in the Main Menu state,
(`Assets/Hamster/Scripts/States/MainMenu.cs`) the game registers
a listener for invites - if it either has an Invite event waiting,
or if one is fired while the game is running.

The listener is removed whenever the Main Menu state is exited
or suspended, but is reinstated whenever the user returns to
the main menu.  Since the listener will fire if there is a pending
event (that even if the event occured before the listener was
reigstered) this ensures that invite events are only handled while
the game is in a state that is prepared to deal with it.  (The main
menu, in MechaHamster's case.)

<br>

  [Firebase Invites]: https://firebase.google.com/docs/invites/
  [Dynamic Links]: https://firebase.google.com/docs/dynamic-links/
  [Firebase Console]: https://console.firebase.google.com/
