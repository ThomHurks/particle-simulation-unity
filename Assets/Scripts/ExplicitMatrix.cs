using System;


public class ExplicitMatrix:Matrix
{


    private double[,] m_Values;
    private readonly int m_M, m_N;


    public ExplicitMatrix(int m, int n)
    {
        m_N = n;
        m_M = m;
        m_Values = new double[m, n];
    }

    public void setValue(int i, int j, double value)
    {
        m_Values[i, j] = value;
    }

    public int getM()
    {
        return m_M;
    }

    public int getN()
    {
        return m_N;
    }

    public double getValue(int i, int j)
    {
        return m_Values[i, j];
    }
}


