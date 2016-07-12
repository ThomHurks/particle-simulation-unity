public class MidpointVerletIntegrator : Integrator
{
    private double[] derivativeI;
    private double[] stateStorage;
    private double[] vIPlusOneHalve;
    private double[] derivativeIPlusOne;
    private double[] derivativeIPlusOneFourth;
    private double[] originals;

    public override void Initialize(ParticleSystem a_ParticleSystem)
    {
        int particleDimensions = a_ParticleSystem.ParticleDimensions; // = 4n
        stateStorage = new double[particleDimensions];
        vIPlusOneHalve = new double[particleDimensions];
        derivativeI = new double[particleDimensions];
        derivativeIPlusOne = new double[particleDimensions];
        derivativeIPlusOneFourth = new double[particleDimensions];
        originals = new double[particleDimensions];
    }


    public override void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime)
    {
        int n = a_ParticleSystem.Particles.Count;
        float dt = a_DeltaTime;
        a_ParticleSystem.ParticlesGetState(originals); //backup original locations and speeds

        a_ParticleSystem.ParticleDerivative(derivativeI); // v,a
        for (int i = 0; i < n; i++)
        { 
            int i4 = i * 4;
            //+0 to make it more readable, compiler will fix it :)
            stateStorage[i4 + 0] = originals[i4 + 0] + derivativeI[i4 + 0] * dt / 4; // x_{i+1} = x_i + v_i dt + a_i dtsq
            stateStorage[i4 + 1] = originals[i4 + 1] + derivativeI[i4 + 1] * dt / 4;
            stateStorage[i4 + 2] = originals[i4 + 2] + derivativeI[i4 + 2] * dt / 4;
            stateStorage[i4 + 3] = originals[i4 + 3] + derivativeI[i4 + 0] * dt / 4;
        }
        a_ParticleSystem.ParticlesSetState(stateStorage); // 1/4 euler step

        a_ParticleSystem.ParticleDerivative(derivativeIPlusOneFourth); // v,a
        for (int i = 0; i < n; i++)
        { 
            int i4 = i * 4;
            //+0 to make it more readable, compiler will fix it :)
            vIPlusOneHalve[i4 + 2] = originals[i4 + 2] + derivativeIPlusOneFourth[i4 + 2] * dt / 2f;
            vIPlusOneHalve[i4 + 3] = originals[i4 + 3] + derivativeIPlusOneFourth[i4 + 3] * dt / 2f;
            stateStorage[i4 + 0] = originals[i4 + 0] + vIPlusOneHalve[i4 + 2] * dt; // x_{i+1} = x_i + v_i dt + a_i dtsq
            stateStorage[i4 + 1] = originals[i4 + 1] + vIPlusOneHalve[i4 + 3] * dt;
            //stateStorage[i4 + 0] = originals[i4 + 0] + originals[i4 + 2] * dt + derivativeIPlusOneFourth[i4 + 2] * dt * dt / 2f; // x_{i+1} = x_i + v_i dt + a_i dtsq
            //stateStorage[i4 + 1] = originals[i4 + 1] + originals[i4 + 3] * dt + derivativeIPlusOneFourth[i4 + 3] * dt * dt / 2f;
            stateStorage[i4 + 2] = originals[i4 + 2];
            stateStorage[i4 + 3] = originals[i4 + 3];
        }
        a_ParticleSystem.ParticlesSetState(stateStorage); // same first step as normal verlet, except this time based on midpoint

        a_ParticleSystem.ParticleDerivative(derivativeIPlusOne); // v+1,a+1
        for (int i = 0; i < n; i++)
        {
            int i4 = i * 4;
            stateStorage[i4 + 2] = vIPlusOneHalve[i4 + 2] + derivativeIPlusOne[i4 + 2] * a_DeltaTime / 2f;
            stateStorage[i4 + 3] = vIPlusOneHalve[i4 + 3] + derivativeIPlusOne[i4 + 3] * a_DeltaTime / 2f;
            //stateStorage[i4 + 2] = originals[i4 + 2] + (derivativeIPlusOne[i4 + 2] + derivativeI[i4 + 2]) * a_DeltaTime / 2f;
            //stateStorage[i4 + 3] = originals[i4 + 3] + (derivativeIPlusOne[i4 + 3] + derivativeI[i4 + 3]) * a_DeltaTime / 2f;
        }
        a_ParticleSystem.ParticlesSetState(stateStorage);
    }


}