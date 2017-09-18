using System.Collections.Generic;
using UnityEngine;

public class UnderwaterPhysics {
    
    private const float RhoWater = 1027f;
    private const float Viscosity = 0.000001f;

    private readonly Rigidbody _rigidbody;
    private static Transform _transform;
    private List<WaterTriangle> _underwaterTriangles;
    private readonly List<SlammingForce> _slammingForces;
    private readonly OptionsController _optionsController;
    private readonly float _originalMeshArea;

    public UnderwaterPhysics(
        Rigidbody rigidbody, 
        List<WaterTriangle> underwaterTriangles, 
        List<SlammingForce> slammingForces, 
        OptionsController optionsController,
        float originalMeshArea)
    {
        _rigidbody = rigidbody;
        _underwaterTriangles = underwaterTriangles;
        _slammingForces = slammingForces;
        _optionsController = optionsController;
        _originalMeshArea = originalMeshArea;
        _transform = _rigidbody.gameObject.transform;
    }

    public void UpdateForces(float length)
    {
        var resistanceCoefficent = GetResistanceCoefficient(_rigidbody.velocity.magnitude, length);
		CalculateSlammingVelocities(_slammingForces);

        foreach (var triangle in _underwaterTriangles)
        {
            CalculateTriangleVelocity(triangle);

            if (_optionsController.SlammingEnabled)
            {
                _rigidbody.AddForceAtPosition(
                    SlammingForce(_slammingForces[triangle.OriginalMeshIndex], triangle, _originalMeshArea, _rigidbody.mass), 
                    _transform.TransformPoint(triangle.Center), 
                    ForceMode.Impulse);
            }

			var forceVector = new Vector3();
            forceVector += _optionsController.BounancyEnabled ? BouancyForce(triangle) : Vector3.zero;
            forceVector += _optionsController.ViscosityEnabled ? ViscousWaterResistanceForce(triangle, resistanceCoefficent) : Vector3.zero;
            forceVector += _optionsController.PressureEnabled ? PressureDragForce(triangle) : Vector3.zero;

            if (float.IsNaN(forceVector.x + forceVector.y + forceVector.z))
            {
                Debug.Log("NaN");
            }

            _rigidbody.AddForceAtPosition(forceVector, _transform.TransformPoint(triangle.Center));
        }
    }

    private float GetResistanceCoefficient(float velocity, float length)
    {
        var reynoldsNum = velocity * length / Viscosity;
        return 0.075f / Mathf.Pow(Mathf.Log10(reynoldsNum) - 2f, 2f);
    }
    
    private void CalculateSlammingVelocities(List<SlammingForce> slammingForces)
    {
        for (var i = 0; i < slammingForces.Count; i++)
        {
            slammingForces[i].PreviousVelocity = slammingForces[i].Velocity;
            var center = _transform.TransformPoint(slammingForces[i].TriangleCenter);
            slammingForces[i].Velocity = _rigidbody.GetRelativePointVelocity(center);
        }
    }
    
    private void CalculateTriangleVelocity(WaterTriangle triangle)
    {
        triangle.Velocity = _rigidbody.GetRelativePointVelocity(_transform.TransformVector(triangle.Center));
        triangle.CosTheta = Vector3.Dot(triangle.Velocity.normalized, _transform.TransformVector(triangle.Normal));
    }

    private Vector3 BouancyForce(WaterTriangle triangle)
    {
        var forceVector = RhoWater * Physics.gravity.y * -triangle.DepthAtCenter * triangle.Area * _transform.TransformVector(triangle.Normal);

        forceVector.x = forceVector.z = 0f;
        if (_optionsController.BounancyDebugEnabled)
        {
            var color = forceVector.y > 0 ? Color.green : Color.red;
            Debug.DrawRay(_transform.TransformPoint(triangle.Center), forceVector * _optionsController.DebugUIScale, color);
        }
        return forceVector;
    }
    
    private Vector3 ViscousWaterResistanceForce(WaterTriangle triangle, float resistanceCoefficient)
    {   
        var normal = _transform.TransformVector(triangle.Normal);
        var velocity = triangle.Velocity;

        var velocityTangent = Vector3.Cross(normal, Vector3.Cross(velocity, normal) / normal.magnitude) / normal.magnitude;
        var tangentialDirection = velocityTangent.normalized * -1f;
        
        var tangentialVelocity = triangle.Velocity.magnitude * tangentialDirection;
        var viscousWaterResistanceForce = 0.5f * RhoWater * tangentialVelocity.magnitude * tangentialVelocity * triangle.Area * resistanceCoefficient;
        
        if (_optionsController.ViscosityDebugEnabled)
        {
            Debug.DrawRay(_transform.TransformPoint(triangle.Center), velocity * _optionsController.DebugUIScale, Color.blue);
            Debug.DrawRay(_transform.TransformPoint(triangle.Center), normal * _optionsController.DebugUIScale, Color.white);
            Debug.DrawRay(_transform.TransformPoint(triangle.Center), tangentialDirection * _optionsController.DebugUIScale, Color.black);
            Debug.DrawRay(_transform.TransformPoint(triangle.Center), tangentialVelocity * _optionsController.DebugUIScale, Color.gray);
            Debug.DrawRay(_transform.TransformPoint(triangle.Center), viscousWaterResistanceForce * _optionsController.DebugUIScale, Color.red);
        }

        return viscousWaterResistanceForce;
    }
    
