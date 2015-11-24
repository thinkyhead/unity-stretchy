/**
 * OrbitZoomCam.cs
 *
 * The most basic of orbiting-zooming cameras...
 *
 * - Orbit the camera by dragging with the finger or mouse.
 * - Zoom the camera with pinch, mousewheel, or scroll gesture.
 *
 */
using UnityEngine;
using System.Collections;

public class OrbitZoomCam : MonoBehaviour {

  public static OrbitZoomCam instance;
  public float inertialOrbit = 0, dampenOrbit = 0;

  public Vector3 target = new Vector3(0, 500, 0);
  float distance = 15, newDist = 15;
  float xSpeed = 25, ySpeed = 12;
  float distanceMin = 10, distanceMax = 100;
 
  int firstFingerID = -1, pinchFingerID = -1;
  Vector2 firstFingerXY, pinchFingerXY;
  float prevPinchDist = 0, zoomDelta = 0;
  Vector2 orbitVelocity = new Vector2(0, 0);
  float orbitFactor;

  Transform T;

  void Awake() {
    instance = this;
    orbitFactor = 1 - dampenOrbit;
    T = transform;
  }
  
  void Start() {
    newDist = distance;

    Vector3 angles = T.eulerAngles;
    T.rotation = Quaternion.Euler(angles.y, angles.x, 0);
    T.position = new Vector3(0.0f, 0.0f, -distance) + target;

    // Make the rigid body not change rotation
    Rigidbody r = GetComponent<Rigidbody>();
    if (r != null) r.freezeRotation = true;
  }
  
  void Update() {
    zoomDelta = -Input.GetAxis("Mouse ScrollWheel");
    Vector2 rotDelta = Vector2.zero;
    
    // Handle touches, if there are any
    if (Input.touchCount > 0) {
      Vector2 firstFingerHV = Vector2.zero;
      foreach (Touch t in Input.touches) {    
        // Debug.Log("Touches exist and rotDelta is  " + rotDelta);
        int fid = t.fingerId;
        Vector2 pos = t.position;
        switch (t.phase) {
          case TouchPhase.Began:
            if (firstFingerID == -1) {
              firstFingerID = fid;
              firstFingerXY = pos;
              firstFingerHV.Set(0, 0);
              // Debug.Log("Touch began at "+pos);
            }
            else if (pinchFingerID == -1) {
              pinchFingerID = fid;
              pinchFingerXY = pos;
              prevPinchDist = (firstFingerXY - pinchFingerXY).magnitude;
            }
            break;

          case TouchPhase.Stationary:
            if (firstFingerID == fid) {
              // Debug.Log("Touch stationary at "+pos);
              firstFingerHV.Set(0, 0);
            }
            else if (pinchFingerID == fid) {
            }
            break;
          case TouchPhase.Moved:
            if (firstFingerID == fid) {
              // Debug.Log("Touch moved at " + pos + " with first at " + firstFingerXY);
              firstFingerHV = pos - firstFingerXY;
              firstFingerXY = pos;
            }
            else if (pinchFingerID == fid) {
              pinchFingerXY = pos;
            }
            break;

          case TouchPhase.Ended:
          case TouchPhase.Canceled:
            if (firstFingerID == fid) {
              // Debug.Log("Touch ended at " + pos + " with previous at " + firstFingerXY + " and pinch at " + pinchFingerXY);
              firstFingerID = pinchFingerID;
              firstFingerXY = pinchFingerXY;
              firstFingerHV = Vector2.zero;
              pinchFingerID = -1;
            }
            else if (pinchFingerID == fid) {
              pinchFingerID = -1;
            }
            break;
        }
      }
  
      // if pinching is still happening...
      if (pinchFingerID != -1) {
        // zoom by the difference since last time
        float dist = (firstFingerXY - pinchFingerXY).magnitude;
        zoomDelta = (dist - prevPinchDist) * 0.01f;
        prevPinchDist = dist;
      }
      else if (firstFingerHV.x != 0 || firstFingerHV.y != 0) {
        // Debug.Log("Rotation changed by " + firstFingerHV);
        rotDelta = firstFingerHV * 0.2f;
        orbitVelocity = rotDelta * inertialOrbit;
      }
    }
    // only get the mouse button if there are no touches
    else if (Input.GetMouseButton(0)) {
      rotDelta.Set(0.02f * xSpeed * Input.GetAxis("Mouse X"), 0.02f * ySpeed * Input.GetAxis("Mouse Y"));
      orbitVelocity = rotDelta * inertialOrbit;
    }

    if (rotDelta != Vector2.zero) {
      T.rotation *= Quaternion.AngleAxis(rotDelta.x, Vector3.up) * Quaternion.AngleAxis(rotDelta.y, Vector3.left);
    }
    else if (inertialOrbit > 0 && orbitVelocity != Vector2.zero) {
      T.rotation *= Quaternion.AngleAxis(orbitVelocity.x, Vector3.up) * Quaternion.AngleAxis(orbitVelocity.y, Vector3.left);
      orbitVelocity *= orbitFactor;
    }

    if (zoomDelta != 0) {
      newDist -= zoomDelta * Mathf.Abs(newDist / 2);
      newDist = Mathf.Clamp(newDist, distanceMin, distanceMax);
    }

    float howfar = newDist - distance;
    if (Mathf.Abs(howfar) > 5) {
      distance += howfar / 5;
    }

    T.position = target + (T.rotation * new Vector3(0.0f, 0.0f, -distance));
  }
}
