using UnityEngine;
using System.Collections;

//*****************************************************************************
//       Crucial Collider Gizmo by TimeFloat. Thanks for purchasing!         **
//*****************************************************************************

public enum CCGPresets
{
	Custom,
	Red,
	Blue,
	Green,
	Purple,
	Yellow
};

// Simply attach this script to any game object that has a collider that you would like
// to be drawn on screen and you are good to go!

public class CrucialColliderGizmo : MonoBehaviour
{
	
	public CCGPresets selectedPreset;
	
	public Color savedCustomWireColor;
	public Color savedCustomFillColor;
	public Color savedCustomCenterColor;
	
	public float overallAlpha = 1.0f;
	public Color wireColor = new Color(.6f, .6f, 1f, .5f);
	public Color fillColor = new Color(.6f,.7f,1f,.1f);
	public Color centerColor = new Color(.6f,.7f,1f,.7f);
	
	public bool drawFill = true;
	public bool drawWire = true;
	
	public bool drawCenter = false;
	public float centerMarkerRadius = 1.0f;
	public float edgePointMarkerRadius = .5f;
	
	public float collider2D_ZDepth = 2.0f;
	
	public bool includeChildColliders = false;
	
	void OnDrawGizmos()
	{
		if(!enabled)
		{
			return;
		}

		DrawColliders(this.gameObject);
		
		Transform[] allTransforms = gameObject.GetComponentsInChildren<Transform>();
		
		if(includeChildColliders)
		{
			for(int i = 0; i < allTransforms.Length; i++)
			{
				if(allTransforms[i].gameObject == this.gameObject)
				{
					continue;
				}
				DrawColliders(allTransforms[i].gameObject);
			}
		}
	}
	
	delegate void Drawer(CubeDrawer cubeDrawer, SphereDrawer sphereDrawer, LineDrawer linedrawer);
	
	delegate void CubeDrawer(Vector3 center, Vector3 size);
	delegate void SphereDrawer(Vector3 center, float radius);
	delegate void LineDrawer(Vector3 posOne, Vector3 pos2);
	
	void DrawCollider(SphereDrawer drawer, EdgeCollider2D collider, Vector3 position, Vector3 scale, Transform targetTran, LineDrawer lineDrawer)
	{
		if (!collider) return;

		Vector3 prev = Vector2.zero;
		for (int i = 0; i < collider.points.Length; i++)
		{
			var colPoint = collider.points[i];
			Vector3 pos = new Vector3(colPoint.x * scale.x, colPoint.y * scale.y, 0);
			Vector3 rotated = targetTran.rotation * pos;

			if (i != 0) {
				lineDrawer(position + prev, position + rotated);
			}

			prev = rotated;


			drawer(position + rotated, edgePointMarkerRadius);
		}
	}

	void DrawCollider(CubeDrawer drawer, BoxCollider2D collider, Transform targetTran)
	{
		if (!collider) return;
		Vector3 newColliderLocation = new Vector3(targetTran.position.x + collider.offset.x, targetTran.position.y + collider.offset.y, targetTran.position.z);
		Vector3 newColliderSize = new Vector3(collider.bounds.size.x, collider.bounds.size.y, collider2D_ZDepth);
		drawer(newColliderLocation, newColliderSize);
	}
	
	void DrawCollider(CubeDrawer drawer, BoxCollider collider, Transform targetTran)
	{
		if (!collider) return;
		Gizmos.matrix = Matrix4x4.TRS(targetTran.position, targetTran.rotation, targetTran.lossyScale);
		drawer(collider.center, collider.size);
		Gizmos.matrix = Matrix4x4.identity;
	}
	
	void DrawCollider(SphereDrawer drawer, CircleCollider2D collider, Vector3 position, Vector3 scale)
	{
		if (!collider) return;
		drawer(position + new Vector3(collider.offset.x, collider.offset.y, 0.0f), collider.radius * Mathf.Max(scale.x, scale.y));
	}
	
	void DrawCollider(SphereDrawer drawer, SphereCollider collider, Vector3 position, Vector3 scale)
	{
		if (!collider) return;
		drawer(position + new Vector3(collider.center.x, collider.center.y, 0.0f), collider.radius * Mathf.Max(scale.x, scale.y, scale.z));
	}
	
	void DrawColliders(GameObject hostGameObject)
	{
		Transform targetTran = hostGameObject.transform;
		Vector3 position = targetTran.position;
		Vector3 trueScale = targetTran.localScale;
		while(targetTran.parent != null)
		{
			targetTran = targetTran.parent;
			trueScale = new Vector3(targetTran.localScale.x * trueScale.x, targetTran.localScale.y * trueScale.y, targetTran.localScale.z * trueScale.z);
		}
		
		Drawer draw = (CubeDrawer cubeDrawer, SphereDrawer sphereDrawer, LineDrawer linedrawer) =>
			
		{
			DrawCollider(sphereDrawer, hostGameObject.GetComponent<EdgeCollider2D>(), position, trueScale, hostGameObject.transform, linedrawer);
			DrawCollider(cubeDrawer, hostGameObject.GetComponent<BoxCollider2D>(), hostGameObject.transform);
			DrawCollider(cubeDrawer, hostGameObject.GetComponent<BoxCollider>(), hostGameObject.transform);
			DrawCollider(sphereDrawer, hostGameObject.GetComponent<CircleCollider2D>(), position, trueScale);
			DrawCollider(sphereDrawer, hostGameObject.GetComponent<SphereCollider>(), position, trueScale);
		};
		
		Gizmos.color = new Color(wireColor.r, wireColor.g, wireColor.b, wireColor.a * overallAlpha);
		if (drawWire)
		{
			draw(Gizmos.DrawWireCube, Gizmos.DrawWireSphere, Gizmos.DrawLine);
		}
		
		Gizmos.color = new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * overallAlpha);
		if (drawFill)
		{
			draw(Gizmos.DrawCube, Gizmos.DrawSphere, Gizmos.DrawLine);
		}
		
		if(drawCenter)
		{
			Gizmos.color = new Color(centerColor.r, centerColor.g, centerColor.b, centerColor.a * overallAlpha);
			Gizmos.DrawSphere(hostGameObject.transform.position, centerMarkerRadius );
		}
	}
}