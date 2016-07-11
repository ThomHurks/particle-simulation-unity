public class RungeKutta4Integrator : Integrator
{
    public override void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime)
    {
        int particleDimensions = a_ParticleSystem.ParticleDimensions; // = 4n
        double[] originals = new double[particleDimensions];
        double[] k1 = new double[particleDimensions];
        double[] k2 = new double[particleDimensions];
        double[] k3 = new double[particleDimensions];
        double[] k4 = new double[particleDimensions];
        double[] temp = new double[particleDimensions];
        a_ParticleSystem.ParticlesGetState(originals); //backup original locations and speeds
        double h = a_DeltaTime;//To keep consistent with slide notation

        //solve k1
        a_ParticleSystem.ParticleDerivative(k1); 
        ScaleVector(k1, h);

        //solve k2
        AddVectorsScale(originals, k1, temp, 1, .5f);
        a_ParticleSystem.ParticlesSetState(temp);
        a_ParticleSystem.ParticleDerivative(k2); 
        ScaleVector(k2, h);

        //solve k3
        AddVectorsScale(originals, k2, temp, 1, .5f);
        a_ParticleSystem.ParticlesSetState(temp);
        a_ParticleSystem.ParticleDerivative(k3); 
        ScaleVector(k3, h);

        //solve k4
        AddVectorsScale(originals, k3, temp, 1, 1);
        a_ParticleSystem.ParticlesSetState(temp);
        a_ParticleSystem.ParticleDerivative(k4); 
        ScaleVector(k4, h);

        //calculate final result
        AddVectorsScale(originals, k1, temp, 1, 1f / 6f);
        AddVectorsScale(temp, k2, temp, 1, 1f / 3f);
        AddVectorsScale(temp, k3, temp, 1, 1f / 3f);
        AddVectorsScale(temp, k4, temp, 1, 1f / 6f);

        a_ParticleSystem.ParticlesSetState(temp);
    }
}
