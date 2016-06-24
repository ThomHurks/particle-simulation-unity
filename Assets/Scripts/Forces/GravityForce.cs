using UnityEngine;

public class GravityForce : Force
{
    private Vector2 m_Gravity = Vector2.down * 9.80665f;

    public GravityForce(float a_Multiplier)
    {
        m_Gravity *= a_Multiplier;
    }

    public void ApplyForce(ParticleSystem a_ParticleSystem)
    {
        int numParticles = a_ParticleSystem.Count;
        for (int i = 0; i < numParticles; ++i)
        {
            a_ParticleSystem.Particles[i].ForceAccumulator += m_Gravity;
        }
    }

    public void Draw()
    {
        return;
    }
}
