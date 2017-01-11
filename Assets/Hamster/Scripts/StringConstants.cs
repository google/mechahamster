using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hamster {

  // String constants used by MechaHamster.
  class StringConstants {

    // Names for Buttons:
    public const string ButtonSave = "Save";
    public const string ButtonLoad = "Load";
    public const string ButtonClear = "Clear";
    public const string ButtonPlay = "Play";
    public const string ButtonMainMenu = "Main Menu";
    public const string ButtonEditor = "Editor";
    public const string ButtonInvite = "Invite";
    public const string ButtonCancel = "Cancel";
    public const string ButtonMenu = "Menu";

    // Title screen text:
    public const string TitleText = "MechaHamster!";
    public const string SubTitleText = "The thrilling adventures of Col. Hammy D. Hamster!";

    // Save/Load screen text:
    public const string LabelSaveMap = "Save Map:";
    public const string LabelLoadMap = "Load Map:";
    public const string DefaultMapName = "Unnamed Map";
    public const string DefaultMapId = "<<default mapid>>";
    public const string ButtonSaveInNew = "Save as New Map";
    public const string ButtonSaveUpdate = "Save Map";
    public const string LabelName = "Name:";
    public const string LabelOverwrite = "Overwrite an existing map:";

    // Remote Config constants:
    public const string RemoteConfigPhysicsGravity = "physics_gravity";

    // Analytics tags:

    // Called when a level is started.
    // Properties:
    // AnalyticsParamMapId : string representing the mapid of the level.
    public const string AnalyticsEventMapStart = "map_start";


    // Analytics properties:

    // Specifies a map ID in the database.
    public const string AnalyticsParamMapId = "map_id";

  }
}
