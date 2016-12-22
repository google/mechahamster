using System;
using System.Collections.Generic;

namespace Hamster.States {

  // Class for handling states and transitions between them.
  // Program flow is handled through state classes instead of
  // scene changes, becuase we want to preserve the objects
  // currently in the scene.
  public class StateManager {
    Stack<BaseState> stateStack;

    // Constructor.  Note that there is always at least one
    // state in the stack.  (By default, a base-state that does
    // nothing.)
    public StateManager() {
      stateStack = new Stack<BaseState>();
      stateStack.Push(new BaseState());
    }

    // Pushes a state onto the stack.  Suspends whatever is currently
    // running, and starts the new state up.  If this is being called
    // from a state's update() function, then in general it should be
    // at the end.  (Because calling this triggers suspend() and could
    // lead to weird situations if the state was planning on doing
    // more work.)
    public void PushState(BaseState newState) {
      newState.manager = this;
      CurrentState().Suspend();
      stateStack.Push(newState);
      newState.Initialize();
    }

    // Ends the currently-running state, and resumes whatever is next
    // down the line.
    public void PopState() {
      StateExitValue result = CurrentState().Cleanup();
      stateStack.Pop();
      CurrentState().Resume(result);
    }

    // Switches the current state for a new one, without disturbing
    // anything below.  Different from Pop + Push, in that the next
    // state down never gets resumed/suspended.
    public void SwapState(BaseState newState) {
      newState.manager = this;
      CurrentState().Cleanup();
      stateStack.Pop();
      stateStack.Push(newState);
      CurrentState().Initialize();
    }

    // Called by the main game every update.
    public void Update() {
      CurrentState().Update();
    }

    // Called by the main game every UI update.
    public void OnGUI() {
      CurrentState().OnGUI();
    }

    // Handy utility function for checking the top state in the stack.
    public BaseState CurrentState() {
      return stateStack.Peek();
    }
  }

}
