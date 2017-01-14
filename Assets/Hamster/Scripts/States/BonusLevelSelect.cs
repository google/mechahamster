using UnityEngine;
using System.Collections.Generic;

namespace Hamster.States {
  // Select a bonus map!  Uses the general level select template. (BaseLevelSelect)
  class BonusLevelSelect : BaseLevelSelect {

    // Initialization method.  Called after the state is added to the stack.
    public override void Initialize() {
      mapDBPath = CommonData.DBBonusMapTablePath;
      mapSourceList = CommonData.currentUser.data.bonusMaps;

      base.Initialize();
    }
  }
}
