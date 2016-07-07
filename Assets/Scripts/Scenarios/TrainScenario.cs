using UnityEngine;

public class TrainScenario : Scenario
{
    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        const float particleMass = 1f;

        Particle p1 = new Particle(particleMass);
        p1.Position = new Vector2(-3, 0);
        a_ParticleSystem.AddParticle(p1);
        new CircularWireConstraint(p1, new Vector2(-2, 0), a_ParticleSystem);

        Particle p3 = new Particle(particleMass);
        p3.Position = p1.Position + new Vector2(0.5f, 0f);
        a_ParticleSystem.AddParticle(p3);
        HooksLawSpring s1 = new HooksLawSpring(p1, p3, 0.5f, 100f, 10f);
        a_ParticleSystem.AddForce(s1);

        Particle p2 = new Particle(particleMass);
        p2.Position = new Vector2(2, 0);
        a_ParticleSystem.AddParticle(p2);
        new CircularWireConstraint(p2, new Vector2(3, 0), a_ParticleSystem);

        Particle p4 = new Particle(particleMass);
        p4.Position = p2.Position + new Vector2(0.5f, 0f);
        a_ParticleSystem.AddParticle(p4);
        HooksLawSpring s2 = new HooksLawSpring(p2, p4, 0.5f, 100f, 10f);
        a_ParticleSystem.AddForce(s2);

        //new RodConstraint(p3, p4, a_ParticleSystem);

        Particle p5 = new Particle(particleMass);
        p5.Position = new Vector2(1, 0);
        a_ParticleSystem.AddParticle(p5);
        new UnitCircularWireConstraint(p5, a_ParticleSystem);
    }
}
