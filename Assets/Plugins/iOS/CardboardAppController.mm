// Copyright 2015 Google Inc. All rights reserved.
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

#import "CardboardAppController.h"

extern "C" {

#define WORKAROUND_FULLY_LINKED_GVR_IOS_LIBS 1
#if WORKAROUND_FULLY_LINKED_GVR_IOS_LIBS
// Because of a bug in how the GVR library is linked on iOS, we need to disable
// libgvrunity.a, which means we need to stub out the functions that called it.
void cardboardPause(bool paused) {}
void createUiLayer(id app, UIView* view) {}
#else
extern void cardboardPause(bool paused);
extern void createUiLayer(id app, UIView* view);
#endif

bool isOpenGLAPI() {
  CardboardAppController *app = (CardboardAppController *)GetAppController();
  UnityRenderingAPI api = [app renderingAPI];
  return api == apiOpenGLES2 || api == apiOpenGLES3;
}

void finishActivityAndReturn(bool exitVR) {
  CardboardAppController *app = (CardboardAppController *)GetAppController();
  [app finishActivityAndReturn:exitVR];
}

// We have to manually register the Unity Audio Effect plugin.
struct UnityAudioEffectDefinition;
typedef int (*UnityPluginGetAudioEffectDefinitionsFunc)(
    struct UnityAudioEffectDefinition*** descptr);

#if WORKAROUND_FULLY_LINKED_GVR_IOS_LIBS
// Because of a bug in how the GVR library is linked on iOS, we need to disable
// libgvrunity.a, which means we need to stub out the functions that called it.
void UnityRegisterAudioPlugin(
    UnityPluginGetAudioEffectDefinitionsFunc getAudioEffectDefinitions) {}
int UnityGetAudioEffectDefinitions(UnityAudioEffectDefinition*** definitionptr) { return 0; }
#else
extern void UnityRegisterAudioPlugin(
    UnityPluginGetAudioEffectDefinitionsFunc getAudioEffectDefinitions);
extern int UnityGetAudioEffectDefinitions(UnityAudioEffectDefinition*** definitionptr);
#endif

}  // extern "C"

@implementation CardboardAppController

- (UnityView *)createUnityView {
  UnityRegisterViewControllerListener(self);
  UnityRegisterAudioPlugin(UnityGetAudioEffectDefinitions);
  UnityView* unity_view = [super createUnityView];
  createUiLayer(self, (UIView *)unity_view);
  return unity_view;
}

- (UIViewController *)unityViewController {
  return UnityGetGLViewController();
}

- (void)viewWillAppear:(NSNotification *)notification {
  cardboardPause(false);
}

- (void)setPaused:(BOOL)paused {
  [super setPaused:paused];
  cardboardPause(paused == YES);
}

- (void)finishActivityAndReturn:(BOOL)exitVR {
}

@end

IMPL_APP_CONTROLLER_SUBCLASS(CardboardAppController)
