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
    private UnityEngine.UI.Toggle m_SimulationFlowToggle;
    private UnityEngine.UI.Slider m_SpeedSlider;
    private UnityEngine.UI.Text m_SpeedSliderText;
    private const float m_ParticleSelectThreshold = 0.3f;
    private const float m_MouseSelectRestLength = 0f;
    private const float m_MouseSelectSpringConstant = 20f;
    private const float m_MouseSelectDampingConstant = 2f;
    private bool m_HasMouseSelection = false;
    private float m_Speed = 1f;
    private MouseSpringForce m_CurrentMouseForce;
    private bool m_ReversedTime = false;
    private Dictionary<int, MouseSpringForce> m_TouchForces;

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

        const int solverSteps = 100;
        m_ParticleSystem = new ParticleSystem(new ConjGradSolver2(), solverEpsilon, solverSteps, constraintSpringConstant, constraintDampingConstant);
        m_Integrator = new RungeKutta4Integrator();
        m_Scenario = new TestScenario();
        m_Scenario.CreateScenario(m_ParticleSystem);
        m_Speed = 1f;
        SetupDebugGameObjects();
        m_TouchForces = new Dictionary<int, MouseSpringForce>();
    }

    void Start()
    {
        m_ParticleSystem.Initialize();
        m_Integrator.Initialize(m_ParticleSystem);
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
        else if (m_Integrator is Verlet2Integrator)
        {
            m_IntegratorDropdown.value = 4;
        }
        else if (m_Integrator is LeapfrogIntegrator)
        {
            m_IntegratorDropdown.value = 5;
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
        else if (m_Scenario is ElipticalEngineScenario)
        {
            m_ScenarioDropdown.value = 7;
        }
        else if (m_Scenario is SlicedEllipseScenario)
        {
            m_ScenarioDropdown.value = 8;
        }
        else if (m_Scenario is ReversibleScenario)
        {
            m_ScenarioDropdown.value = 9;
        }
        else if (m_Scenario is LeapfrogTestScenario)
        {
            m_ScenarioDropdown.value = 10;
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
        m_SolverDropdown.value = 1; // CG2 default

        m_SimulationFlowToggle = GameObject.Find("ReverseSimulationToggle").GetComponent<UnityEngine.UI.Toggle>();
        m_SpeedSlider = GameObject.Find("SpeedSlider").GetComponent<UnityEngine.UI.Slider>();
        m_SpeedSliderText = m_SpeedSlider.GetComponentInChildren<UnityEngine.UI.Text>();
        m_SpeedSlider.value = Mathf.Clamp(m_Speed, 0.01f, 5f);
        m_SpeedSliderText.text = m_Speed + "x";
    }

    void Update()
    {
        HandleUserInteraction();
    }

    void FixedUpdate()
    {
        if (m_ParticleSystem.Time < 0)
        {
            ResetSimulation();
        }
        float delta = Time.fixedDeltaTime * m_Speed;
        try
        {
            m_Integrator.Step(m_ParticleSystem, delta);
            m_ParticleSystem.Time += (m_ReversedTime ? -delta : delta);
        }
        catch (Exception e)
        {
            Debug.LogError("We encountered an error, so we will reset the scenario.");
            Debug.LogError(e.Message + "\n" + e.StackTrace);
            ResetSimulation();
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

    public void ResetSimulation()
    {
        m_HasMouseSelection = false;
        m_CurrentMouseForce = null;
        if (m_SpeedSliderText != null)
        {
            m_SpeedSliderText.text = Math.Round(m_Speed, 4) + "x";
        }
        m_ReversedTime = false;
        if (m_SimulationFlowToggle != null)
        {
            m_SimulationFlowToggle.isOn = false;
        }
        m_TouchForces.Clear();
        m_ParticleSystem.Clear();
        m_Scenario.CreateScenario(m_ParticleSystem);
        m_ParticleSystem.Initialize();
        m_Integrator.Initialize(m_ParticleSystem);
        SetupDebugGameObjects();
    }

    public void ResetSpeed()
    {
        m_Speed = 1f;
        m_SpeedSliderText.text = "1x";
        m_SpeedSlider.value = 1f;
    }

    private void HandleUserInteraction()
    {
        if (Input.touchSupported)
        {
            if (Input.touchCount > 0)
            {
                Touch[] touches = Input.touches;
                for (int i = 0; i < touches.Length; ++i)
                {
                    Touch touch = touches[i];
                    int id = touch.fingerId;
                    Vector3 touchPos3D = Camera.main.ScreenToWorldPoint(touch.position);
                    Vector2 touchPos = new Vector2(touchPos3D.x, touchPos3D.y);
                    if (touch.phase == TouchPhase.Began)
                    {
                        Particle closestParticle;
                        if (GetClosestParticle(touchPos, m_ParticleSelectThreshold, out closestParticle))
                        {
                            MouseSpringForce touchForce = new MouseSpringForce(closestParticle, touchPos, m_MouseSelectRestLength, m_MouseSelectSpringConstant, m_MouseSelectDampingConstant);
                            // This should never happen, but adding anyway for robustness:
                            MouseSpringForce existingForce;
                            if (m_TouchForces.TryGetValue(id, out existingForce))
                            {
                                m_ParticleSystem.RemoveForce(existingForce);
                            }
                            m_TouchForces[id] = touchForce;
                            m_ParticleSystem.AddForce(touchForce);
                        }
                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                        MouseSpringForce existingForce;
                        if (m_TouchForces.TryGetValue(id, out existingForce))
                        {
                            existingForce.UpdateMousePosition(touchPos);
                        }
                    }
                    else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
                    {
                        MouseSpringForce existingForce;
                        if (m_TouchForces.TryGetValue(id, out existingForce))
                        {
                            m_ParticleSystem.RemoveForce(existingForce);
                            m_TouchForces.Remove(id);
                        }
                    }
                }
            }
        }
        else
        {
            Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos = new Vector2(mousePos3D.x, mousePos3D.y);

            if (!m_HasMouseSelection && Input.GetMouseButtonDown(0))
            {
                Particle closestParticle;
                if (GetClosestParticle(mousePos, m_ParticleSelectThreshold, out closestParticle))
                {
                    m_CurrentMouseForce = new MouseSpringForce(closestParticle, mousePos, m_MouseSelectRestLength,
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
            else if (m_HasMouseSelection)
            {
                m_CurrentMouseForce.UpdateMousePosition(mousePos);
            }
        }
    }

    private bool GetClosestParticle(Vector2 a_Position, float a_Threshold, out Particle out_Particle)
    {
        int numParticles = m_ParticleSystem.Particles.Count;
        Particle closestParticle = null;
        float particleDistanceSqr = float.MaxValue;
        Particle curParticle = null;
        for (int i = 0; i < numParticles; ++i)
        {
            curParticle = m_ParticleSystem.Particles[i];
            float curDistanceSqr = (a_Position - curParticle.Position).sqrMagnitude;
            if (curDistanceSqr < particleDistanceSqr)
            {
                closestParticle = curParticle;
                particleDistanceSqr = curDistanceSqr;
            }
        }
        if (closestParticle != null && particleDistanceSqr < (a_Threshold * a_Threshold))
        {
            out_Particle = closestParticle;
            return true;
        }
        out_Particle = null;
        return false;
    }

    public void OnIntegratorTypeChanged()
    {
        switch (m_IntegratorDropdown.value)
        {
            case 0:
                m_Integrator = new EulerIntegrator();
                m_Integrator.Initialize(m_ParticleSystem);
                Debug.Log("Switched to Euler");
                break;
            case 1:
                m_Integrator = new MidpointIntegrator();
                m_Integrator.Initialize(m_ParticleSystem);
                Debug.Log("Switched to Midpoint");
                break;
            case 2:
                m_Integrator = new RungeKutta4Integrator();
                m_Integrator.Initialize(m_ParticleSystem);
                Debug.Log("Switched to Runge Kutta 4th");
                break;
            case 3:
                m_Integrator = new VerletIntegrator();
                m_Integrator.Initialize(m_ParticleSystem);
                Debug.Log("Switched to Verlet 2nd order");
                break;
            case 4:
                m_Integrator = new Verlet2Integrator();
                m_Integrator.Initialize(m_ParticleSystem);
                Debug.Log("Switched to Verlet 3rd order");
                break;
            case 5:
                m_Integrator = new LeapfrogIntegrator();
                m_Integrator.Initialize(m_ParticleSystem);
                Debug.Log("Switched to Leapfrog");
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
            case 7:
                m_Scenario = new ElipticalEngineScenario();
                Debug.Log("Switched to elipse Engine scenario");
                break;
            case 8:
                m_Scenario = new SlicedEllipseScenario();
                Debug.Log("Switched to elipse Engine scenario");
                break;
            case 9: 
                m_Scenario = new ReversibleScenario();
                Debug.Log("Switched to reversible scenario");
                break;
            case 10:
                m_Scenario = new LeapfrogTestScenario();
                Debug.Log("Switched to leapfrog test scenario");
                break;
        }
        ResetSimulation();
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
        ResetSimulation();
    }

    public void OnSimulationFlowChanged()
    {
        m_ReversedTime = m_SimulationFlowToggle.isOn;
        m_ParticleSystem.ReverseParticleVelocities();
    }

    public void OnSpeedSliderChanged()
    {
        m_Speed = (float)Math.Round(m_SpeedSlider.value, 4);
        m_SpeedSliderText.text = Math.Round(m_Speed, 4) + "x";
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
