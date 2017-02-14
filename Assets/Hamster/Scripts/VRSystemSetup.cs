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

  // This class creates things at startup that are required
  // by the game
  public class VRSystemSetup : MonoBehaviour {
    public bool StartInVRMode = false;

    public GameObject VREventSystem;
    public GameObject VRViewer;
    public GameObject VRController;
    public GameObject VRControllerPointer;

    public CameraController CameraHolder;

    private void Awake() {
      if (StartInVRMode) {
        Instantiate(VREventSystem);
        GameObject viewer = Instantiate(VRViewer);
        Instantiate(VRController);
        GvrViewer gvrViewer = viewer.GetComponent<GvrViewer>();
        if (gvrViewer) {
          gvrViewer.VRModeEnabled = false;
        }

        GameObject pointer = Instantiate(VRControllerPointer);
        pointer.transform.SetParent(CameraHolder.transform);
        pointer.transform.localPosition = new Vector3(0.0f, 0.0f, -9.75f);
      }
    }
  }
}
