using UnityEngine;

public class HairScenario : Scenario
{
    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        const int internalParticles = 65; //amount of particles in hair is this + 2
        Vector2 start = new Vector2(-4f, 0f);
        Vector2 end = new Vector2(4f, 0f);

        Vector2 step = (end - start) / (internalParticles + 1);
        for (int i = 0; i <= internalParticles + 1; i++)
        {
            Particle p = new Particle(1f);
            p.Position = start + step * i;
            a_ParticleSystem.AddParticle(p);
        }

        //m_ParticleSystem.AddForce(new GravityForce(0.1f));
        float rest = step.magnitude;
        float ks = 0.05f;
        float kd = 0.01f;
        for (int i = 0; i <= internalParticles; i++)
        {
            a_ParticleSystem.AddForce(new HooksLawSpring(a_ParticleSystem.Particles[i], a_ParticleSystem.Particles[i + 1], rest, ks, kd));
        }
        ks = 0.01f;
        kd = 0.1f;
        const float totalInternalAngle = 180f * (internalParticles);
        const float angleDegrees = (totalInternalAngle / (internalParticles + 2));
        const float angleRadians = Mathf.PI * angleDegrees / 180f;
        for (int i = 1; i <= internalParticles; i++)
        {
            a_ParticleSystem.AddForce(new AngularSpringForce(a_ParticleSystem.Particles[i], a_ParticleSystem.Particles[i - 1], 
                    a_ParticleSystem.Particles[i + 1], angleRadians, ks, kd));
        }
    }
}

