public class LeapfrogIntegrator : Integrator
{
    private double[] temp;
    private double[] originals;
    private int m_StepCount = 0;

    public override void Initialize(ParticleSystem a_ParticleSystem)
    {
        int particleDimensions = a_ParticleSystem.ParticleDimensions; // = 4n
        temp = new double[particleDimensions];
        originals = new double[particleDimensions];
    }

    public override void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime)
    {
        a_ParticleSystem.ParticlesGetState(originals);

        // Perform half an Euler step for the velocities only, on the first step.
        if (m_StepCount == 0)
        {
            ++m_StepCount;
            a_ParticleSystem.ParticleDerivative(temp); // v,a
            ScaleAndAddVelocities(temp, originals, originals, a_DeltaTime / 2);
            a_ParticleSystem.ParticlesSetState(originals);
        }

        // Do the leapfrog step.
        AddScaledVelocitiesToPositions(originals, a_DeltaTime);
        a_ParticleSystem.ParticlesSetState(originals);
        a_ParticleSystem.ParticleDerivative(temp);
        ScaleAndAddVelocities(temp, originals, originals, a_DeltaTime);
        a_ParticleSystem.ParticlesSetState(originals);
    }

    private void ScaleAndAddVelocities(double[] a_VectorA, double[] a_VectorB, double[] a_VectorOut,
                                       double a_ScaleFactor)
    {
        if (!(a_VectorA.Length == a_VectorB.Length && a_VectorB.Length == a_VectorOut.Length))
        {
            throw new System.Exception("Input vectors do not have equal length!");
        }
        int vecLength = a_VectorA.Length / 4;
        for (int i = 0; i < vecLength; ++i)
        {
            int velIndex = (i << 2) + 2;//i*4+2
            a_VectorOut[velIndex] = (a_VectorA[velIndex] * a_ScaleFactor) + a_VectorB[velIndex];
            a_VectorOut[velIndex + 1] = (a_VectorA[velIndex + 1] * a_ScaleFactor) + a_VectorB[velIndex + 1];
        }
    }

    private void AddScaledVelocitiesToPositions(double[] a_Vector, double a_ScaleFactor)
    {
        int vecLength = a_Vector.Length / 4;
        for (int i = 0; i < vecLength; ++i)
        {
            int posIndex = i << 2;
            int velIndex = posIndex + 2;
            a_Vector[posIndex] += (a_Vector[velIndex] * a_ScaleFactor);
            a_Vector[posIndex += 1] += (a_Vector[velIndex + 1] * a_ScaleFactor);
        }
    }
}