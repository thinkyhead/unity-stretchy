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

  // Let Targets have properties instead of using multiple arrays
  public StretchyTarget[] target = new StretchyTarget[2];

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

    // Get the default end distance margins from the targets
    ResetTargetMargins();
  }

  /**
   * Set target points to object world positions plus offsets.
   * The local offset is always applied. The world offset is applied if enabled.
   * We call Stretchy.Update to do the hard work.
   */
  protected override void Update() {
    for (int i = 0; i < 2; i++) {
      StretchyTarget t = target[i];
      if (t != null) targetPoint[i] = t.worldTetherPoint;
    }

    base.Update();
  }

  /**
   * Reset targetMargin values from each target's defaultMargin
   * If an end has no target, leave it alone.
   */
  void ResetTargetMargins() {
    for (int i = 0; i < 2; i++) {
      StretchyTarget t = target[i];
      if (t != null) targetMargin[i] = t.defaultMargin;
      // targetMargin[i] = (t != null) ? t.defaultMargin : 0;
    }
  }

  /**
   * Set target.LocalOffset vectors for both ends based on current distance from targets.
   */
  public void RefreshTargetLocalOffsets() {
    target[0].useWorldOffset = target[1].useWorldOffset = false;
    RefreshTargetOffsets();
  }

  /**
   * Set target.LocalOffset vectors for both ends based on current distance from targets.
   */
  public void RefreshTargetWorldOffsets() {
    target[0].useWorldOffset = target[1].useWorldOffset = true;
    RefreshTargetOffsets();
  }

  /**
   * Using the current arrangement of objects,
   * set the target.localOffset and target.worldOffset vectors for both ends.
   *
   * The ends may be swapped if needed.
   *
   * Currently the ends are both set as having either local or world offsets
   * on initialization. World offsets use slightly less processing, but
   * local offsets are the default because they are the more common case,
   * working properly even as the system rotates and scales in the world.
   */
  public void RefreshTargetOffsets() {

    int targetCount = 0;
    bool[] hasTarget = new bool[2];
    for (int i = 0; i < 2; i++)
      if (hasTarget[i] = (target[i] != null)) targetCount++;

    if (targetCount == 0) return;

    Vector3[] endPt = endPoints; // Get start and end points of the stretch

    if (targetCount == 1) {

      // With 1 target a reversal means the ends will be preset as:
      //  (A) Associated with a target, but the point farther from the target
      //  (B) Associated with no target, but the point closer to the target
      //
      // The correct solution is to swap the tether-points:
      //  (A) Associated with a target and the point closer to the target
      //  (B) Associated with no target and the point farther from the target

      int targetEnd = hasTarget[0] ? 0 : 1;         // The index with a target
      StretchyTarget t = target[targetEnd];         // The target component

      // Is the end with no target actually closer to the target?
      if (t.WorldDistanceToPoint(endPt[1-targetEnd]) < t.WorldDistanceToPoint(endPt[targetEnd])) {
        SwapTargetPoints();
        endPt.Swap();
      }
    }
    else { // targetCount == 2

      // End 1 closer to target 2? Just swap the points to get smallest offsets.
      if (target[1].WorldDistanceToPoint(endPt[0]) < target[0].WorldDistanceToPoint(endPt[0]))
        endPt.Swap();
    }

    for (int i = 0; i < 2; i++) {
      if (hasTarget[i]) {
        StretchyTarget t = target[i];
        if (t.useWorldOffset)
          t.SetOffsets(Vector3.zero, t.WorldOffsetToPoint(endPt[i]));
        else
          t.SetOffsets(t.LocalOffsetToPoint(endPt[i]), Vector3.zero);
      }
    }

  } // end of RefreshTargetOffsets

  /**
   * Set an end's target local-offset based on current relative positions.
   * Use this to update an offset later.
   */
  public void RefreshTargetLocalOffset(int end) {
    target[end].useWorldOffset = false;
    RefreshTargetOffset(end);
  }

  /**
   * Set an end's target world-offset based on current relative positions.
   * Use this to update an offset later.
   */
  public void RefreshTargetWorldOffset(int end) {
    target[end].useWorldOffset = true;
    RefreshTargetOffset(end);
  }

  /**
   * Set an end's local or world target offset based on current relative positions.
   * In this implementation, setting one clears the other. But technically it's no
   * problem to use both in tandem.
   */
  public void RefreshTargetOffset(int end) {
    if (end == 0 || end == 1) {
      StretchyTarget t = target[end];
      if (t != null) {
        if (t.useWorldOffset)
          t.SetOffsets(Vector3.zero, t.WorldOffsetToPoint(endPoints[end]));
        else
          t.SetOffsets(t.LocalOffsetToPoint(endPoints[end]), Vector3.zero);
      }
    }
  }

  /**
   * Tether one of the ends to a StretchyTarget (null is ok)
   */
  public void TetherEndToTarget(int end, StretchyTarget target) {
    TetherEndToTargetWithOffset(end, target, Vector3.zero);
  }

  /**
   * Tether one of the ends to a Transform (null is ok)
   */
  public void TetherEndToTransform(int end, Transform trans) {
    TetherEndToTransformWithOffset(end, trans, Vector3.zero);
  }

  /**
   * Tether one of the ends to a StretchyTarget (null is ok) with a world offset
   */
  public void TetherEndToTargetWithWorldOffset(int end, StretchyTarget target, Vector3 offset) {
    TetherEndToTargetWithOffset(end, target, offset, true);
  }

  /**
   * Tether one of the ends to a Transform (null is ok) with a world offset
   */
  public void TetherEndToTransformWithWorldOffset(int end, Transform trans, Vector3 offset) {
    TetherEndToTransformWithOffset(end, trans, offset, true);
  }

  /**
   * Tether one of the ends to a StretchyTarget (null is ok) with a local offset
   */
  public void TetherEndToTargetWithLocalOffset(int end, StretchyTarget target, Vector3 offset) {
    TetherEndToTargetWithOffset(end, target, offset, false);
  }

  /**
   * Tether one of the ends to a Transform (null is ok) with a local offset
   */
  public void TetherEndToTransformWithLocalOffset(int end, Transform trans, Vector3 offset) {
    TetherEndToTransformWithOffset(end, trans, offset, false);
  }

  /**
   * Tether one of the ends to a Transform (null is ok) with a local or world offset
   * The other offset is set to zero, but technically both can be used together
   */
  public void TetherEndToTransformWithOffset(int end, Transform trans, Vector3 offset, bool isWorld=false) {
    GameObject o = trans.gameObject;
    StretchyTarget t = o.GetComponent<StretchyTarget>();
    if (t == null) t = o.AddComponent<StretchyTarget>();
    TetherEndToTargetWithOffset(end, t, offset, isWorld);
  }

  /**
   * Tether one of the ends to a StretchyTarget (null is ok) with a local or world offset
   * The other offset is set to zero, but technically both can be used together
   */
  public void TetherEndToTargetWithOffset(int end, StretchyTarget t, Vector3 offset, bool isWorld=false) {
    if (end == 0 || end == 1) {
      if (t != null) {
        if (isWorld)
          t.SetOffsets(Vector3.zero, offset);
        else
          t.SetOffsets(offset, Vector3.zero);
      }
      target[end] = t;
    }
  }

  /**
   * Untether one of the ends, leaving it tethered to its current position
   */
  public void Untether(int end=-1) {
    if (end == -1) {
      TetherEndToTarget(0, null);
      end = 1;
    }
    TetherEndToTarget(end, null);
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
   * Swap target and targetMargin, but not targetPoint
   *
   * One Target:
   *  End A now tracks the target.
   *  End B now tracks its last point.
   */
  public void SwapTargetObjects() {
    target.Swap();
    targetMargin.Swap();
  }

  public override void SwapTargets() {
    base.SwapTargets();
    SwapTargetObjects();
  }

} // end of StretchyTethered class
