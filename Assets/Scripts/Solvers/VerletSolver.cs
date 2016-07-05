using System;

public class VerletSolver : Solver
{

	public override void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime)
	{
		int particleDimensions = a_ParticleSystem.ParticleDimensions; // = 4n
		float[] temp1 = new float[particleDimensions];
		float[] temp2 = new float[particleDimensions];
		float[] temp3 = new float[particleDimensions];
		float[] originals = new float[particleDimensions];
		a_ParticleSystem.ParticlesGetState(originals); //backup original locations and speeds

		//perform half an euler step
		a_ParticleSystem.ParticleDerivative(temp1); // v,a
		ScaleVector(temp1, a_DeltaTime / 2);
		a_ParticleSystem.ParticlesGetState(temp2); 
		AddVectors(temp1, temp2, temp2);
		a_ParticleSystem.ParticlesSetState(temp2);

		//Do the midpoint step
		a_ParticleSystem.ParticleDerivative(temp1); //temp1 = v + .5, a+ .5
		AddVectorsScale(originals, temp1, temp2,1f,a_DeltaTime); //temp2 = x(t+1),v(t+1)
		a_ParticleSystem.ParticlesSetState(temp2);

		a_ParticleSystem.ParticleDerivative(temp3); // v+1,a+1
		int n = particleDimensions/4;
		for (int i = 0; i < n; i++) {
			temp2 [i*4+2] = temp1 [i*4] + temp3 [i*4+2] * a_DeltaTime / 2f;
		}
		a_ParticleSystem.ParticlesSetState (temp2);
		a_ParticleSystem.Time += a_DeltaTime;
	}

	private void AddVectorsScale(float[] a_VectorA, float[] a_VectorB, float[] a_VectorOut, float a_ScaleA, float a_ScaleB)
	{
		if (!(a_VectorA.Length == a_VectorB.Length && a_VectorB.Length == a_VectorOut.Length))
		{
			throw new System.Exception("Input vectors do not have equal length!");
		}
		int vecLength = a_VectorA.Length;
		for (int i = 0; i < vecLength; ++i)
		{
			a_VectorOut[i] = a_VectorA[i] * a_ScaleA + a_VectorB[i] * a_ScaleB;
		}
	}
}


