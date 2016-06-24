using UnityEngine;
using System.Collections.Generic;

public sealed class Main : MonoBehaviour
{
    private ParticleSystem m_ParticleSystem;
    private Solver m_Solver;
    private static Material m_LineMaterial;
    private List<Transform> m_DebugGameObjects;
    private UnityEngine.UI.Dropdown m_SolverDropdown;

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
        const float constraintSpringConstant = 100f;
        const float constraintDampingConstant = 0.1f;
        const float solverEpsilon = 0.1f;
        const int solverSteps = 100;
        m_ParticleSystem = new ParticleSystem(solverEpsilon, solverSteps, constraintSpringConstant, constraintDampingConstant);
        m_Solver = new RungeKutta4Solver();

        CreateTestSimulation();
        //CreateClothSimulation();

        CreateDebugGameObjects();
    }

    void Start()
    {
        m_SolverDropdown = GameObject.Find("SolverDropdown").GetComponent<UnityEngine.UI.Dropdown>();
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

    public void OnSolverTypeChanged()
    {
        switch (m_SolverDropdown.value)
        {
            case 0:
                m_Solver = new EulerSolver();
                Debug.Log("Switched to Euler");
                break;
            case 1:
                m_Solver = new MidpointSolver();
                Debug.Log("Switched to Midpoint");
                break;
            case 2:
                m_Solver = new RungeKutta4Solver();
                Debug.Log("Switched to Runge Kutta 4th");
                break;
        }
    }

    private void CreateTestSimulation()
    {
		/*
		Particle particle1 = new Particle(1f);
		particle1.Position = new Vector2(-2f, 0f);
		m_ParticleSystem.AddParticle(particle1);
		Particle particle2 = new Particle(1f);
		particle2.Position = new Vector2(0f, 0f);
		m_ParticleSystem.AddParticle(particle2);
		Particle particle3 = new Particle(100f);
		particle3.Position = new Vector2(4f, 4f);
		m_ParticleSystem.AddParticle(particle3);

		Force springForce1 = new HooksLawSpring(particle2, particle3, 0f, 1f, 1f);
		m_ParticleSystem.AddForce(springForce1);
		Force gravityForce = new GravityForce(.01f);
		//m_ParticleSystem.AddForce(gravityForce);
		new RodConstraint(particle1,particle2,2,m_ParticleSystem);
/**/	


		Particle particle4 = new Particle(1f);
		particle4.Position = new Vector2(-4f, 4f);
		Particle particle5 = new Particle(1f);
		particle5.Position = new Vector2(-4f, 2f);
		m_ParticleSystem.AddParticle(particle5);
		m_ParticleSystem.AddParticle(particle4);

		Force springForce2 = new HooksLawSpring(particle4, particle5, 0f, 1f, 1f);
		m_ParticleSystem.AddForce(springForce2);
		//Particle particle6 = new Particle(1f);
		//particle6.Position = new Vector2(-4f, 4f);
		//m_ParticleSystem.AddParticle(particle6);
		new CircularWireConstraint(particle4, particle4.Position + Vector2.left, 1f, m_ParticleSystem);/**/


		Particle particle6 = new Particle(1f);
		particle6.Position = new Vector2(0f, 4f);
		Particle particle7 = new Particle(1f);
		particle7.Position = new Vector2(0f, 2f);
		m_ParticleSystem.AddParticle(particle6);
		m_ParticleSystem.AddParticle(particle7);

		Force springForce3 = new HooksLawSpring(particle6, particle7, 0f, 1f, 1f);
		m_ParticleSystem.AddForce(springForce3);
		//Particle particle6 = new Particle(1f);
		//particle6.Position = new Vector2(-4f, 4f);
		//m_ParticleSystem.AddParticle(particle6);
		new CircularWireConstraint(particle6, particle6.Position + Vector2.left, 1f, m_ParticleSystem);/**/

		Force springForce4 = new HooksLawSpring(particle5, particle7, 8f, 0.1f, 1f);
		m_ParticleSystem.AddForce(springForce4);

		Force gravityForce = new GravityForce(.1f);
		m_ParticleSystem.AddForce(gravityForce);


		Particle particle8 = new Particle(5f);
		particle8.Position = new Vector2(0f, 0f);
		m_ParticleSystem.AddParticle(particle8);

		Force springForce5 = new HooksLawSpring(particle5, particle8, 3f, 1f, 1f);
		m_ParticleSystem.AddForce(springForce5);

    }

    private void CreateClothSimulation(bool withCrossFibers = false)
    {
        //Note; Without cross fibers appears to function better
        const int dim = 14;
        Vector2 bottomLeft = new Vector2(-5f, -5f);
        Vector2 topLeft = new Vector2(-5f, 5f);
        Vector2 bottomRight = new Vector2(5f, -5f);
        Vector2 offsetX = (bottomRight - bottomLeft) / dim;
        Vector2 offsetY = (topLeft - bottomLeft) / dim;
        float dist = offsetX.x;
        Vector2 topRight = bottomLeft + (offsetX + offsetY) * dim;

        for (int i = 0; i <= dim; i++)
        {
            for (int j = 0; j <= dim; j++)
            {
                Particle p = new Particle(1f);
                p.Position = bottomLeft + offsetX * i + offsetY * j;
                m_ParticleSystem.AddParticle(p);
            }
        }
        const float ks = 1f;
        const float kd = 0.01f;
        float rest = dist / 1.05f;
        for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                int cur = j * (dim + 1) + i;
                int right = cur + dim + 1;
                int below = cur + 1;
                m_ParticleSystem.AddForce(new HooksLawSpring(m_ParticleSystem.Particles[cur], m_ParticleSystem.Particles[right], rest, ks, kd));
                m_ParticleSystem.AddForce(new HooksLawSpring(m_ParticleSystem.Particles[cur], m_ParticleSystem.Particles[below], rest, ks, kd));
            }
        }
        for (int i = 0; i < dim; i++)
        {
            int cur1 = (i + 1) * (dim + 1) - 1;
            int right = cur1 + dim + 1;
            m_ParticleSystem.AddForce(new HooksLawSpring(m_ParticleSystem.Particles[cur1], m_ParticleSystem.Particles[right], rest, ks, kd));

            int cur2 = i + dim * (dim + 1);
            int below = cur2 + 1;
            m_ParticleSystem.AddForce(new HooksLawSpring(m_ParticleSystem.Particles[cur2], m_ParticleSystem.Particles[below], rest, ks, kd));
        }
        if (withCrossFibers)
        {
            float drest = rest * Mathf.Sqrt(2f);
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    int cur = j * (dim + 1) + i;
                    int rightbelow = cur + dim + 2;
                    m_ParticleSystem.AddForce(new HooksLawSpring(m_ParticleSystem.Particles[cur], m_ParticleSystem.Particles[rightbelow], drest, ks, kd));
                }
            }
            for (int i = 1; i <= dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    int cur = j * (dim + 1) + i;
                    int rightabove = cur + dim;
                    m_ParticleSystem.AddForce(new HooksLawSpring(m_ParticleSystem.Particles[cur], m_ParticleSystem.Particles[rightabove], drest, ks, kd));
                }
            }
        }
        m_ParticleSystem.AddForce(new GravityForce(Mathf.Pow(10, -4.5f)));
        new FixedPointConstraint(m_ParticleSystem.Particles[dim], topLeft, m_ParticleSystem);
        new FixedPointConstraint(m_ParticleSystem.Particles[(dim + 1) * (dim + 1) - 1], topRight, m_ParticleSystem);
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
