Firebase Realtime Database {#mechahamster_guide_database}
================

### Overview

The [Firebase Realtime Database][] is used to manage persistent
data in MechaHamster.  This includes:

* User profile data
* Saved maps
* Bonus maps


MechaHamster uses [Firebase Auth][] to manage user identites, and
tie persistent user data to accounts.  Uses Firebase to assign
unique userIDs (tied to accounts), which can be used by the
[Firebase Realtime Database][] as a unique key for user data.


### Code Locations

Most of the Database code is abstracted through the class
`Assets/Hamster/Scripts/Database/DBStruct.cs` - it represents
a struct that is in communication with the database, and needs
to be kept in synch, and occasionally pushed to the remote
server.

It's a template class, and accepts most simple structures (composed
of basic types), and handles serialization and deserialization
as they are sent to and from the database.

Note that due to some quirks of the Unity json serializer, native
dictionaries do not serialize correctly.  MechaHamster works around
this with the SerializableDict class, located in
`Assets/Hamster/Scripts/LevelMap.cs`.  The class also includes a full
description of the problem (and workarounds) in the comments.

### Viewing in the Console

From the [Firebase Console][], select your Firebase project, and click
"Database" from the menu on the left.  This brings you to the database
explorer.  From here, you can view all of the entries in the database.
It also updates in real time, so changes made while playing the game
will be reflected in the view.

The most interesting datasets are the `MapList` and `DB_Users` nodes.
`MapList` contains a list of every user-created map that has been
saved to the database.

`DB_Users` contains the user data based on every user that has made an
account (including anonymous sign-ins) and played the game.  This is
probably some of the most human-readable data - it is just a serialized
version of the `UserData` struct, found in `/Assets/Hamster/Scripts/UserData.cs`.

<br>

  [Firebase Auth]: https://firebase.google.com/docs/auth/
  [Firebase Console]: https://console.firebase.google.com/
  [Firebase Realtime Database]: https://firebase.google.com/docs/database/
  [Analytics DebugView]: https://support.google.com/firebase/answer/7201382?hl=en&utm_id=ad