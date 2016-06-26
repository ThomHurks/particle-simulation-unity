using UnityEngine;

public class TestScenario : Scenario
{
    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        Particle particle1 = new Particle(1f);
        particle1.Position = new Vector2(-2f, 0f);
        a_ParticleSystem.AddParticle(particle1);
        Particle particle2 = new Particle(1f);
        particle2.Position = new Vector2(0f, 0f);
        a_ParticleSystem.AddParticle(particle2);
        Particle particle3 = new Particle(1f);
        particle3.Position = new Vector2(4f, 4f);
        a_ParticleSystem.AddParticle(particle3);

        Force springForce1 = new HooksLawSpring(particle2, particle3, 2f, 10f, 0.1f);
        a_ParticleSystem.AddForce(springForce1);

        Force gravityForce = new GravityForce(1f);
        a_ParticleSystem.AddForce(gravityForce);

        Force dragForce = new ViscousDragForce(2f);
        a_ParticleSystem.AddForce(dragForce);

        new RodConstraint(particle1, particle2, 2f, a_ParticleSystem);
        new CircularWireConstraint(particle3, particle3.Position + Vector2.right, 1f, a_ParticleSystem);

        Particle particle4 = new Particle(1f);
        particle4.Position = new Vector2(-4f, 4f);
        a_ParticleSystem.AddParticle(particle4);
        //new FixedPointConstraint (particle4, m_ParticleSystem);
    }
}
