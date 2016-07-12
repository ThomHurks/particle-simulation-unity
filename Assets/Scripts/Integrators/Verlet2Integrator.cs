public class Verlet2Integrator : Integrator
{
    private double[] temp1;
    private double[] temp2;
    private double[] temp3;
    private double[] originals;

    public override void Initialize(ParticleSystem a_ParticleSystem)
    {
        int particleDimensions = a_ParticleSystem.ParticleDimensions; // = 4n
        temp1 = new double[particleDimensions];
        temp2 = new double[particleDimensions];
        temp3 = new double[particleDimensions];
        originals = new double[particleDimensions];
    }

    public override void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime)
    {
        a_ParticleSystem.ParticlesGetState(originals); //backup original locations and speeds

        //perform half an euler step
        a_ParticleSystem.ParticleDerivative(temp1); // v,a
        ScaleVector(temp1, a_DeltaTime / 2);
        a_ParticleSystem.ParticlesGetState(temp2); 
        AddVectors(temp1, temp2, temp2);
        a_ParticleSystem.ParticlesSetState(temp2);

        //Do the midpoint step
        a_ParticleSystem.ParticleDerivative(temp1); //temp1 = v + .5, a+ .5
        AddVectorsScale(originals, temp1, temp2, 1f, a_DeltaTime); //temp2 = x(t+1),v(t+1)
        a_ParticleSystem.ParticlesSetState(temp2);

        a_ParticleSystem.ParticleDerivative(temp3); // v+1,a+1
        int n = a_ParticleSystem.ParticleDimensions / 4;
        for (int i = 0; i < n; i++)
        {
            int i4 = i * 4;
            temp2[i * 4 + 2] = originals[i4 + 2] + temp3[i4 + 2] * a_DeltaTime / 2f;
            temp2[i * 4 + 3] = originals[i4 + 3] + temp3[i4 + 3] * a_DeltaTime / 2f;
        }
        a_ParticleSystem.ParticlesSetState(temp2);
    }
}