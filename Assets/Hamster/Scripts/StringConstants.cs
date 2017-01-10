using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hamster {

  // String constants used by MechaHamster.
  class StringConstants {

    // Names for Buttons:
    public const string kButtonNameSave = "Save";
    public const string kButtonNameLoad = "Load";
    public const string kButtonNameClear = "Clear";
    public const string kButtonNamePlay = "Play";
    public const string kButtonNameBack = "Back";
    public const string kButtonNameEditor = "Editor";
    public const string kButtonNameInvite = "Invite";
    public const string kButtonNameCancel = "Cancel";

    // Title screen text:
    public const string kTitleText = "MechaHamster!";
    public const string kSubTitleText = "The thrilling adventures of Col. Hammy D. Hamster!";

    // Save/Load screen text:
    public const string kLabelSaveMap = "Save Map:";
    public const string kLabelLoadMap = "Load Map:";
    public const string kDefaultMapName = "Unnamed Map";
    public const string kButtonSaveInNew = "Save as New Map";
    public const string kButtonSaveUpdate = "Save Map";
    public const string kLabelName = "Name:";
    public const string kLabelOverwrite = "Overwrite an existing map:";

    // Remote Config constants:
    public const string kRC_PhysicsGravity = "physics_gravity";
  }
}
