using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class RandomPulse : MonoBehaviour {

  Rigidbody R;
  public float spin = 0;

  void Awake() {
    R = gameObject.GetComponent<Rigidbody>();
  }

  void Start () {
    StartCoroutine(PulseAndWait());
    R.angularVelocity = new Vector3(spin, 0);
  }
  
  IEnumerator PulseAndWait() {
    yield return new WaitForSeconds(5 + Random.value * 3);
    float force = 50;
    for (;;) {
      R.AddForce(
        (Random.value - 0.5f) * force * 2f,
        (Random.value - 0.5f) * force * 2f,
        (Random.value - 0.5f) * force * 2f
      );
      yield return new WaitForSeconds(Random.Range(2, 6+1));
    }
  }
}
