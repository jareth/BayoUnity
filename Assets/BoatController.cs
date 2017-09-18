using System.Collections.Generic;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

public class BoatController : MonoBehaviour
{
	[SerializeField] private Rigidbody _boatRigidBody;
	[SerializeField] private GameObject _underwaterObject;
	[SerializeField] private OptionsController _optionsController;

	private Mesh _underwaterMesh;
	private UnderwaterMeshManager _underwaterMeshManager;
	private List<SlammingForce> _slammingForces;
	private UnderwaterPhysics _underwaterPhysics;
	private float _totalArea;

	private void Awake ()
	{
		Debug.Assert(_underwaterObject != null, "Boat " + this + " underwater object not linked!");
		Debug.Assert(_boatRigidBody != null, "Boat " + this + " rigid body not linked!");
		Debug.Assert(_optionsController != null, "_optionsController != null");
		
		//_boatRigidBody.velocity = new Vector3(0, 0, 10);
		
		_underwaterMesh = _underwaterObject.GetComponent<MeshFilter>().mesh;
		_slammingForces = new List<SlammingForce>();
		InitialiseSlammingForces();
		_underwaterMeshManager = new UnderwaterMeshManager(gameObject, _slammingForces);
		_underwaterPhysics = new UnderwaterPhysics(
			_boatRigidBody, 
			_underwaterMeshManager.UnderwaterTriangles, 
			_slammingForces, 
			_optionsController,
			_totalArea);
	}

	private void InitialiseSlammingForces()
	{
		var originalTriangles = gameObject.GetComponent<MeshFilter>().mesh.triangles;
		var originalVerticies = gameObject.GetComponent<MeshFilter>().mesh.vertices;
		_totalArea = 0f;

		for (int i = 0; i < originalTriangles.Length; i += 3)
		{
			var pointA = originalVerticies[originalTriangles[i]];
			var pointB = originalVerticies[originalTriangles[i + 1]];
			var pointC = originalVerticies[originalTriangles[i + 2]];
			var center = (pointA + pointB + pointC) / 3f;
			var sideA = Vector3.Distance(pointA, pointB);
			var sideC = Vector3.Distance(pointC, pointA);
			var area = sideA * sideC * Mathf.Sin(Vector3.Angle(pointB - pointA, pointC - pointA) * Mathf.Deg2Rad) / 2f;
			_slammingForces.Add(new SlammingForce
			{
				OriginalArea = area,
				TriangleCenter = center
			});
			_totalArea += area;
		}
	}

	private void Update()
	{
		_underwaterMeshManager.GenerateUnderwaterMesh();
		_underwaterMeshManager.CreateDisplayMesh(_underwaterMesh);
	}

	private void FixedUpdate()
	{
        //_boatRigidBody.AddForceAtPosition(_boatRigidBody.transform.TransformVector(Vector3.forward) * 1000, new Vector3(-0.5f, -0.5f, -0.5f));
		_underwaterPhysics.UpdateForces(_underwaterMesh.bounds.size.z);
	}
}
