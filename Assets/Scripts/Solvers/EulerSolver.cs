public class EulerSolver : Solver
{
    public override void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime)
    {
        int particleDimensions = a_ParticleSystem.ParticleDimensions;
        double[] temp1 = new double[particleDimensions];
        double[] temp2 = new double[particleDimensions];
        a_ParticleSystem.ParticleDerivative(temp1);
        ScaleVector(temp1, a_DeltaTime);
        a_ParticleSystem.ParticlesGetState(temp2);
        AddVectors(temp1, temp2, temp2);
        a_ParticleSystem.ParticlesSetState(temp2);
        a_ParticleSystem.Time += a_DeltaTime;
    }
}