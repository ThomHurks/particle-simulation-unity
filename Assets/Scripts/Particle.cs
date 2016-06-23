using System.Collections;
using UnityEngine;

public class Particle
{
	private float m_Mass;
	private Vector2 m_Position;
	private Vector2 m_Velocity;
	private Vector2 m_ForceAccumulator;

	public Particle(float a_Mass)
	{
		m_Mass = a_Mass;
	}

	public float Mass { get { return m_Mass; } }
	public Vector2 Position
	{
		get { return m_Position; }
		set
        {
            if (float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsInfinity(value.x) || float.IsInfinity(value.y))
            {
                throw new System.Exception("Position cannot be NaN or Inf");
            }
            m_Position = value;
        }
	}
	public Vector2 Velocity
	{
		get { return m_Velocity; }
		set
        {
            if (float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsInfinity(value.x) || float.IsInfinity(value.y))
            {
                throw new System.Exception("Velocity cannot be NaN or Inf");
            }
            m_Velocity = value;
        }
	}
	public Vector2 ForceAccumulator
	{
		get { return m_ForceAccumulator; }
		set
        {
            if (float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsInfinity(value.x) || float.IsInfinity(value.y))
            {
                throw new System.Exception("Accumulated force cannot be NaN or Inf");
            }
            m_ForceAccumulator = value;
        }
	}
}
