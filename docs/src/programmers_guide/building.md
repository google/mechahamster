Building MechaHamster {#mechahamster_guide_building}
================

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


### Setting Up a Firebase Project {#mechahamster_guide_firebase_setup}

In addition to importing the Firebase Unity SDK packages
you'll also need to create a a project in the [Firebase Console][], and
download the files necessary to link it to MechaHamster:

1. Navigate to the [Firebase Console][].
2. If you already have an existing Google project associated with your mobile app, click **Import Google Project.** Otherwise, click **Create New Project.**
3. Select the target platform (Android or iOS) and follow the setup steps. If you're importing an existing Google project, this may happen automatically and you can just download the config file.
4.  When prompted, enter the app's package name.  (`com.google.fpl.mechahamster`)
5. At the end, depending on your platform, you'll download a file named `google-services.json` (android) or `GoogleService-Info.plist` (iOS).  Put this file somewhere in your `/Assets` directory.  (You can re-download this file again at any time from the [Firebase Console][].)



<br>

  [Firebase]: https://firebase.google.com/docs/
  [Daydream]: https://developers.google.com/vr/daydream/overview
  [Google Daydream]: https://developers.google.com/vr/daydream/overview
  [Google VR SDK for Unity]: https://developers.google.com/vr/unity/
  [MechaHamster]: @ref #mechahamster_index
  [Firebase Unity SDK]: https://firebase.google.com/docs/unity/setup
  [Firebase Console]: https://console.firebase.google.com/
