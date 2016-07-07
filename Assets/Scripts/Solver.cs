public abstract class Solver
{
    public abstract void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime);

    protected void ScaleVector(double[] a_Vector, double a_ScaleFactor)
    {
        for (int i = 0; i < a_Vector.Length; ++i)
        {
            a_Vector[i] *= a_ScaleFactor;
        }
    }

    protected void AddVectors(double[] a_VectorA, double[] a_VectorB, double[] a_VectorOut)
    {
        if (!(a_VectorA.Length == a_VectorB.Length && a_VectorB.Length == a_VectorOut.Length))
        {
            throw new System.Exception("Input vectors do not have equal length!");
        }
        int vecLength = a_VectorA.Length;
        for (int i = 0; i < vecLength; ++i)
        {
            a_VectorOut[i] = a_VectorA[i] + a_VectorB[i];
        }
    }

    protected void AddVectorsScale(double[] a_VectorA, double[] a_VectorB, double[] a_VectorOut, double a_ScaleA, double a_ScaleB)
    {
        if (!(a_VectorA.Length == a_VectorB.Length && a_VectorB.Length == a_VectorOut.Length))
        {
            throw new System.Exception("Input vectors do not have equal length!");
        }
        int vecLength = a_VectorA.Length;
        for (int i = 0; i < vecLength; ++i)
        {
            a_VectorOut[i] = a_VectorA[i] * a_ScaleA + a_VectorB[i] * a_ScaleB;
        }
    }
}
