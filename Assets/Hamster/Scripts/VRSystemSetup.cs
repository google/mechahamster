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
using System.Collections.Generic;

namespace Hamster {

  // This class spawns various things at startup that are required to
  // play the game in VR mode.  It also sets up the event system/ui canvas.
  public class VRSystemSetup : MonoBehaviour {
    public bool SimulateVRInEditor = false;

    public GameObject VRViewer;
    public GameObject VRController;
    public GameObject VRControllerPointer;

    public CameraController CameraHolder;

    public Canvas canvas;
    public UnityEngine.EventSystems.EventSystem eventSystem;

    private void Awake() {
      CommonData.inVrMode =
          UnityEngine.VR.VRSettings.enabled || (Application.isEditor && SimulateVRInEditor);
      CommonData.canvas = canvas;
      if (CommonData.inVrMode) {
        GameObject viewer = Instantiate(VRViewer);
        Instantiate(VRController);
        GvrViewer gvrViewer = viewer.GetComponent<GvrViewer>();
        if (gvrViewer) {
          gvrViewer.VRModeEnabled = false;
        }

        GameObject pointer = Instantiate(VRControllerPointer);
        pointer.transform.SetParent(CameraHolder.transform);
        #if UNITY_EDITOR
        // Make it easier to see the controller pointer in editor's game view,
        // but leave controller positioning up to GVR arm model in final build.
        pointer.transform.localPosition = .25f * Vector3.forward;
        #endif

        canvas.gameObject.AddComponent<GvrPointerGraphicRaycaster>();
        eventSystem.gameObject.AddComponent<GvrPointerInputModule>();
        eventSystem.gameObject.AddComponent<GvrPointerManager>();
      } else {
        canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        eventSystem.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
      }
    }
  }
}
