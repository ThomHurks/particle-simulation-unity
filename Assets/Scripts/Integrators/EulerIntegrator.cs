public class EulerIntegrator : Integrator
{
    private double[] temp1;
    private double[] temp2;

    public override void Initialize(ParticleSystem a_ParticleSystem)
    {
        int particleDimensions = a_ParticleSystem.ParticleDimensions;
        temp1 = new double[particleDimensions];
        temp2 = new double[particleDimensions];
    }

    public override void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime)
    {
        a_ParticleSystem.ParticleDerivative(temp1);
        ScaleVector(temp1, a_DeltaTime);
        a_ParticleSystem.ParticlesGetState(temp2);
        AddVectors(temp1, temp2, temp2);
        a_ParticleSystem.ParticlesSetState(temp2);
    }
}