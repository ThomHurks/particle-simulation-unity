using UnityEngine;

public sealed class Main : MonoBehaviour
{
    private ParticleSystem m_ParticleSystem;
    private Solver m_Solver;
    private static Material m_LineMaterial;

    static void CreateLineMaterial()
    {
        if (!m_LineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            m_LineMaterial = new Material(shader);
            m_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            m_LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            m_LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            m_LineMaterial.SetInt("_ZWrite", 0);
        }
    }

    void Awake()
    {
        m_ParticleSystem = new ParticleSystem();
        m_Solver = new EulerSolver();
        Particle particle1 = new Particle(0.1f);
        m_ParticleSystem.AddParticle(particle1);
        Particle particle2 = new Particle(0.1f);
        m_ParticleSystem.AddParticle(particle2);
        Force springForce1 = new HooksLawSpring(particle1, particle2, 5f, 0.1f, 0.1f);
        m_ParticleSystem.AddForce(springForce1);
        new CircularWireConstraint(particle1, particle1.Position, 5f, m_ParticleSystem);
    }

    void Update()
    {
        m_Solver.Step(m_ParticleSystem, Time.deltaTime);
    }

    void OnRenderObject()
    {
        CreateLineMaterial();
        // Apply the line material
        m_LineMaterial.SetPass(0);

        m_ParticleSystem.Draw();
    }
}
