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

  public class BaseState {
    // Depth to spawn UI, when running in mobile mode.
    const float UIDepthMobile = 6.0f;
    // Depth to spawn UI, when running in VR mode.
    const float UIDepthVR = 10.0f;

    // A link to the state manager, used any time we need to switch states.
    public StateManager manager;

    // The actual object representing any UI used by the state.
    protected GameObject gui;

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

    // Called at fixed timesteps during gameplay.  Will not normally
    // be called during UI, as timestep is set to 0 during those times.
    public virtual void FixedUpdate() { }

    // Called once per frame for GUI creation, if the state is active.
    public virtual void OnGUI() { }

    // Called by buttons and other UI elements, when they trigger.
    public virtual void HandleUIEvent(GameObject source, object eventData) { }

    // Spawn a UI Prefab, and return the component associated with it.
    protected T SpawnUI<T>(string prefabLookup) {
      gui = GameObject.Instantiate(CommonData.prefabs.menuLookup[prefabLookup]);
      gui.transform.position = new Vector3(0, 0, CommonData.inVrMode ? UIDepthVR : UIDepthMobile);
      gui.transform.SetParent(CommonData.mainCamera.transform, false);
      ResizeUI(gui);
      return gui.GetComponent<T>();
    }

    // Sets the size of the UI and makes sure it fits on the screen, etc.
    protected virtual void ResizeUI(GameObject gui) {
      Camera camera = CommonData.mainCamera.GetComponentInChildren<Camera>();
      RectTransform rt = gui.GetComponent<RectTransform>();
      Vector2 lowerLeft =
          camera.WorldToScreenPoint(rt.TransformPoint(rt.anchorMin + rt.offsetMin));
      Vector2 upperRight =
          camera.WorldToScreenPoint(rt.TransformPoint(rt.anchorMax + rt.offsetMax));

      float totalWidth = Mathf.Abs(upperRight.x - lowerLeft.x);
      float totalHeight = Mathf.Abs(upperRight.y - lowerLeft.y);
      float guiScale = 1.0f;
      if (totalWidth > Screen.width) {
        guiScale = Screen.width / totalWidth;
      }
      if (totalHeight > Screen.height) {
        guiScale = Math.Min(Screen.height / totalHeight, guiScale);
      }
      gui.transform.localScale *= guiScale;
    }

    // Shows any UI that is currently hidden.
    protected void ShowUI() {
      if (gui != null) {
        gui.SetActive(true);
      }
    }

    // Hides all active GUI, and clears it from the ActiveButtonList.
    protected void HideUI() {
      if (gui != null) {
        gui.SetActive(false);
      }
      Menus.GUIButton.allActiveButtons.Clear();
    }


    // Destroy the UI prefab.
    protected void DestroyUI() {
      if (gui != null) {
        GameObject.Destroy(gui);
        gui = null;
      }
    }

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