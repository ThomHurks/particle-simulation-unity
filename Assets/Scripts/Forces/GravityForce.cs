using UnityEngine;

public class GravityForce : Force
{
    private Vector2 m_Gravity = Vector2.down * 9.80665f / 10f;

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
