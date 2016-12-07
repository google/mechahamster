using UnityEngine;
using System.Collections;

namespace Hamster.MapObjects {

  // When stepped on, these kick the player up into the air.
  public class JumpTile : MapObject {

    // Velocity of the kick, in world-units/second.
    public float kJumpVelocity = 8;

    protected override void MapObjectActivation(Collider collider) {
      if (collider.GetComponent<PlayerController>() != null) {
        Rigidbody rigidbody = collider.attachedRigidbody;
        collider.attachedRigidbody.velocity =
            new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
        rigidbody.AddForce(new Vector3(0.0f, kJumpVelocity, 0.0f), ForceMode.Impulse);
      }
    }
  }
}