using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactable.Example
{
	[DisallowMultipleComponent]
	public class IAOutline : InteractableObject
	{

		[Serializable]
		private class ListVector3
		{
			public List<Vector3> data;
		}

		private static HashSet<Mesh> registeredMeshes = new();

		[SerializeField] private Color outlineColor = Color.white;

		[SerializeField] [Range(0f, 1f)] private float outlineWidth = 0.02f;

		[SerializeField] [Range(0f, 1f)] private float maxOutlineWidth = 0.05f;

		[SerializeField] private bool includeChildren = true;

		[Header("Stencil Test")] [SerializeField]
		private int defaultStencilRef = 1;

		[SerializeField] private int activeStencilRef = 2;

		[Header("Optional")]
		[SerializeField]
		[Tooltip(
			"Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. " +
			"Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). " +
			"This may cause a pause for large meshes.")]
		private bool precomputeOutline;

		[SerializeField] [HideInInspector] private List<Mesh> bakeKeys = new();

		[SerializeField] [HideInInspector] private List<ListVector3> bakeValues = new();

		private Renderer[] renderers;
		private Material outlineMaterial;

		private bool interactionAvailable;

		private static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
		private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
		private static readonly int MaxOutlineWidth = Shader.PropertyToID("_MaxOutlineWidth");
		private static readonly int StencilRef = Shader.PropertyToID("_StencilRef");


		#region MonoBehaviour lifecycles

		private void Awake()
		{
			renderers = includeChildren ? GetComponentsInChildren<Renderer>() : GetComponents<Renderer>();
			outlineMaterial = Instantiate(Resources.Load<Material>("Outline"));
			outlineMaterial.name = "Outline (Instance)";
			outlineMaterial.DisableKeyword("ENABLE_OUTLINE");
			LoadSmoothNormals();
			UpdateMaterialProperties();

			foreach (Renderer r in renderers)
			{
				List<Material> list = r.sharedMaterials.ToList();
				list.Add(outlineMaterial);
				r.materials = list.ToArray();
			}
		}

		private void OnValidate()
		{
			if ((!precomputeOutline && bakeKeys.Count != 0) || bakeKeys.Count != bakeValues.Count)
			{
				bakeKeys.Clear();
				bakeValues.Clear();
			}

			if (precomputeOutline && bakeKeys.Count == 0)
			{
				Bake();
			}
		}

		private void OnDestroy()
		{
			Destroy(outlineMaterial);
		}

		#endregion

		#region private

		private void Bake()
		{
			HashSet<Mesh> hashSet = new HashSet<Mesh>();
			MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
			foreach (MeshFilter meshFilter in componentsInChildren)
			{
				if (hashSet.Add(meshFilter.sharedMesh))
				{
					List<Vector3> data = SmoothNormals(meshFilter.sharedMesh);
					bakeKeys.Add(meshFilter.sharedMesh);
					bakeValues.Add(new ListVector3
					{
						data = data
					});
				}
			}
		}

		private void LoadSmoothNormals()
		{
			MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
			foreach (MeshFilter meshFilter in componentsInChildren)
			{
				if (registeredMeshes.Add(meshFilter.sharedMesh))
				{
					int num = bakeKeys.IndexOf(meshFilter.sharedMesh);
					List<Vector3> uvs = ((num >= 0) ? bakeValues[num].data : SmoothNormals(meshFilter.sharedMesh));
					meshFilter.sharedMesh.SetUVs(0, uvs);
					Renderer component = meshFilter.GetComponent<Renderer>();
					if (component != null)
					{
						CombineSubmeshes(meshFilter.sharedMesh, component.sharedMaterials);
					}
				}
			}

			SkinnedMeshRenderer[] componentsInChildren2 = GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren2)
			{
				if (registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
				{
					var sharedMesh = skinnedMeshRenderer.sharedMesh;

					Vector3[] normals = sharedMesh.normals;
					Vector3[] uv = new Vector3[sharedMesh.vertexCount];
					for (int i = 0; i < sharedMesh.vertexCount; i++)
					{
						uv[i] = new Vector3(normals[i].x, normals[i].y, normals[i].z);
					}

					sharedMesh.SetUVs(0, uv);
					CombineSubmeshes(sharedMesh, skinnedMeshRenderer.sharedMaterials);
				}
			}
		}

		private List<Vector3> SmoothNormals(Mesh mesh)
		{
			IEnumerable<IGrouping<Vector3, KeyValuePair<Vector3, int>>> enumerable =
				from pair in mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index))
				group pair by pair.Key;
			List<Vector3> list = new List<Vector3>(mesh.normals);
			foreach (IGrouping<Vector3, KeyValuePair<Vector3, int>> item in enumerable)
			{
				if (item.Count() == 1)
				{
					continue;
				}

				Vector3 zero = Vector3.zero;
				foreach (KeyValuePair<Vector3, int> item2 in item)
				{
					zero += list[item2.Value];
				}

				zero.Normalize();
				foreach (KeyValuePair<Vector3, int> item3 in item)
				{
					list[item3.Value] = zero;
				}
			}

			return list;
		}

		private void CombineSubmeshes(Mesh mesh, Material[] materials)
		{
			if (mesh.subMeshCount != 1 && mesh.subMeshCount <= materials.Length)
			{
				mesh.subMeshCount++;
				mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
			}
		}

		private void UpdateMaterialProperties()
		{
			outlineMaterial.SetColor(EdgeColor, outlineColor);
			outlineMaterial.SetFloat(OutlineWidth, outlineWidth);
			outlineMaterial.SetFloat(MaxOutlineWidth, maxOutlineWidth);
			outlineMaterial.SetInt(StencilRef, defaultStencilRef);
		}

		private void SetStencilRef(bool active)
		{
			outlineMaterial.SetInt(StencilRef, active ? activeStencilRef : defaultStencilRef);
		}

		#endregion


		protected override void OnInteractionAvailable(bool available)
		{
			if (interactionAvailable == available)
				return;
			interactionAvailable = available;

			SetStencilRef(active: available);

			if (available)
			{
				outlineMaterial.EnableKeyword("ENABLE_OUTLINE");
			}
			else
			{
				outlineMaterial.DisableKeyword("ENABLE_OUTLINE");
			}
		}
	}

}
