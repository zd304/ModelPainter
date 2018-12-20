using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChannelType
{
    UV0,
    UV1,
    UV2,
    UV3,
}

public class Paintable : MonoBehaviour
{
    void OnEnable()
    {
        if (cachedMaterial == null)
        {
            Shader s = Shader.Find("Hidden/Paint in 3D/Decal");
            cachedMaterial = new Material(s);
        }
        if (cachedMesh == null)
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null)
            {
                GameObject.Destroy(this);
                return;
            }
            cachedMesh = mf.mesh;
        }
    }

    public void HandleHit(PaintBrush brush)
    {
        Quaternion rotation = NormalToCameraRotation(brush.Normal);

        float width = brush.mRadius;
        float height = brush.mRadius;

        if (brush.mDecal != null)
        {
            if (brush.mDecal.width > brush.mDecal.height)
            {
                height *= (float)brush.mDecal.height / (float)brush.mDecal.width;
            }
            else
            {
                width *= (float)brush.mDecal.width / (float)brush.mDecal.height;
            }
        }

		Vector3 size = new Vector3(width, height, brush.mDepth);

        cachedMatrix = Matrix4x4.TRS(brush.Point, rotation, size);
        cachedDirection = rotation * Vector3.forward;
        cachedBrush = brush;
    }

    Quaternion NormalToCameraRotation(Vector3 normal)
    {
        Camera camera = Camera.main;
        Vector3 up = Vector3.up;

        if (camera != null)
        {
            up = camera.transform.up;
        }

        return Quaternion.LookRotation(-normal, up);
    }

    void SetChannel(Material mat, ChannelType channel)
    {
        switch (channel)
        {
            case ChannelType.UV0:
                {
                    mat.SetVector("_Channel", new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
                }
                break;
            case ChannelType.UV1:
                {
                    mat.SetVector("_Channel", new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
                }
                break;
            case ChannelType.UV2:
                {
                    mat.SetVector("_Channel", new Vector4(0.0f, 0.0f, 1.0f, 0.0f));
                }
                break;
            case ChannelType.UV3:
                {
                    mat.SetVector("_Channel", new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                }
                break;
        }
    }

	Vector3 GetRev(Vector3 v)
	{
		Vector3 rst = new Vector3();
		rst.x = v.x == 0.0f ? 0.0f : 1 / v.x;
		rst.y = v.y == 0.0f ? 0.0f : 1 / v.y;
		rst.z = v.z == 0.0f ? 0.0f : 1 / v.z;
		return rst;
	}

    void LateUpdate()
    {
		if (cachedBrush == null)
			return;

        RenderTexture oldActive = RenderTexture.active;
        RenderTexture.active = current;

        SetChannel(cachedMaterial, ChannelType.UV0);
		cachedMaterial.SetMatrix("_Matrix", cachedMatrix.inverse);
		if (cachedBrush.mBuffer != null)
		{
			cachedMaterial.EnableKeyword("P3D_B");
			cachedMaterial.SetTexture("_Buffer", cachedBrush.mBuffer);
		}
		else
		{
			cachedMaterial.DisableKeyword("P3D_B");
		}
        //cachedMaterial.SetTexture("_Buffer", );
        cachedMaterial.SetVector("_Direction", cachedDirection);
        cachedMaterial.SetColor("_Color", cachedBrush.mColor);
		cachedMaterial.SetFloat("_Hardness", cachedBrush.mHandness);
        cachedMaterial.SetTexture("_Texture", cachedBrush.mDecal);
		cachedMaterial.SetFloat("_NormalScale", cachedBrush.mNormalScale);
        cachedMaterial.SetPass(0);

		Vector3 scaling = transform.lossyScale;
		Matrix4x4 meshMatrix = transform.localToWorldMatrix * Matrix4x4.Scale(GetRev(scaling));

        Graphics.DrawMeshNow(cachedMesh, meshMatrix, 0);

        RenderTexture.active = oldActive;

		cachedBrush = null;
    }

    Matrix4x4 cachedMatrix;
    Vector3 cachedDirection;
    PaintBrush cachedBrush;
    Material cachedMaterial;

    Mesh cachedMesh;

    public RenderTexture current;
}
