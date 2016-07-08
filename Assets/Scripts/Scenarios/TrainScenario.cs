using UnityEngine;

public class TrainScenario : Scenario
{
    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        const float particleMass = 1f;

        Particle p1 = new Particle(particleMass);
        p1.Position = new Vector2(-3f, 3.5f);
        a_ParticleSystem.AddParticle(p1);
        new CircularWireConstraint(p1, new Vector2(-2f, 3.5f), a_ParticleSystem);

        Particle p3 = new Particle(particleMass);
        p3.Position = p1.Position + new Vector2(0.5f, 0f);
        a_ParticleSystem.AddParticle(p3);
        new RodConstraint(p1, p3, a_ParticleSystem);

        Particle p2 = new Particle(particleMass);
        p2.Position = new Vector2(2f, 3.5f);
        a_ParticleSystem.AddParticle(p2);
        new CircularWireConstraint(p2, new Vector2(3f, 3.5f), a_ParticleSystem);

        Particle p4 = new Particle(particleMass);
        p4.Position = p2.Position + new Vector2(0.5f, 0f);
        a_ParticleSystem.AddParticle(p4);
        new RodConstraint(p2, p4, a_ParticleSystem);

        new RodConstraint(p3, p4, a_ParticleSystem);

        // With springs
        const float springConstant = 75f;
        const float dampingConstant = 40f;

        Particle p5 = new Particle(particleMass);
        p5.Position = new Vector2(-3f, 1);
        a_ParticleSystem.AddParticle(p5);
        new CircularWireConstraint(p5, new Vector2(-2f, 1), a_ParticleSystem);

        Particle p6 = new Particle(particleMass);
        p6.Position = p5.Position + new Vector2(0.5f, 1f);
        a_ParticleSystem.AddParticle(p6);
        HooksLawSpring s1 = new HooksLawSpring(p5, p6, 0.5f, springConstant, dampingConstant);
        a_ParticleSystem.AddForce(s1);

        Particle p7 = new Particle(particleMass);
        p7.Position = new Vector2(2f, 1);
        a_ParticleSystem.AddParticle(p7);
        new CircularWireConstraint(p7, new Vector2(3f, 1), a_ParticleSystem);

        Particle p8 = new Particle(particleMass);
        p8.Position = p7.Position + new Vector2(0.5f, 1f);
        a_ParticleSystem.AddParticle(p8);
        HooksLawSpring s2 = new HooksLawSpring(p7, p8, 0.5f, springConstant, dampingConstant);
        a_ParticleSystem.AddForce(s2);

        new RodConstraint(p6, p8, a_ParticleSystem);



        Particle q1 = new Particle(particleMass);
        q1.Position = new Vector2(-3f, -1.5f);
        a_ParticleSystem.AddParticle(q1);
        new CircularWireConstraint(q1, new Vector2(-2f, -1.5f), a_ParticleSystem);


        Particle q2 = new Particle(particleMass);
        q2.Position = new Vector2(2f, -1.5f);
        a_ParticleSystem.AddParticle(q2);
        new CircularWireConstraint(q2, new Vector2(3f, -1.5f), a_ParticleSystem);
        new RodConstraint(q1, q2, a_ParticleSystem);


        Particle a1 = new Particle(1);
        Particle a2 = new Particle(1);
        Particle a3 = new Particle(1);
        Particle a4 = new Particle(1);
        a1.Position = new Vector2(-4, -3);
        a2.Position = new Vector2(0, -3);
        a3.Position = new Vector2(0, -5);
        a4.Position = new Vector2(4, -5);
        a_ParticleSystem.AddParticle(a1);
        a_ParticleSystem.AddParticle(a2);
        a_ParticleSystem.AddParticle(a3);
        a_ParticleSystem.AddParticle(a4);
        new CircularWireConstraint(a1, a1.Position + Vector2.down, a_ParticleSystem);
        new CircularWireConstraint(a2, a2.Position + Vector2.down, a_ParticleSystem);
        new CircularWireConstraint(a3, a3.Position + Vector2.up, a_ParticleSystem);
        new CircularWireConstraint(a4, a4.Position + Vector2.up, a_ParticleSystem);
        new RodConstraint(a1, a2, a_ParticleSystem);
        new RodConstraint(a2, a3, a_ParticleSystem);
        new RodConstraint(a3, a4, a_ParticleSystem);
    }
}
