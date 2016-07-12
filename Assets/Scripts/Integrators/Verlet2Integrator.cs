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
        int n = a_ParticleSystem.Particles.Count;
        float dt = a_DeltaTime;
        a_ParticleSystem.ParticlesGetState(originals); //backup original locations and speeds

        a_ParticleSystem.ParticleDerivative(temp1); // v,a
        for (int i = 0; i < n; i++)
        { 
            int i4 = i * 4;
            //+0 to make it more readable, compiler will fix it :)
            temp2[i4 + 0] = originals[i4 + 0] + temp1[i4 + 0] * dt / 4; // x_{i+1} = x_i + v_i dt + a_i dtsq
            temp2[i4 + 1] = originals[i4 + 1] + temp1[i4 + 1] * dt / 4;
            temp2[i4 + 2] = originals[i4 + 2] + temp1[i4 + 2] * dt / 4;
            temp2[i4 + 3] = originals[i4 + 3] + temp1[i4 + 0] * dt / 4;
        }
        a_ParticleSystem.ParticlesSetState(temp2); // 1/4 euler step

        a_ParticleSystem.ParticleDerivative(temp1); // v,a
        for (int i = 0; i < n; i++)
        { 
            int i4 = i * 4;
            //+0 to make it more readable, compiler will fix it :)
            temp2[i4 + 0] = originals[i4 + 0] + originals[i4 + 2] * dt + temp1[i4 + 2] * dt * dt / 2f; // x_{i+1} = x_i + v_i dt + a_i dtsq
            temp2[i4 + 1] = originals[i4 + 1] + originals[i4 + 3] * dt + temp1[i4 + 3] * dt * dt / 2f;
            temp2[i4 + 2] = originals[i4 + 2];
            temp2[i4 + 3] = originals[i4 + 3];
        }
        a_ParticleSystem.ParticlesSetState(temp2); // same first step as normal verlet, except this time based on midpoint

        a_ParticleSystem.ParticleDerivative(temp3); // v+1,a+1
        for (int i = 0; i < n; i++)
        {
            int i4 = i * 4;
            temp2[i4 + 2] = originals[i4 + 2] + (temp3[i4 + 2] + temp1[i4 + 2]) * a_DeltaTime / 2f;
            temp2[i4 + 3] = originals[i4 + 3] + (temp3[i4 + 3] + temp1[i4 + 3]) * a_DeltaTime / 2f;
        }
        a_ParticleSystem.ParticlesSetState(temp2);
    }


}