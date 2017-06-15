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

using UnityEngine;

// Helper class that can be stuck onto animations, to
// disable the animator after an animation state is reached.
public class AnimationDisabler : StateMachineBehaviour {

  // OnStateEnter is called when a transition starts and the state machine starts to
  // evaluate this state.
  override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,
      int layerIndex) {
    animator.enabled = false;
  }

}
