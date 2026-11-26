//‚­‚Ë‚­‚Ë°‚ÌƒvƒƒOƒ‰ƒ€

using UnityEngine;
using UnityEngine.U2D;

public class WavyGround : MonoBehaviour
{
    public SpriteShapeController shape;
    public float amplitude = 0.5f;
    public float frequency = 2f;

    private float[] baseY;

    void Start()
    {
        int count = shape.spline.GetPointCount();
        baseY = new float[count];

        for (int i = 0; i < count; i++)
        {
            baseY[i] = shape.spline.GetPosition(i).y;
        }
    }

    void Update()
    {
        int count = shape.spline.GetPointCount();

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = shape.spline.GetPosition(i);

            pos.y = baseY[i] +
                    Mathf.Sin(Time.time * frequency + i) *
                    amplitude;

            shape.spline.SetPosition(i, pos);
        }

        shape.BakeCollider();
    }
}