Building MechaHamster {#mechahamster_guide_building}
================

### Downloading Source Code
Source code for MechaHamster is available for download from [Github.][]

> If cloning locally using `git clone`, be sure to use the `--recurse-submodules` flag
> to ensure required scripts from submodules are present.

### Overview

The MechaHamster project was built using version 2017.2.1p3 of the Unity
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
| Firebase Crashlytics (Beta) | FirebaseCrashlytics.unitypackage |
| Firebase Database | FirebaseDatabase.unitypackage |
| Firebase Messaging | FirebaseMessaging.unitypackage |
| Firebase Remote Config | FirebaseRemoteConfig.unitypackage |
| Firebase Cloud Storage | FirebaseStorage.unitypackage |

> [MechaHamster][] currently only works with .NET 3.x. If [Firebase Unity SDK][] version is 5.4.0 or
> above, please import plugins from `dotnet3` folder. And make sure `Scripting Runtime Version` in
> `Edit > Project Settings > Player` is set to .NET 3.x, ex. `Stable (.NET 3.5 Equivalent)` in Unity
> 2017

### Downloading Firebase Files

In addition to importing the Firebase Unity SDK packages
you'll also need to create a a project in the [Firebase Console][], and
download the files necessary to link it to MechaHamster:

1. Navigate to the [Firebase Console][].
2. If you already have an existing Google project associated with your mobile app, click **Import Google Project.** Otherwise, click **Create New Project.**
3. Select the target platform (Android or iOS) and follow the setup steps. If you're importing an existing Google project, this may happen automatically and you can just download the config file.
4. When prompted, enter the app's package name.  (`com.google.fpl.mechahamster` for Android and `com.google.fpl.mechahamster.dev` for iOS since the former iOS bundle ID was taken)
5. At the end, depending on your platform, you'll download a file named `google-services.json` (android) or `GoogleService-Info.plist` (iOS).  Put this file somewhere in your `/Assets` directory.  (You can re-download this file again at any time from the [Firebase Console][].)


### Setting Project Properties and Permissions

#### (Android Only) Add your signing key's SHA-1 to the project

In order to make use of Firebase Authentication,
you will need to calculate a SHA-1 hash from your signing key, and
enter it into the Firebase console.

Note:  This is only necessary when building for Android devices.  You do not need to enter the SHA-1 for iOS builds.

1. Navigate to the [Firebase Console][].
2. In the upper left, click on the gear, and select 'Project Settings'
3. Enter your signing certificate's SHA-1 in the indicated field.  (Instructions for
calculating your certificate's fingerprint can be found
[here](https://developers.google.com/android/guides/client-auth))
4. After the change, you need to re-download `google-services.json` again.  You can find the
download button in the same page.  Then replace the one you stored in your `/Assets` directory.


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

> 'Push Notifications' requires a paid Apple developer account. More information can be found
> in [Configuring APNs with FCM][] page. However, the game can still run without it.

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
3. Replace the rules text with the code in [database.rules.json][] under `/console` directory.
4. Alternatively, use [Firebase CLI][] to upload `database.rules.json` file under `/console`
   directory to your Firebase project, which will be detailed below.

#### Update Firebase Database url in the script

MechaHamster can access Firebase Realtime Database in the Unity Editor.  Set Database url to your
project's database url using the SetEditorDatabaseUrl() function.

1. Open `/Assets/Hamster/Scripts/MainGame.cs` in the text editor.
2. Find the line with `SetEditorDatabaseUrl`.  For instance,
~~~~
    void StartGame() {
      ...
      CommonData.app.SetEditorDatabaseUrl("https://YOUR-PROJECT-ID.firebaseio.com/");
      ...
    }
~~~~
3. Change the url to "https://YOUR-PROJECT-ID.firebaseio.com/".  "YOUR-PROJECT-ID" is your
   project ID which you can find from the [Firebase Console][] .  You can also find the url in
   `google-services.json` (android) or `GoogleService-Info.plist` (iOS).
4. Save the script and rebuild with the Unity Editor.

#### (Optional) Deploy Firebase Project using Firebase CLI Tool

MechaHamster requires the Firebase project to be configured in a specific way to run properly.
[Firebase CLI][] allows the developers to deploy or update configurations, such as rules and Cloud
Functions, to your Firebase project with few console commands.

1. Follow the instructions in [Firebase CLI][] page for installation.
2. Open the console terminal and navigate to `/console` directory in the project.
3. If you have multiple Firebase projects under the same account, make sure to select the correct
   active project by running:
~~~~
    $ firebase use --add
~~~~
4. Initialize the project by running:
~~~~
    $ firebase init
~~~~
  * Select only `Functions` when it prompts for `Which Firebase CLI features do you want to setup
    for this folder?`.
  * Press `Enter` until the initialization completes.
5. Deploy configurations by running:
~~~~
    $ firebase deploy
~~~~
   This would deploy the following to your active project.
  * Firebase Realtime Database Rules
  * Firebase Storage Rules
  * Cloud Function to limit number of scores in Database and remove unreferenced replay data from
    Storage.

#### (Optional) Enable Replay feature

Replay in MechaHamster is an experimental feature to capture and upload the play-through of the
best score in each level so that other player can download and view the animation later.  This is
still a work in progress and thus disabled by default.  To enable it, please follow the instructions
below.

1. Open `/Assets/Hamster/Scripts/MainGame.cs` in the text editor.
2. Find the line with `RemoteConfigGameplayRecordingEnabled`.  For instance,
~~~~
    System.Threading.Tasks.Task InitializeRemoteConfig() {
      ...
      defaults.Add(StringConstants.RemoteConfigGameplayRecordingEnabled, false);
      ...
    }
~~~~
3. Change the default value to `true`.
4. Save the script and rebuild with the Unity Editor.

<br>

  [Firebase]: https://firebase.google.com/docs/
  [Daydream]: https://developers.google.com/vr/daydream/overview
  [Google Daydream]: https://developers.google.com/vr/daydream/overview
  [Google VR SDK for Unity]: https://developers.google.com/vr/unity/
  [MechaHamster]: #mechahamster_index
  [Firebase Unity SDK]: https://firebase.google.com/docs/unity/setup
  [Firebase Console]: https://console.firebase.google.com/
  [GitHub.]: https://github.com/google/mechahamster
  [Configuring APNs with FCM]: https://firebase.google.com/docs/cloud-messaging/ios/certs
  [database.rules.json]: https://github.com/google/mechahamster/tree/master/console/database.rules.json
  [Firebase CLI]: https://firebase.google.com/docs/cli/
