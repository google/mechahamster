using UnityEngine;
using System.Collections;

namespace Hamster.MapObjects {

  // General base-class for objects on the map.
  public class MapObject : MonoBehaviour {

    // By default, map objects don't do anything special when created or
    // on update, but map tiles can override this if needed.
    public virtual void Start() {
    }

    public virtual void Update() {
    }

    // Generic behavior for map objects.
    // When something hits them, they do stuff.
    void OnTriggerEnter(Collider collider) {
      MapObjectActivation(collider);
    }

    // Objects override this to define custom behavior when hit.
    protected virtual void MapObjectActivation(Collider collider) {
    }
  }
}
