using UnityEngine;

public class ReversibleScenario : Scenario
{
    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        ViscousDragForce drag = new ViscousDragForce(0.1f);
        a_ParticleSystem.AddForce(drag);

        GravityForce gravity = new GravityForce(0.1f);
        a_ParticleSystem.AddForce(gravity);

        Particle p1 = new Particle(1f);
        p1.Position = new Vector2(-3f, 0f);
        a_ParticleSystem.AddParticle(p1);

        Particle p2 = new Particle(1f);
        p2.Position = new Vector2(3f, 0f);
        a_ParticleSystem.AddParticle(p2);

        HooksLawSpring spring1 = new HooksLawSpring(p1, p2, 3f, 10f, 10f);
        a_ParticleSystem.AddForce(spring1);
    }
}
