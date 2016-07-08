using UnityEngine;

public class DualCircleScenario : Scenario
{
    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        Particle p1 = new Particle(1f);
        p1.Position = new Vector2(0, 3);
        a_ParticleSystem.AddParticle(p1);
        new CircularWireConstraint(p1, p1.Position + new Vector2(1, -1), a_ParticleSystem);
        new CircularWireConstraint(p1, p1.Position + new Vector2(-1, -1), a_ParticleSystem);

        a_ParticleSystem.AddForce(new GravityForce(.2f));
    }
}

