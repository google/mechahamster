// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster.States {

  // Class for handling states and transitions between them.
  // Program flow is handled through state classes instead of
  // scene changes, because we want to preserve the objects
  // currently in the scene.
  public class StateManager {
    Stack<BaseState> stateStack;

    // Constructor.  Note that there is always at least one
    // state in the stack.  (By default, a base-state that does
    // nothing.)
    public StateManager() {
      stateStack = new Stack<BaseState>();
      stateStack.Push(new BaseState());
    }

    // Pushes a state onto the stack.  Suspends whatever is currently
    // running, and starts the new state up.  If this is being called
    // from a state's update() function, then in general it should be
    // at the end.  (Because calling this triggers suspend() and could
    // lead to weird situations if the state was planning on doing
    // more work.)
    public void PushState(BaseState newState) {
      newState.manager = this;
      CurrentState().Suspend();
      stateStack.Push(newState);
      newState.Initialize();
    }

    // Ends the currently-running state, and resumes whatever is next
    // down the line.
    public void PopState() {
      StateExitValue result = CurrentState().Cleanup();
      stateStack.Pop();
      CurrentState().Resume(result);
    }

    // Clears out all states, leaving just newState as the sole state
    // on the stack.  Since PopState is called, all underlying states
    // still get to respond to Resume() and Cleanup().  Mainly useful
    // for soft resets where we don't want to care about how many levels
    // of menu we have below us.
    public void ClearStack(BaseState newState) {
      while (stateStack.Count > 1) {
        PopState();
      }
      SwapState(newState);
    }

    // Switches the current state for a new one, without disturbing
    // anything below.  Different from Pop + Push, in that the next
    // state down never gets resumed/suspended.
    public void SwapState(BaseState newState) {
      newState.manager = this;
      CurrentState().Cleanup();
      stateStack.Pop();
      stateStack.Push(newState);
      CurrentState().Initialize();
    }

    // Called by the main game every update.
    public void Update() {
      CurrentState().Update();
    }

    // Called by the main game every fixed update.
    // Note that during most UI and menus, the update timestep
    // is set to 0, so this function will not fire.
    public void FixedUpdate() {
      CurrentState().FixedUpdate();
    }

    // Called by the main game every UI update.
    public void OnGUI() {
      CurrentState().OnGUI();
    }

    // Handy utility function for checking the top state in the stack.
    public BaseState CurrentState() {
      return stateStack.Peek();
    }

    // When GUIButton receives a Unity UI event, it reports it via
    // this function.  (Which then directs it to whichever state is active.)
    public void HandleUIEvent(GameObject source, object eventData) {
      CurrentState().HandleUIEvent(source, eventData);
    }
  }

}
