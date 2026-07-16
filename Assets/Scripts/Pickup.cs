using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Pickup : MonoBehaviour
{
    public event System.Action<Pickup> PickedUp;

    public void CompletePickup() =>
        PickedUp?.Invoke(this);
}
