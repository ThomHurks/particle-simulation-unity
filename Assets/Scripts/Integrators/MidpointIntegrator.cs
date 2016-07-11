public class MidpointIntegrator : Integrator
{
    private double[] temp1;
    private double[] temp2;
    private double[] originals;

    public override void Initialize(ParticleSystem a_ParticleSystem)
    {
        int particleDimensions = a_ParticleSystem.ParticleDimensions; // = 4n
        temp1 = new double[particleDimensions];
        temp2 = new double[particleDimensions];
        originals = new double[particleDimensions];
    }

    public override void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime)
    {
        a_ParticleSystem.ParticlesGetState(originals); //backup original locations and speeds

        //perform half an euler step
        a_ParticleSystem.ParticleDerivative(temp1); 
        ScaleVector(temp1, a_DeltaTime / 2);
        a_ParticleSystem.ParticlesGetState(temp2); 
        AddVectors(temp1, temp2, temp2);
        a_ParticleSystem.ParticlesSetState(temp2);

        //Do the midpoint step
        a_ParticleSystem.ParticleDerivative(temp1); 
        ScaleVector(temp1, a_DeltaTime); //whole timestep
        AddVectors(originals, temp1, temp2);
        a_ParticleSystem.ParticlesSetState(temp2);
    }
}
