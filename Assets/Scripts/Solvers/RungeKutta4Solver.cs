using UnityEngine;

public class RungeKutta4Solver : Solver
{
    public override void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime)
    {
        int particleDimensions = a_ParticleSystem.ParticleDimensions; // = 4n
        float[] originals = new float[particleDimensions];
        float[] k1 = new float[particleDimensions];
        float[] k2 = new float[particleDimensions];
        float[] k3 = new float[particleDimensions];
        float[] k4 = new float[particleDimensions];
        float[] temp = new float[particleDimensions];
        a_ParticleSystem.ParticlesGetState(originals); //backup original locations and speeds
		float h = a_DeltaTime;//To keep consistent with slide notation

		//solve k1
        a_ParticleSystem.ParticleDerivative(k1); 
        ScaleVector(k1, h);

		//solve k2
		AddVectorsScale(originals,k1,temp,1,.5f);
        a_ParticleSystem.ParticlesSetState(temp);
		a_ParticleSystem.ParticleDerivative(k2); 
		ScaleVector(k2,h);

		//solve k3
		AddVectorsScale(originals,k2,temp,1,.5f);
        a_ParticleSystem.ParticlesSetState(temp);
		a_ParticleSystem.ParticleDerivative(k3); 
		ScaleVector(k3,h);

		//solve k4
		AddVectorsScale(originals,k3,temp,1,1);
        a_ParticleSystem.ParticlesSetState(temp);
		a_ParticleSystem.ParticleDerivative(k4); 
		ScaleVector(k4,h);

		//calculate final result
		AddVectorsScale(originals,k1,temp,1,1f/6f);
		AddVectorsScale(temp,k2,temp,1,1f/3f);
		AddVectorsScale(temp,k3,temp,1,1f/3f);
		AddVectorsScale(temp,k4,temp,1,1f/6f);

        a_ParticleSystem.ParticlesSetState(temp);
        a_ParticleSystem.Time += a_DeltaTime;
    }

	//a_VectorOut = a_ScaleA*a_VectorA + a_ScaleB*a_VectorB
	private void AddVectorsScale(float[] a_VectorA, float[] a_VectorB, float[] a_VectorOut, float a_ScaleA, float a_ScaleB)
    {
        if (!(a_VectorA.Length == a_VectorB.Length && a_VectorB.Length == a_VectorOut.Length))
        {
            throw new System.Exception("Input vectors do not have equal length!");
        }
        int vecLength = a_VectorA.Length;
        for (int i = 0; i < vecLength; ++i)
        {
            a_VectorOut[i] = a_VectorA[i]*a_ScaleA + a_VectorB[i]*a_ScaleB;
        }
    }
}
