using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintBrush : MonoBehaviour
{
	public Color mColor = Color.red;
    public float mRadius = 0.05f;
	public float mDepth = 0.1f;
	public float mHandness = 1.0f;
	public float mNormalScale = 1.0f;
    public Texture mDecal = null;
	public Texture mBuffer = null;
	private float mLastTime;

    public Vector3 Point
    {
        set;
        get;
    }

    public Vector3 Normal
    {
        set;
        get;
    }

	void Start()
    {
		mLastTime = 0.0f;
	}
	
	void Update()
    {
		if (Time.realtimeSinceStartup - mLastTime <= 0.05f)
			return;
		mLastTime = Time.realtimeSinceStartup;

		if (Input.GetMouseButton(0))
        {
            Vector2 mousePosition = (Vector2)Input.mousePosition;
			PaintAt(mousePosition);
        }
	}

    void PaintAt(Vector2 screenPosition)
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, -1))
            {
                Point = hit.point;
                Normal = -ray.direction;

                Paintable[] paintables = hit.transform.GetComponentsInChildren<Paintable>();
                for (int i = 0; i < paintables.Length; ++i)
                {
                    Paintable paintable = paintables[i];
                    paintable.HandleHit(this);
                }
            }
        }
    }
}
