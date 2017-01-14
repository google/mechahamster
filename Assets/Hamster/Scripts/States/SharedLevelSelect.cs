using UnityEngine;
using System.Collections.Generic;

namespace Hamster.States {
  // Select a map that has bene shared!  Uses the general level select template. (BaseLevelSelect)
  class SharedLevelSelect : BaseLevelSelect {

    // Initialization method.  Called after the state is added to the stack.
    public override void Initialize() {
      mapDBPath = CommonData.DBMapTablePath;
      mapSourceList = CommonData.currentUser.data.sharedMaps;

      base.Initialize();
    }
  }
}
