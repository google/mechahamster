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

namespace Hamster.Ball {

  // Tracks modifications of the physics drag on the attached object.
  public class AdjustableDrag : MonoBehaviour {
    // The original drag that the object starts with.
    public float OriginalDrag { get; private set; }
    // The list of drags that have been applied to the object.
    private List<float> AppliedDrags;
    // The rigid body attached to the object.
    private Rigidbody Rigidbody;

    void Start() {
      Rigidbody = GetComponent<Rigidbody>();
      if (Rigidbody != null) {
        OriginalDrag = Rigidbody.drag;
      }
    }

    public void ApplyDrag(float newDrag) {
      if (Rigidbody != null) {
        Rigidbody.drag = Mathf.Max(Rigidbody.drag, newDrag);
        if (AppliedDrags == null) {
          AppliedDrags = new List<float>();
        }
        AppliedDrags.Add(newDrag);
      }
    }

    public void RemoveDrag(float oldDrag) {
      if (Rigidbody != null && AppliedDrags != null) {
        AppliedDrags.Remove(oldDrag);
        if (oldDrag >= Rigidbody.drag) {
          float newDrag = OriginalDrag;
          if (AppliedDrags.Count > 0) {
            foreach (float drag in AppliedDrags) {
              if (drag > newDrag) {
                newDrag = drag;
              }
            }
          }
          Rigidbody.drag = newDrag;
        }
      } else {
        Debug.LogError("Trying to remove drag where none has been added.");
      }
    }
  }
}
