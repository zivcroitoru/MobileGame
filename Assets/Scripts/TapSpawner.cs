using UnityEngine;
using UnityEngine.InputSystem;

// Tap an empty spot: a new coin appears. Tap a coin: it disappears.
public class TapSpawner : MonoBehaviour
{
    [SerializeField] GameObject coinPrefab;   // drag your prefab here in the Inspector

    void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;

        Vector2 world = Camera.main.ScreenToWorldPoint(pointer.position.ReadValue());

        Collider2D hit = Physics2D.OverlapPoint(world);   // what is under the finger?
        if (hit != null)
            Destroy(hit.gameObject);                      // only works if it has a Collider2D
        else
            Instantiate(coinPrefab, world, Quaternion.identity);
    }
}