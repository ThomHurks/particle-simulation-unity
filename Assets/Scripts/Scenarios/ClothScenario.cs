using UnityEngine;

public class ClothScenario : Scenario
{
    private readonly bool m_WithCrossFibers;

    public ClothScenario(bool a_WithCrossFibers)
    {
        m_WithCrossFibers = a_WithCrossFibers;
    }

    public void CreateScenario(ParticleSystem a_ParticleSystem)
    {
        //Note; Without cross fibers appears to function better
        const int dim = 14;
        Vector2 bottomLeft = new Vector2(-5f, -5f);
        Vector2 topLeft = new Vector2(-5f, 5f);
        Vector2 bottomRight = new Vector2(5f, -5f);
        Vector2 offsetX = (bottomRight - bottomLeft) / dim;
        Vector2 offsetY = (topLeft - bottomLeft) / dim;
        float dist = offsetX.x;

        for (int i = 0; i <= dim; i++)
        {
            for (int j = 0; j <= dim; j++)
            {
                Particle p = new Particle(1f);
                p.Position = bottomLeft + offsetX * i + offsetY * j;
                a_ParticleSystem.AddParticle(p);
            }
        }
        const float ks = 10f;
        const float kd = 1f;
        float rest = dist / 1.05f;
        for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                int cur = j * (dim + 1) + i;
                int right = cur + dim + 1;
                int below = cur + 1;
                a_ParticleSystem.AddForce(new HooksLawSpring(a_ParticleSystem.Particles[cur], a_ParticleSystem.Particles[right], rest, ks, kd));
                a_ParticleSystem.AddForce(new HooksLawSpring(a_ParticleSystem.Particles[cur], a_ParticleSystem.Particles[below], rest, ks, kd));
            }
        }
        for (int i = 0; i < dim; i++)
        {
            int cur1 = (i + 1) * (dim + 1) - 1;
            int right = cur1 + dim + 1;
            a_ParticleSystem.AddForce(new HooksLawSpring(a_ParticleSystem.Particles[cur1], a_ParticleSystem.Particles[right], rest, ks, kd));

            int cur2 = i + dim * (dim + 1);
            int below = cur2 + 1;
            a_ParticleSystem.AddForce(new HooksLawSpring(a_ParticleSystem.Particles[cur2], a_ParticleSystem.Particles[below], rest, ks, kd));
        }
        if (m_WithCrossFibers)
        {
            float drest = rest * Mathf.Sqrt(2f);
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    int cur = j * (dim + 1) + i;
                    int rightbelow = cur + dim + 2;
                    a_ParticleSystem.AddForce(new HooksLawSpring(a_ParticleSystem.Particles[cur], a_ParticleSystem.Particles[rightbelow], drest, ks, kd));
                }
            }
            for (int i = 1; i <= dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    int cur = j * (dim + 1) + i;
                    int rightabove = cur + dim;
                    a_ParticleSystem.AddForce(new HooksLawSpring(a_ParticleSystem.Particles[cur], a_ParticleSystem.Particles[rightabove], drest, ks, kd));
                }
            }
        }
        a_ParticleSystem.AddForce(new GravityForce(Mathf.Pow(10, -2.5f)));
        for (int i = 0; i <= dim; i++)
        {
            new HLineConstraint(a_ParticleSystem.Particles[i * (dim + 1) + dim], a_ParticleSystem);
        }
    }
}
