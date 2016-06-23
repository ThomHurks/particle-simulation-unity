using System;
using UnityEngine;
using System.Collections;

public class EulerSolver : Solver
{
	public void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime)
	{
		int particleDimensions = a_ParticleSystem.ParticleDimensions;
		float[] temp1 = new float[particleDimensions];
		float[] temp2 = new float[particleDimensions];
		a_ParticleSystem.ParticleDerivative(temp1);
		ScaleVector(temp1, a_DeltaTime);
		a_ParticleSystem.ParticlesGetState(temp2);
		AddVectors(temp1, temp2, temp2);
		a_ParticleSystem.ParticlesSetState(temp2);
		a_ParticleSystem.Time += a_DeltaTime;
	}

	private void ScaleVector(float[] a_Vector, float a_ScaleFactor)
	{
		for (int i = 0; i < a_Vector.Length; ++i)
		{
			a_Vector[i] *= a_ScaleFactor;
		}
	}

	private void AddVectors(float[] a_VectorA, float[] a_VectorB, float[] a_VectorOut)
	{
		if (!(a_VectorA.Length == a_VectorB.Length && a_VectorB.Length == a_VectorOut.Length))
		{
			throw new Exception("Input vectors do not have equal length!");
		}
		int vecLength = a_VectorA.Length;
		for (int i = 0; i < vecLength; ++i)
		{
			a_VectorOut[i] = a_VectorA[i] + a_VectorB[i];
		}
	}

}
