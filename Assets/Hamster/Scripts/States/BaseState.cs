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

namespace Hamster.States {

  public class BaseState {
    public StateManager manager;

    // Initialization method.  Called after the state
    // is added to the stack.
    public virtual void Initialize() { }
    // Cleanup function.  Called just before the state
    // is removed from the stack.  Returns an optional
    // StateExitValue
    public virtual StateExitValue Cleanup() {
      return null;
    }

    // Suspends the state.  Called when something new is
    // popped over it.
    public virtual void Suspend() { }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public virtual void Resume(StateExitValue results) { }

    // Called once per frame when the state is active.
    public virtual void Update() { }

    // Called once per frame for GUI creation, if the state is active.
    public virtual void OnGUI() { }
  }

  // When states exit, they can return an
  // optional data object to whatever is the next state
  // down on the stack.   Primarily useful for states that are
  // things like yes/no dialog boxes, etc.  This class represents
  // those return values.
  public class StateExitValue {
    public StateExitValue(Type sourceState, object data = null) {
      this.data = data;
      this.sourceState = sourceState;
    }
    public Type sourceState;
    public object data;
  }

}