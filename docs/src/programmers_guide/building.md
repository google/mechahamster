Building MechaHamster {#mechahamster_guide_building}
================

### Downloading Source Code
Source code for MechaHamster is available for download from [Github.][]

### Overview

The MechaHamster project was built using version 5.6.06b of the Unity
Editor.  Opening it with an older version of the editor may encounter
errors.

When you first open MechaHamster in Unity, you will see a large number
of errors.  MechaHamster depends on several external packages that need
to be imported before it can be run.

### Firebase Build Setup

You will need to download the [Firebase Unity SDK][], and import the following
packages into the Unity project:

| Package | File Name |
|---------|-----------|
| Firebase Analytics | FirebaseAnalytics.unitypackage |
| Firebase Auth | FirebaseAuth.unitypackage |
| Firebase Database | FirebaseDatabase.unitypackage |
| Firebase Invites | FirebaseInvites.unitypackage |
| Firebase Messaging | FirebaseMessaging.unitypackage |
| Firebase Remote Config | FirebaseRemoteConfig.unitypackage |


### Downloading Firebase Files

In addition to importing the Firebase Unity SDK packages
you'll also need to create a a project in the [Firebase Console][], and
download the files necessary to link it to MechaHamster:

1. Navigate to the [Firebase Console][].
2. If you already have an existing Google project associated with your mobile app, click **Import Google Project.** Otherwise, click **Create New Project.**
3. Select the target platform (Android or iOS) and follow the setup steps. If you're importing an existing Google project, this may happen automatically and you can just download the config file.
4. When prompted, enter the app's package name.  (`com.google.fpl.mechahamster`)
5. At the end, depending on your platform, you'll download a file named `google-services.json` (android) or `GoogleService-Info.plist` (iOS).  Put this file somewhere in your `/Assets` directory.  (You can re-download this file again at any time from the [Firebase Console][].)


### Setting Project Properties and Permissions

#### (Android Only) Add your signing key's SHA-1 to the project

In order to make use of several Firebase features, (Authentication and App Invites)
you will need to calculate a SHA-1 hash from your signing key, and
enter it into the Firebase console.

Note:  This is only necessary when building for Android devices.  You do not need to enter the SHA-1 for iOS builds.

1. Navigate to the [Firebase Console][].
2. In the upper left, click on the gear, and select 'Project Settings'
3. Enter your signing certificate's SHA-1 in the indicated field.  (Instructions for
calculating your certificate's fingerprint can be found
[here](https://developers.google.com/android/guides/client-auth))


#### (iOS Only) Set App Capabilities

MechaHamster requires several capabilities to be enabled in XCode
in order to function on iOS devices.


1. After doing an iOS build in Unity, open the resulting project in XCode.
2. Go to the project's settings, click the 'Capabilities' tab.
3. Enable 'Keychain Sharing'.
4. Enable 'Push Notifications'.
5. Enable 'Associated Domains'.
6. Expand 'Associated Domains' and add "applinks:YOUR_DYNAMIC_LINKS_DOMAIN"
to the list.  You can find your project's Dynamic Links domain under
the Dynamic Links tab of the [Firebase console][].



#### Enable Login Methods

Firebase Auth provides several methods by which a user can sign in and
access their data.  MechaHamster supports several of these, but they must
be enabled in the Firebase console before they can be used.  (They are all
disabled by default.)

1. Navigate to the [Firebase Console][], and select 'Authentication' from the side panel.
2. Click on the 'Sign-In Method' tab.
3. Select "Anonymous" from the list, enable it, and click 'Save'.
4. Select "Email/Password" from the list, enable it, and click 'Save'.


#### Set up the database access rules

MechaHamster depends heavily on the Firebase Realtime Database for storage.  In order to run
Mechahamster, you'll need set up the database access rules.

1. Navigate to the [Firebase Console][], and select 'Database' from the side panel.
2. Click on the 'Rules' tab.
3. Replace the rules text with the following code:

~~~~
{
  "rules": {
    // Bonus maps cannot be written to under normal circumstances.
    "BonusMaps":  {
      ".read": true,
      ".write": false
    },
    // The DB_Users table contains all of the data for users.  It can only be read
    // or written to if your auth ID matches the key you are trying to access.
    "DB_Users":  {
      "$uid": {
        ".read": "(auth != null && $uid == auth.uid) || $uid == 'XYZZY'",
        ".write": "(auth != null && $uid == auth.uid) || $uid == 'XYZZY'"
      }
    },
    // Entries in the maplist can only be read if they are shared, or
    // they are being modified by their owner.
    "MapList": {
      "$mapid": {
        ".read": "data.child('isShared').val() == true || ((auth != null && data.child('ownerId').val() == auth.uid) || data.child('ownerId').val() == 'XYZZY')",
        ".write": "!data.exists() || (auth != null && data.child('ownerId').val() == auth.uid) || data.child('ownerId').val() == 'XYZZY'"
      }
    },
    // High score tables for the offline maps.
    "OfflineMaps": {
      "$mapid": {
        "Times": {
          ".read": true,
          ".write": true,
          "$rank": {
            ".validate": "newData.child('name').isString() && newData.child('time').isNumber() && newData.child('name').val().length < 100"
          }
        }
      }
    }
  }
}
~~~~

(MechaHamster uses the name 'XYZZY' to represent desktop users,
since Firebase Authentication only functions on mobile devices.)

<br>

  [Firebase]: https://firebase.google.com/docs/
  [Daydream]: https://developers.google.com/vr/daydream/overview
  [Google Daydream]: https://developers.google.com/vr/daydream/overview
  [Google VR SDK for Unity]: https://developers.google.com/vr/unity/
  [MechaHamster]: @ref #mechahamster_index
  [Firebase Unity SDK]: https://firebase.google.com/docs/unity/setup
  [Firebase Console]: https://console.firebase.google.com/
  [GitHub.]: https://github.com/google/mechahamster
