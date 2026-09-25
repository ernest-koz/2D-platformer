using UnityEngine;

public static class TargetSearch
{
    private const int InitialTargetBufferSize = 8;
    private const int MaximumTargetBufferSize = 64;

    public static Collider2D[] CreateBuffer()
    {
        return new Collider2D[InitialTargetBufferSize];
    }

    public static int Collect(Vector2 origin, float range, LayerMask targetLayer, ref Collider2D[] buffer)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayer);

        int count = Physics2D.OverlapCircle(origin, range, filter, buffer);

        while (count == buffer.Length)
        {
            if (buffer.Length >= MaximumTargetBufferSize)
            {
                break;
            }

            int newSize = Mathf.Min(buffer.Length * 2, MaximumTargetBufferSize);
            buffer = new Collider2D[newSize];

            count = Physics2D.OverlapCircle(origin, range, filter, buffer);
        }

        return count;
    }

    public static ITargetable FindNearest(Collider2D[] buffer, int count, Vector2 origin, GameObject self)
    {
        ITargetable nearest = null;
        float nearestSquaredDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (buffer[i].gameObject == self)
            {
                continue;
            }

            if (buffer[i].TryGetComponent(out ITargetable candidate) == false)
            {
                continue;
            }

            if (candidate.IsTargetable == false)
            {
                continue;
            }

            float squaredDistance = ((Vector2)candidate.Position - origin).sqrMagnitude;

            if (squaredDistance < nearestSquaredDistance)
            {
                nearestSquaredDistance = squaredDistance;
                nearest = candidate;
            }
        }

        return nearest;
    }
}
