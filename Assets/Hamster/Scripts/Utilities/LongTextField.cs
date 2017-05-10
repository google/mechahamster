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
using UnityEngine.UI;
using System.Collections.Generic;

namespace Hamster.Utilities {

  // Class for displaying long text fields.  Unity uses 4 verts per
  // character, and has a 64k vert limit per text field, so they
  // stop working at around 16k length.  This text field
  // circumvents that by breaking a long string into multiple
  // text fields based on newlines.
  public class LongTextField : MonoBehaviour {

    string text;
    List<GameObject> textFields = new List<GameObject>();
    // Set in inspector.  Prefab to use when spawning
    // chunks of text.
    public RectTransform TextChunkPrefab;

    // Unity uses 65000 as the limit instead of 65536.
    const int MaxStringLength = (65000 / 4) - 1;

    // Spawn the required text.  Also removes any existing text
    // that has been spawned.
    public void SpawnText(string text) {
      this.text = text;
      string[] lines = text.Split('\n');
      RectTransform rectTransform = GetComponent<RectTransform>();
      RemoveAllText();
      string currentString = "";
      foreach (string line in lines) {
        int currentLength = currentString.Length;
        if (currentLength + line.Length >= MaxStringLength) {
          SpawnTextBlock(currentString, rectTransform);
          currentString = line;
        } else {
          currentString += currentLength != 0 ? "\n" + line : line;
        }
      }
      SpawnTextBlock(currentString, rectTransform);
    }


    void SpawnTextBlock(string text, RectTransform parent) {
      GameObject newText = Instantiate(TextChunkPrefab.gameObject);

      newText.GetComponent<Text>().text = text;
      newText.GetComponent<RectTransform>().SetParent(parent, false);

      textFields.Add(newText);
    }


    // Removes any existing game objects that are currently displaying
    // text.
    void RemoveAllText() {
      foreach (GameObject obj in textFields) {
        Destroy(obj);
      }
      textFields.Clear();
    }
  }

}
