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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Hamster {
  // A Component to animate attached game object based on given replay data
  public class ReplayAnimator : MonoBehaviour {
    // Loop once the last position entry is played in the replay data
    public bool shouldLoop = false;

    // Event triggered when the last position entry is played.
    // If shouldLoop property is true, this will never be triggered.
    public UnityEvent FinishEvent {
      get {
        if (finishEvent == null) {
          finishEvent = new UnityEvent();
        }
        return finishEvent;
      }
    }
    private UnityEvent finishEvent = null;

    ReplayData replayData = null;

    // Whether this animator is playing the replay data.
    enum AnimState {
      Stop,     // Animation stopped.  Reset to first frame
      Playing,  // Playing animation.
      Finished  // The animation is complete and not looping
    }

    AnimState animState = AnimState.Stop;

    // Current frame number
    int timestamp = 0;

    // Current index of position entry with the timestamp greater or equal to the current one.
    int positionDataIndex = 0;

    // Set the replay data to animate
    public void SetReplayData(ReplayData data) {
      replayData = data;

      timestamp = 0;
      positionDataIndex = 0;

      // Move the gameobject to the first recorded position to prevent animation pop
      if (replayData != null && replayData.positionData.Length > 0) {
        transform.position = replayData.positionData[0].position;
        transform.rotation = replayData.positionData[0].rotation;
      }
    }

    public void Play() {
      animState = AnimState.Playing;
    }

    public void Stop() {
      timestamp = 0;
      positionDataIndex = 0;
      animState = AnimState.Stop;
    }

    // Update the current GameObject position and rotation in fixed framerate if in playing state.
    void FixedUpdate() {
      if(replayData != null && animState == AnimState.Playing) {
        // Move positionDataIndex so that the timestamp of current positionEntry is greater or
        // equal to current time stamp.
        while (positionDataIndex < replayData.positionData.Length &&
            replayData.positionData[positionDataIndex].timestamp < timestamp) {
          positionDataIndex++;
        }

        if (positionDataIndex < replayData.positionData.Length) {
          PositionDataEntry entry = replayData.positionData[positionDataIndex];
          if (positionDataIndex == 0 || entry.timestamp == timestamp) {
            transform.position = entry.position;
            transform.rotation = entry.rotation;
          } else {
            // Interpolate position/rotation based on two recorded frames
            PositionDataEntry startEntry = replayData.positionData[positionDataIndex - 1];

            float fT = (float)(timestamp - startEntry.timestamp) /
                       (float)(entry.timestamp - startEntry.timestamp);

            transform.position = Vector3.Lerp(startEntry.position, entry.position, fT);
            transform.rotation = Quaternion.Slerp(startEntry.rotation, entry.rotation, fT);
          }
          ++timestamp;
        } else {
          if (shouldLoop) {
            // Loop back to first frame
            timestamp = 0;
            positionDataIndex = 0;
          } else {
            animState = AnimState.Finished;
            if (finishEvent != null) {
              finishEvent.Invoke();
            }
          }
        }
      }
    }
  }
}
