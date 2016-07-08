using UnityEngine;
using System.Collections.Generic;
using System;

public sealed class Main : MonoBehaviour
{
    private ParticleSystem m_ParticleSystem;
    private Integrator m_Integrator;
    private Scenario m_Scenario;
    private static Material m_LineMaterial;
    private List<Transform> m_DebugGameObjects;
    private UnityEngine.UI.Dropdown m_IntegratorDropdown;
    private UnityEngine.UI.Dropdown m_ScenarioDropdown;
    private UnityEngine.UI.Dropdown m_CircleDropdown;
    private UnityEngine.UI.Dropdown m_RodDropdown;
    private UnityEngine.UI.Dropdown m_AngleDropdown;
    private UnityEngine.UI.Dropdown m_SolverDropdown;
    private const float m_ParticleSelectThreshold = 0.2f;
    private const float m_MouseSelectRestLength = 0f;
    private const float m_MouseSelectSpringConstant = 20f;
    private const float m_MouseSelectDampingConstant = 2f;
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
        const float constraintSpringConstant = 100f;
        const float constraintDampingConstant = 10f;
        double solverEpsilon = Math.Pow(10, -2);// Having this too small causes issues, since the solver works by squaring.

        const int solverSteps = 10;
        m_ParticleSystem = new ParticleSystem(new ConjGradSolver2(), solverEpsilon, solverSteps, constraintSpringConstant, constraintDampingConstant);
        m_Integrator = new RungeKutta4Integrator();
        m_Scenario = new DualCircleScenario();
        m_Scenario.CreateScenario(m_ParticleSystem);
        SetupDebugGameObjects();
    }

    void Start()
    {
        Test();
        m_IntegratorDropdown = GameObject.Find("IntegratorDropdown").GetComponent<UnityEngine.UI.Dropdown>();
        if (m_Integrator is EulerIntegrator)
        {
            m_IntegratorDropdown.value = 0;
        }
        else if (m_Integrator is MidpointIntegrator)
        {
            m_IntegratorDropdown.value = 1;
        }
        else if (m_Integrator is RungeKutta4Integrator)
        {
            m_IntegratorDropdown.value = 2;
        }
        else if (m_Integrator is VerletIntegrator)
        {
            m_IntegratorDropdown.value = 3;
        }
        m_IntegratorDropdown.RefreshShownValue();

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
        else if (m_Scenario is CirclesAndSpringsScenario)
        {
            m_ScenarioDropdown.value = 3;
        }
        else if (m_Scenario is TrainScenario)
        {
            m_ScenarioDropdown.value = 4;
        }
        else if (m_Scenario is PendulumScenario)
        {
            m_ScenarioDropdown.value = 5;
        }
        else if (m_Scenario is DualCircleScenario)
        {
            m_ScenarioDropdown.value = 6;
        }
        m_ScenarioDropdown.RefreshShownValue();

        m_CircleDropdown = GameObject.Find("CircleDropdown").GetComponent<UnityEngine.UI.Dropdown>();
        if (CircularWireConstraint.OLD)
        {
            m_CircleDropdown.value = 0;
        }
        else
        {
            m_CircleDropdown.value = 1;
        }

        m_CircleDropdown.RefreshShownValue();

        m_RodDropdown = GameObject.Find("RodDropdown").GetComponent<UnityEngine.UI.Dropdown>();
        if (RodConstraint.OLD)
        {
            m_RodDropdown.value = 0;
        }
        else
        {
            m_RodDropdown.value = 1;
        }

        m_CircleDropdown.RefreshShownValue();

        m_AngleDropdown = GameObject.Find("AngleDropdown").GetComponent<UnityEngine.UI.Dropdown>();
        if (AngularSpringForce.INIT_AD_HOC)
        {
            m_AngleDropdown.value = 0;
        }
        else
        {
            m_AngleDropdown.value = 1;
        }

        m_CircleDropdown.RefreshShownValue();

        m_SolverDropdown = GameObject.Find("SolverDropdown").GetComponent<UnityEngine.UI.Dropdown>();
        m_SolverDropdown.value = 1; // ConjGrad2 is default.
    }

    private void Test()
    {
        BlockSparseMatrix A = new BlockSparseMatrix();
        BlockSparseMatrix.MatrixBlock block = A.CreateMatrixBlock(0, 0, 2, 2);
        block.data[0] = 50;
        block.data[1] = 0;
        block.data[2] = 0;
        block.data[3] = 1.00678410253204;
        A.SetN(2);
        double[] b = new double[2];
        b[0] = 3.89261960983276;
        b[1] = -67.8883167144682;
        double solverEpsilon = Math.Pow(10, -5);
        const int solverSteps = 100000;
        double[] x = new double[2];
        int steps = 0;
        new ConjGradSolver2().Solve(A, x, b, solverEpsilon, solverSteps, out steps);
    }


    void Update()
    {
        HandleMouseInteraction();
    }

    void FixedUpdate()
    {
        try
        {
            m_Integrator.Step(m_ParticleSystem, Time.fixedDeltaTime);
        }
        catch (Exception e)
        {
            Debug.LogError("We encountered an error, so we will reset the scenario.");
            Debug.LogError(e.Message + "\n" + e.StackTrace);
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

    public void Reset()
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

    public void OnIntegratorTypeChanged()
    {
        switch (m_IntegratorDropdown.value)
        {
            case 0:
                m_Integrator = new EulerIntegrator();
                Debug.Log("Switched to Euler");
                break;
            case 1:
                m_Integrator = new MidpointIntegrator();
                Debug.Log("Switched to Midpoint");
                break;
            case 2:
                m_Integrator = new RungeKutta4Integrator();
                Debug.Log("Switched to Runge Kutta 4th");
                break;
            case 3:
                m_Integrator = new VerletIntegrator();
                Debug.Log("Switched to Verlet");
                break;
        }
    }

    public void OnSolverTypeChanged()
    {
        switch (m_SolverDropdown.value)
        {
            case 0:
                m_ParticleSystem.SetSolver(new ConjGradSolver());
                Debug.Log("Switched to original conjugate gradient solver");
                break;
            case 1:
                m_ParticleSystem.SetSolver(new ConjGradSolver2());
                Debug.Log("Switched to conjugate gradient solver 2");
                break;
            case 2:
                m_ParticleSystem.SetSolver(new JacobiSolver());
                Debug.Log("Switched to Jacobi solver");
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
            case 3:
                m_Scenario = new CirclesAndSpringsScenario();
                Debug.Log("Switched to circles & springs scenario");
                break;
            case 4:
                m_Scenario = new TrainScenario();
                Debug.Log("Switched to train scenario");
                break;
            case 5:
                m_Scenario = new PendulumScenario();
                Debug.Log("Switched to pendulum scenario");
                break;
            case 6:
                m_Scenario = new DualCircleScenario();
                Debug.Log("Switched to dual circle scenario");
                break;
        }
        Reset();
    }

    public void OnCircleTypeChanged()
    {
        switch (m_CircleDropdown.value)
        {
            case 0:
                CircularWireConstraint.OLD = true;
                Debug.Log("Switched to Quadratic Circles");
                break;
            case 1:
                CircularWireConstraint.OLD = false;
                Debug.Log("Switched to Linear Circles");
                break;
        }
    }

    public void OnRodTypeChanged()
    {
        switch (m_CircleDropdown.value)
        {
            case 0:
                CircularWireConstraint.OLD = true;
                Debug.Log("Switched to Quadratic Rods");
                break;
            case 1:
                CircularWireConstraint.OLD = false;
                Debug.Log("Switched to Linear Rods");
                break;
        }
    }

    public void OnAngleTypeChanged()
    {
        switch (m_AngleDropdown.value)
        {
            case 0:
                AngularSpringForce.INIT_AD_HOC = true;
                Debug.Log("Switched to ad hoc angle springs: New scenarios will initialize with ad hoc angular springs");
                break;
            case 1:
                AngularSpringForce.INIT_AD_HOC = false;
                Debug.Log("Switched to analitic angle springs: New scenarios will initialize with analitical angular springs");
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
