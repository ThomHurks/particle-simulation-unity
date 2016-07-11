using UnityEngine;

public class SlicedEllipseScenario :Scenario
{

    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        const float flatness = .95f;
        Vector2 a = new Vector2(4, -6);
        Vector2 a2 = new Vector2(a.y, -a.x);
        Vector2 b = new Vector2(-a.x / 2, -a.y / 2);

        Particle p1 = new Particle(1);
        p1.Position = b;
        a_ParticleSystem.AddParticle(p1);
        Vector2 f1 = p1.Position + (1 - flatness) * a;
        Vector2 f2 = p1.Position + flatness * a;
        new EllipticalWireConstraint(p1, f1, f2, a_ParticleSystem);

        Particle p2 = new Particle(1);
        p2.Position = b;
        a_ParticleSystem.AddParticle(p2);
        new AnyLineConstraint(p2, a, a_ParticleSystem);

        Particle p3 = new Particle(1);
        p3.Position = new Vector2(0, 0);
        a_ParticleSystem.AddParticle(p3);
        new AnyLineConstraint(p3, a2, a_ParticleSystem);

        a_ParticleSystem.AddForce(new HooksLawSpring(p1, p2, 0, 5, 1));
        a_ParticleSystem.AddForce(new HooksLawSpring(p1, p3, 0, 1, 1));

        Particle q1 = new Particle(1);
        Particle q2 = new Particle(1);
        q1.Position = f1;
        q2.Position = f2;
        a_ParticleSystem.AddParticle(q1);
        a_ParticleSystem.AddParticle(q2);
        new FixedPointConstraint(q1, a_ParticleSystem);
        new FixedPointConstraint(q2, a_ParticleSystem);

        Particle p4 = new Particle(1);
        p4.Position = p1.Position + a;
        a_ParticleSystem.AddParticle(p4);
        new EllipticalWireConstraint(p4, f1, f2, a_ParticleSystem);
        a_ParticleSystem.AddForce(new HooksLawSpring(q1, p4, 0, 0, 0));
        a_ParticleSystem.AddForce(new HooksLawSpring(q2, p4, 0, 0, 0));

    
    }


}