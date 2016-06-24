using UnityEngine;
using System.Collections.Generic;

public sealed class Main : MonoBehaviour
{
    private ParticleSystem m_ParticleSystem;
    private Solver m_Solver;
    private static Material m_LineMaterial;
    private List<Transform> m_DebugGameObjects;

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
        const float constraintSpringConstant = 10f;
        const float constraintDampingConstant = 0.1f;
        const float solverEpsilon = 0.1f;
        const int solverSteps = 100;
        m_ParticleSystem = new ParticleSystem(solverEpsilon, solverSteps, constraintSpringConstant, constraintDampingConstant);
        m_Solver = new RungeKutta4Solver();
        Particle particle1 = new Particle(0.1f);
        particle1.Position = new Vector2(-2f, 0f);
        m_ParticleSystem.AddParticle(particle1);
        Particle particle2 = new Particle(0.1f);
        particle2.Position = new Vector2(2f, 0f);
        m_ParticleSystem.AddParticle(particle2);
        Force springForce1 = new HooksLawSpring(particle1, particle2, 2f, 0.1f, 0.1f);
        m_ParticleSystem.AddForce(springForce1);
        Force gravityForce = new GravityForce();
        m_ParticleSystem.AddForce(gravityForce);
        new CircularWireConstraint(particle1, particle1.Position + Vector2.left, 1f, m_ParticleSystem);
        CreateDebugGameObjects();
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

    void LateUpdate()
    {
        UpdateDebugGameObjects();
    }

    private void CreateDebugGameObjects()
    {
        int numParticles = m_ParticleSystem.Particles.Count;
        m_DebugGameObjects = new List<Transform>(numParticles);
        for (int i = 0; i < numParticles; ++i)
        {
            GameObject gob = new GameObject("Particle " + i);
            Transform gobTf = gob.transform;
            gobTf.position = m_ParticleSystem.Particles[i].Position;
            m_DebugGameObjects.Add(gobTf);
        }
    }

    private void UpdateDebugGameObjects()
    {
        for (int i = 0; i < m_ParticleSystem.Particles.Count; ++i)
        {
            m_DebugGameObjects[i].position = m_ParticleSystem.Particles[i].Position;
        }
    }
}
