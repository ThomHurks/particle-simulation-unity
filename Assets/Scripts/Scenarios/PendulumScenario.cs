using UnityEngine;


public class PendulumScenario : Scenario
{
    private static readonly float y = 3;

    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        int max = 10;
        int max1 = max - 1;
        for (int i = 0; i < max; i++)
        {
            int y = i >= max / 2 ? 0 : 4;
            float x = i >= max / 2 ? 3 * (i - max / 2) - 6 : 3 * i - 6;
            createPendulum(a_ParticleSystem, x, y, .5f, i + 2);
        }
        Force f = new GravityForce(1f);
        a_ParticleSystem.AddForce(f);
        f = new ViscousDragForce(.05f);
        a_ParticleSystem.AddForce(f);
    }

    void createPendulum(ParticleSystem a_ParticleSystem, float x, float y, float length, int n)
    {
        Particle prev = new Particle(1f);
        prev.Position = new Vector2(x, y);
        a_ParticleSystem.AddParticle(prev);
        new FixedPointConstraint(prev, a_ParticleSystem);
        for (int i = 1; i < n; i++)
        {
            Particle next = new Particle(1f);
            next.Position = new Vector2(x + length * i, y);
            a_ParticleSystem.AddParticle(next);
            new RodConstraint(prev, next, a_ParticleSystem);
            prev = next;
        }
    }
}




