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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster.Utilities {

  // This class provides functionality for profiling a level or play
  // experience.  Once initialize() is called, a timer is started, and
  // framerate samples are taken every update.  Calling Finish() ends
  // the profiling session, and performs various framerate calculations.
  // (Average, Median, standard deviation, etc.)
  // After Finish() has been called, data can be retrieved via the
  // GetDataSnapshot() function.
  public class PerformanceProfiler : MonoBehaviour {

    const int BufferLength = 60 * 15;
    float[] frameTimeBuffer = new float[BufferLength];
    int bufferIndex;
    float startTime;
    float endTime;

    // Data from a performance profiling session.
    // All times are in seconds.
    public struct PerformanceData {
      public int levelIndex;
      public float minFrameTime;
      public float maxFrameTime;
      public float meanFrameTime;
      public float medianFrameTime;
      public float frameTimeStdDev;
      public float fps;
      public float totalTime;
      public float totalSamples;
    }

    bool isRunning = false;
    PerformanceData data;

    // Starts the timer.  Delay is the # of frames to skip before
    // starting the recording.  (Useful because the first second often
    // throws things off, because it is loading and populating a level.
    public void Initialize(int delay = 60) {
      bufferIndex = -delay;
      startTime = 0.0f;
      endTime = 0.0f;
      isRunning = true;
      data.minFrameTime = float.MaxValue;
      data.maxFrameTime = float.MinValue;
    }

    // Update is called once per frame
    void Update() {
      if (!isRunning) return;
      if (bufferIndex < BufferLength) {
        if (bufferIndex >= 0) {
          if (startTime == 0.0)
            startTime = Time.realtimeSinceStartup;

          float frameTime = Time.deltaTime;
          if (frameTime < data.minFrameTime) data.minFrameTime = frameTime;
          if (frameTime > data.maxFrameTime) data.maxFrameTime = frameTime;
          frameTimeBuffer[bufferIndex] = frameTime;
        }
        bufferIndex++;
        // if we run out of buffer, we need to capture the time now,
        // to correctly report the framerate.
        if (bufferIndex >= BufferLength) {
          endTime = Time.realtimeSinceStartup;
        }
      }
    }

    // Stop recording, and process the data.
    public void Finish() {
      // If there is no data when we finish (becuase they skipped the replay
      // or something) then we might as well go back and avoid division
      // by zero errors.
      if (!isRunning || bufferIndex <= 0) return;
      isRunning = false;

      if (bufferIndex == 0) return;
      if (endTime == 0) endTime = Time.realtimeSinceStartup;

      // Calculate FPS.
      data.totalTime = endTime - startTime;
      data.totalSamples = bufferIndex;
      data.fps = bufferIndex / data.totalTime;

      // Calculate mean frame time.
      data.meanFrameTime = 0;
      for (int i = 0; i < bufferIndex; i++) {
        data.meanFrameTime += frameTimeBuffer[i];
      }
      data.meanFrameTime /= bufferIndex;

      // Calculate standard deviation:
      data.frameTimeStdDev = 0;
      for (int i = 0; i < bufferIndex; i++) {
        data.frameTimeStdDev +=
            (data.meanFrameTime - frameTimeBuffer[i]) * (data.meanFrameTime - frameTimeBuffer[i]);
      }
      data.frameTimeStdDev = Mathf.Sqrt(data.frameTimeStdDev / bufferIndex);

      // Calculate median frame time:
      for (int i = bufferIndex; i < BufferLength; i++) {
        frameTimeBuffer[i] = float.MaxValue;
      }
      Array.Sort(frameTimeBuffer);
      data.medianFrameTime = frameTimeBuffer[bufferIndex / 2];
    }

    public PerformanceData GetDataSnapshot() {
      return data;
    }

  }

}