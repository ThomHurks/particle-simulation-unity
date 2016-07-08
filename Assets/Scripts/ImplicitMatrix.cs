using UnityEngine;

public abstract class ImplicitMatrix : Matrix
{
    private System.Text.StringBuilder m_DebugStringBuilder;

    // These functions must be implemented in the inheriting classes.
    protected abstract void MatrixTimesVectorImpl(double[] a_Source, double[] a_Destination);

    protected abstract void MatrixTransposeTimesVectorImpl(double[] a_Source, double[] a_Destination);

    public abstract int getM();

    public abstract int getN();

    public abstract double getValue(int i, int j);

    public ExplicitMatrix toExplicitMatrix()
    {
        ExplicitMatrix B = new ExplicitMatrix(getM(), getN());
        for (int i = 0; i < getM(); i++)
        {
            for (int j = 0; j < getN(); j++)
            {
                B.setValue(i, j, getValue(i, j));
            }
        }
        return B;
    }

    // Perform safety checks in abstract class.
    public void MatrixTimesVector(double[] a_Source, double[] a_Destination)
    {
        VerifyCorrectSize(a_Source, a_Destination);
        VerifyValidValues(a_Source);
        MatrixTimesVectorImpl(a_Source, a_Destination);
    }

    public void MatrixTransposeTimesVector(double[] a_Source, double[] a_Destination)
    {
        VerifyCorrectTransposeSize(a_Source, a_Destination);
        VerifyValidValues(a_Source);
        MatrixTransposeTimesVectorImpl(a_Source, a_Destination);
    }

    private void VerifyCorrectSize(double[] a_Source, double[] a_Destination)
    {
        if (a_Destination.Length != getM())
        {
            throw new System.Exception("Output size wrong! Should be " + getM() + ",but is " + a_Destination.Length);
        }
        if (a_Source.Length != this.getN())
        {
            throw new System.Exception("Input size wrong! Should be " + getN() + ",but is " + a_Source.Length);
        }
    }

    private void VerifyCorrectTransposeSize(double[] a_Source, double[] a_Destination)
    {
        if (a_Destination.Length != getN())
        {
            throw new System.Exception("Output size wrong! Should be " + getN() + ",but is " + a_Destination.Length);
        }
        if (a_Source.Length != getM())
        {
            throw new System.Exception("Input size wrong! Should be " + getM() + ",but is " + a_Source.Length);
        }
    }

    private void VerifyValidValues(double[] a_Source)
    {
        for (int i = 0; i < a_Source.Length; ++i)
        {
            if (double.IsNaN(a_Source[i]) || double.IsInfinity(a_Source[i]))
            {
                throw new System.Exception("Source vector did not validate: NaN or Inf found.");
            }
        }
    }

    public virtual void printX()
    {
        const string sep = ", ";
        if (m_DebugStringBuilder == null)
        {
            m_DebugStringBuilder = new System.Text.StringBuilder();
        }
        m_DebugStringBuilder.Remove(0, m_DebugStringBuilder.Length);
        m_DebugStringBuilder.EnsureCapacity(getM() * getN() * 64 * 2);
        for (int i = 0; i < getM(); i++)
        {
            for (int j = 0; j < getN(); j++)
            {
                m_DebugStringBuilder.Append(getValue(i, j));
                m_DebugStringBuilder.Append(sep);
            }
            m_DebugStringBuilder.Append("\n");
        }
        Debug.Log(m_DebugStringBuilder.ToString());
    }
}
