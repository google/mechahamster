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

namespace Hamster.Utilities {

  static class StringHelper {
    public static string FormatTime(long timeMs) {
      return string.Format("{0:0.000}", timeMs / 1000.0);
    }

    public static string CycleDots(int maxDots = 3) {
      return new string('.', (int)(Time.realtimeSinceStartup % maxDots) + 1);
    }

    // Print out as much useful information as we can about a connection error.
    public static string SigninInFailureString(System.Threading.Tasks.Task connectionTask) {
      if (connectionTask.IsCanceled) {
        return StringConstants.SignInCanceled;
      } else if (connectionTask.IsFaulted) {
        return StringConstants.SignInFailed;
      } else {
        return StringConstants.SignInSuccessful;
      }
    }

  }

}
