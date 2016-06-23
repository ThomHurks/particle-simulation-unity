public class ViscousDragForce : Force
{
    private float m_CoefficientOfDrag;

    public ViscousDragForce(float a_CoefficientOfDrag)
    {
        m_CoefficientOfDrag = a_CoefficientOfDrag;
    }

    public void ApplyForce(ParticleSystem a_ParticleSystem)
    {
        int numParticles = a_ParticleSystem.Count;
        Particle curParticle;
        for (int i = 0; i < numParticles; ++i)
        {
            curParticle = a_ParticleSystem.Particles[i];
            curParticle.ForceAccumulator += -m_CoefficientOfDrag * curParticle.Velocity;
        }
    }

    public void Draw()
    {
        return;
    }
}
