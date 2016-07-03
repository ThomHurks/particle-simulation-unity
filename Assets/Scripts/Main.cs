using UnityEngine;
using System.Collections.Generic;
using System;

public sealed class Main : MonoBehaviour
{
    private ParticleSystem m_ParticleSystem;
    private Solver m_Solver;
    private Scenario m_Scenario;
    private static Material m_LineMaterial;
    private List<Transform> m_DebugGameObjects;
    private UnityEngine.UI.Dropdown m_SolverDropdown;
    private UnityEngine.UI.Dropdown m_ScenarioDropdown;
    private const float m_ParticleSelectThreshold = 0.2f;
    private const float m_MouseSelectRestLength = 0f;
    private const float m_MouseSelectSpringConstant = 10f;
    private const float m_MouseSelectDampingConstant = 1f;
    private bool m_HasMouseSelection = false;
    private MouseSpringForce m_CurrentMouseForce;

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
        const float constraintDampingConstant = 0f;
        const float solverEpsilon = 0.1f;
        const int solverSteps = 50;
        m_ParticleSystem = new ParticleSystem(solverEpsilon, solverSteps, constraintSpringConstant, constraintDampingConstant);
        m_Solver = new RungeKutta4Solver();
        m_Scenario = new TestScenario();
        m_Scenario.CreateScenario(m_ParticleSystem);
        SetupDebugGameObjects();
    }

    void Start()
    {
        m_SolverDropdown = GameObject.Find("SolverDropdown").GetComponent<UnityEngine.UI.Dropdown>();
        if (m_Solver is EulerSolver)
        {
            m_SolverDropdown.value = 0;
        }
        else if (m_Solver is MidpointSolver)
        {
            m_SolverDropdown.value = 1;
        }
        else if (m_Solver is RungeKutta4Solver)
        {
            m_SolverDropdown.value = 2;
        }
        m_SolverDropdown.RefreshShownValue();

        m_ScenarioDropdown = GameObject.Find("ScenarioDropdown").GetComponent<UnityEngine.UI.Dropdown>();
        if (m_Scenario is TestScenario)
        {
            m_ScenarioDropdown.value = 0;
        }
        else if (m_Scenario is HairScenario)
        {
            m_ScenarioDropdown.value = 1;
        }
        else if (m_Scenario is ClothScenario)
        {
            m_ScenarioDropdown.value = 2;
        }
        m_ScenarioDropdown.RefreshShownValue();
    }

    void Update()
    {
        HandleMouseInteraction();
    }

    void FixedUpdate()
    {
        try
        {
            m_Solver.Step(m_ParticleSystem, Time.fixedDeltaTime);
        }
		catch(Exception e)
        {
            Debug.LogError("We encountered an error, so we will reset the scenario.");
			Debug.LogError (e.Message +"\n" + e.StackTrace);
            Reset();
        }
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

    private void Reset()
    {
        m_HasMouseSelection = false;
        m_CurrentMouseForce = null;
        m_ParticleSystem.Clear();
        m_Scenario.CreateScenario(m_ParticleSystem);
        SetupDebugGameObjects();
    }

    private void HandleMouseInteraction()
    {
        if (!m_HasMouseSelection && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos = new Vector2(mousePos3D.x, mousePos3D.y);

            int numParticles = m_ParticleSystem.Particles.Count;
            Particle closestParticle = null;
            float particleDistanceSqr = float.MaxValue;
            Particle curParticle = null;
            for (int i = 0; i < numParticles; ++i)
            {
                curParticle = m_ParticleSystem.Particles[i];
                float curDistanceSqr = (mousePos - curParticle.Position).sqrMagnitude;
                if (curDistanceSqr < particleDistanceSqr)
                {
                    closestParticle = curParticle;
                    particleDistanceSqr = curDistanceSqr;
                }
            }
            if (closestParticle != null && particleDistanceSqr < (m_ParticleSelectThreshold * m_ParticleSelectThreshold))
            {
                m_CurrentMouseForce = new MouseSpringForce(closestParticle, m_MouseSelectRestLength,
                    m_MouseSelectSpringConstant, m_MouseSelectDampingConstant);
                m_ParticleSystem.AddForce(m_CurrentMouseForce);
                m_HasMouseSelection = true;
            }
        }
        else if (m_HasMouseSelection && Input.GetMouseButtonDown(1))
        {
            m_ParticleSystem.RemoveForce(m_CurrentMouseForce);
            m_CurrentMouseForce = null;
            m_HasMouseSelection = false;
        }
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

    public void OnScenarioTypeChanged()
    {
        switch (m_ScenarioDropdown.value)
        {
            case 0:
                m_Scenario = new TestScenario();
                Debug.Log("Switched to test scenario");
                break;
            case 1:
                m_Scenario = new HairScenario();
                Debug.Log("Switched to hair scenario");
                break;
            case 2:
                m_Scenario = new ClothScenario(true);
                Debug.Log("Switched to cloth scenario");
                break;
        }
        Reset();
    }

    private void SetupDebugGameObjects()
    {
        int numParticles = m_ParticleSystem.Particles.Count;
        if (m_DebugGameObjects == null)
        {
            m_DebugGameObjects = new List<Transform>(numParticles);
        }
        int numObjects = m_DebugGameObjects.Count;
        int objectsToAdd = numParticles - numObjects;
        int objectsToRemove = numObjects - numParticles;
        if (objectsToAdd > 0)
        {
            for (int i = 0; i < objectsToAdd; ++i)
            {
                GameObject gob = new GameObject("Particle " + (numObjects + i));
                m_DebugGameObjects.Add(gob.transform);
            }
        }
        else if (objectsToRemove > 0)
        {
            for (int i = 0; i < objectsToRemove; ++i)
            {
                int last = m_DebugGameObjects.Count - 1;
                Transform tf = m_DebugGameObjects[last];
                GameObject.Destroy(tf.gameObject);
                m_DebugGameObjects.RemoveAt(last);
            }
        }
        m_DebugGameObjects.Capacity = numParticles;

        UpdateDebugGameObjects();
    }

    private void UpdateDebugGameObjects()
    {
        int numParticles = m_ParticleSystem.Particles.Count;
        for (int i = 0; i < numParticles; ++i)
        {
            m_DebugGameObjects[i].position = m_ParticleSystem.Particles[i].Position;
        }

    }
}
