public class ExplicitMatrix : Matrix
{
    private readonly double[,] m_Values;
    private readonly int m_M, m_N;

    public ExplicitMatrix(int m, int n)
    {
        m_N = n;
        m_M = m;
        m_Values = new double[m, n];
    }

    public void SetValue(int i, int j, double value)
    {
        m_Values[i, j] = value;
    }

    public int GetM()
    {
        return m_M;
    }

    public int GetN()
    {
        return m_N;
    }

    public double GetValue(int i, int j)
    {
        return m_Values[i, j];
    }

    public void Clear()
    {
        for (int i = 0; i < m_Values.GetLength(0); ++i)
        {
            for (int j = 0; j < m_Values.GetLength(1); ++j)
            {
                m_Values[i, j] = 0;
            }
        }
    }
}