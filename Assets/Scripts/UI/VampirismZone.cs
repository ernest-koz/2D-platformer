using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VampirismZone : MonoBehaviour
{
    [SerializeField] private Vampirism _vampirism;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_vampirism == null)
        {
            _vampirism = GetComponentInParent<Vampirism>();
        }

        if (_vampirism == null)
        {
            Debug.LogError($"{nameof(VampirismZone)} vampirism not assigned on {gameObject.name}.", gameObject);
            enabled = false;
        }
    }

    private void Update()
    {
        _spriteRenderer.enabled = _vampirism.IsActive;
    }
}