    private Vector3 PressureDragForce(WaterTriangle triangle)
    {
        var velocity = triangle.Velocity.magnitude;
        velocity = velocity / _optionsController.PressureOptions.ReferenceSpeed;

        var pressureDragForce = new Vector3();

        if (triangle.CosTheta > 0f)
        {
            var pressureCoefficient1 = _optionsController.PressureOptions.PressureCoefficient1;
            var pressureCoefficient2 = _optionsController.PressureOptions.PressureCoefficient2;
            var pressureFalloff = _optionsController.PressureOptions.PressureFalloff;

            pressureDragForce = -(pressureCoefficient1 * velocity + pressureCoefficient2 * (velocity * velocity)) * triangle.Area * Mathf.Pow(triangle.CosTheta, pressureFalloff) * _transform.TransformVector(triangle.Normal);
        }
        else
        {
            var suctionCoefficient1 = _optionsController.PressureOptions.SuctionCoefficient1;
            var suctionCoefficient2 = _optionsController.PressureOptions.SuctionCoefficient2;
            var suctionFalloff = _optionsController.PressureOptions.SuctionFalloff;

            pressureDragForce = (suctionCoefficient1 * velocity + suctionCoefficient2 * (velocity * velocity)) * triangle.Area * Mathf.Pow(Mathf.Abs(triangle.CosTheta), suctionFalloff) * _transform.TransformVector(triangle.Normal);
        }

        if (_optionsController.PressureDebugEnabled)
		{
            Debug.DrawRay(_transform.TransformPoint(triangle.Center), pressureDragForce * _optionsController.DebugUIScale, Color.magenta);
        }

        return pressureDragForce;
    }

    private Vector3 SlammingForce(SlammingForce slammingData, WaterTriangle triangle, float boatArea, float boatMass)
    {
		//To capture the response of the fluid to sudden accelerations or penetrations

        //Add slamming if the normal is in the same direction as the velocity (the triangle is not receding from the water)
        //Also make sure thea area is not 0, which it sometimes is for some reason
        if (triangle.CosTheta < 0f || slammingData.OriginalArea <= 0f)
        {
            return Vector3.zero;
        }

        var deltaTime = Time.fixedDeltaTime;
        //Step 1 - Calculate acceleration
        //Volume of water swept per second
        Vector3 dV = slammingData.SubmergedArea * slammingData.Velocity;
        Vector3 dV_previous = slammingData.PreviousSubmergedArea * slammingData.PreviousVelocity;

        //Calculate the acceleration of the center point of the original triangle (not the current underwater triangle)
        //But the triangle the underwater triangle is a part of
        Vector3 accVec = (dV - dV_previous) / (slammingData.OriginalArea * deltaTime);

        //The magnitude of the acceleration
        float acc = accVec.magnitude;

		//Step 2 - Calculate slamming force
		// F = clamp(acc / acc_max, 0, 1)^p * cos(theta) * F_stop
		// p - power to ramp up slamming force - should be 2 or more

		// F_stop = m * v * (2A / S)
		// m - mass of the entire boat
		// v - velocity
		// A - this triangle's area
		// S - total surface area of the entire boat

        Vector3 F_stop = boatMass * triangle.Velocity * (2f * triangle.Area / boatArea);

        //float p = DebugPhysics.current.p;

        //float acc_max = DebugPhysics.current.acc_max;

        float p = _optionsController.SlammingOptions.RampingPower;

        float acc_max = (dV / (slammingData.OriginalArea * deltaTime)).magnitude;

        float slammingCheat = _optionsController.SlammingOptions.SlammingCheat;

        Vector3 slammingForce = Mathf.Pow(Mathf.Clamp01(acc / acc_max), p) * triangle.CosTheta * F_stop * slammingCheat;
        //Vector3 slammingForce = F_stop;

        //The force acts in the opposite direction
        slammingForce *= -1f;

        //slammingForce = CheckForceIsValid(slammingForce, "Slamming");
        if (_optionsController.SlammingDebugEnabled)
		{
            Debug.DrawRay(_transform.TransformPoint(triangle.Center), slammingForce * _optionsController.DebugUIScale, Color.yellow);
        }

        return slammingForce;   
        
    }

}
