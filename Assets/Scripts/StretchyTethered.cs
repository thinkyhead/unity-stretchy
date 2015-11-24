/**
 * StretchyTethered.cs
 *
 * Stretch an object (on its Z axis) between two Transforms.
 *
 * If the Transform target for an end is unset, the end remains tethered
 * to the last global point that was set.
 *
 */
using UnityEngine;
using System.Collections;

//[NoExecuteInEditMode]
public class StretchyTethered : Stretchy {

  // Attach in editor or set manually
  public Transform[] targetObj = new Transform[2];

  // Offsets from each target in world space, to anchor to points near targets.
  public Vector3[] targetWorldOffset = new Vector3[2];

  // Offsets from each target in local space, which scale and rotate with targets.
  public Vector3[] targetLocalOffset = new Vector3[2];

  /**
   * When starting out, offsets will be assigned based on the
   * current distances from the ends to their targets.
   * This means the Stretchy object needs to be pre-scaled so its
   * ends align with the target objects.
   */
  protected override void Start() {
    base.Start();

    // Get the current offsets and preserve them
    RefreshTargetOffsets();
  }

  /**
   * Set target points to object world positions plus offsets
   * then call Stretchy.Update to do the hard work.
   */
  protected override void Update() {
    for (int i = 0; i < 2; i++)
      if (targetObj[i] != null) {
        Vector3 transformedOffset = targetObj[i].position - targetObj[i].TransformPoint(targetLocalOffset[i]);
        // Debug.Log("Converting local: " + targetLocalOffset[i] + " to world: " + transformedOffset + " for " + targetObj[i].name);
        targetPoint[i] = targetObj[i].position + targetWorldOffset[i] + transformedOffset;
      }

    base.Update();
  }

  /**
   * Using the current arrangement of objects,
   * set the targetWorldOffset vectors for both ends.
   * The targets may be swapped if needed.
   */
  public void RefreshTargetOffsets() {

    int targetCount = 0;
    bool[] hasTarget = new bool[2];
    for (int i = 0; i < 2; i++)
      if (hasTarget[i] = (targetObj[i] != null)) targetCount++;

    if (targetCount == 0) return;

    Vector3[] endPt = endPoints, // Get start and end points of the stretch
              targetWorldPos = new Vector3[2];

    if (targetCount == 1) {

      // With 1 target a reversal means the ends will be preset as:
      //  (A) Associated with a target, but the end-point farther from the target
      //  (B) Associated with no target, but the end-point closer to the target is set
      //
      // The correct solution is to swap the tether-points:
      //  (A) Associated with a target and the end-point closer to the target
      //  (B) Associated with no target and the end-point farther from the target

      int tetherEnd = hasTarget[0] ? 0 : 1,               // The index that has a target
          otherEnd = 1 - tetherEnd;
      Vector3 targetPos = targetObj[tetherEnd].position;  // The world position of the target

      // Is the end with no target actually closer to the target?
      if ((endPt[otherEnd] - targetPos).magnitude < (endPt[tetherEnd] - targetPos).magnitude) {
        SwapTargetPoints();
        endPt.Swap();
      }

      targetWorldPos[tetherEnd] = targetPos;
      targetWorldPos[otherEnd] = endPt[otherEnd];

    }
    else { // targetCount == 2

      // With 2 targets a reversal means the ends will be preset as:
      //  (A) Associated with a target, but the end-point closer to the other target
      //  (B) Associated with a target, but the end-point closer to the other target
      //
      // The correct solution is to keep the targets but swap the offsets:
      //  (A) Associated with the same target, but the end-point closer to it
      //  (A) Associated with the same target, but the end-point closer to it

      float[] firstEndDistance = new float[2];

      for (int i = 0; i < 2; i++) {
        targetWorldPos[i] = targetObj[i].position;
        firstEndDistance[i] = (endPt[0] - targetWorldPos[i]).magnitude;
      }

      // If the first end is closer to the second target, swap global positions
      if (firstEndDistance[1] < firstEndDistance[0]) endPt.Swap();
    }

    for (int i = 0; i < 2; i++) {
      Vector3 worldOffs = endPt[i] - targetWorldPos[i];
      // targetWorldOffset[i] = worldOffs;
      targetLocalOffset[i] = hasTarget[i] ? -targetObj[i].InverseTransformPoint(endPt[i]) : Vector3.zero;

      // if (hasTarget[i])
      //   Debug.Log("Converting world: " + worldOffs + " to local: " + targetLocalOffset[i] + " for " + targetObj[i].name);
    }

  } // end of RefreshTargetOffsets

  /**
   * Update an end's offset to its target based on current relative positions.
   * Use this to update an offset later.
   */
  public void RefreshTargetOffset(int end) {
    if (end == 0 || end == 1) {
      Transform t = targetObj[end];
      bool hasTarget = (t != null);
      // Vector3 worldOffs = endPoints[end] - (hasTarget ? t.position : targetPoint[end]);
      // targetWorldOffset[end] = worldOffs;
      targetLocalOffset[end] = hasTarget ? -t.InverseTransformPoint(endPoints[end]) : Vector3.zero;
    }
  }

  /**
   * Tether one of the ends to a Transform (null is ok)
   */
  public void TetherEndToTransform(int end, Transform target) {
    if (end == 0 || end == 1)
      TetherEndToTransformWithOffset(end, target, Vector3.zero);
  }

  /**
   * Tether one of the ends to a Transform (null is ok) with a world offset
   */
  public void TetherEndToTransformWithOffset(int end, Transform target, Vector3 offset) {
    if (end == 0 || end == 1) {
      targetObj[end] = target;
      // targetWorldOffset[end] = offset;
      targetLocalOffset[end] = offset;
    }
  }

  /**
   * Untether one of the ends, leaving it tethered to its current position
   */
  public void Untether(int end=-1) {
    if (end == -1) {
      TetherEndToTransform(0, null);
      TetherEndToTransform(1, null);
    }
    else
      TetherEndToTransform(end, null);
  }

  /**
   * Tether one of the ends to a global Point
   */
  public override void TetherEndToWorldPoint(int end, Vector3 point) {
    if (end == 0 || end == 1) {
      Untether(end);
      base.TetherEndToWorldPoint(end, point);
    }
  }

  /**
   * Swap targetObj, targetWorldOffset, targetLocalOffset, and targetMargin, but not targetPoint
   *
   * One Target:
   *  End A now tracks the target.
   *  End B now tracks its last point.
   */
  public void SwapTargetObjects() {
    targetObj.Swap();
    targetWorldOffset.Swap();
    targetLocalOffset.Swap();
    targetMargin.Swap();
  }

  public override void SwapTargets() {
    base.SwapTargets();
    SwapTargetObjects();
  }

} // class Stretchy
