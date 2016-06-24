public abstract class Solver
{
   abstract void Step(ParticleSystem a_ParticleSystem, float a_DeltaTime);

	protected void ScaleVector(float[] a_Vector, float a_ScaleFactor)
    {
        for (int i = 0; i < a_Vector.Length; ++i)
        {
            a_Vector[i] *= a_ScaleFactor;
        }
    }

    protected void AddVectors(float[] a_VectorA, float[] a_VectorB, float[] a_VectorOut)
    {
        if (!(a_VectorA.Length == a_VectorB.Length && a_VectorB.Length == a_VectorOut.Length))
        {
            throw new Exception("Input vectors do not have equal length!");
        }
        int vecLength = a_VectorA.Length;
        for (int i = 0; i < vecLength; ++i)
        {
            a_VectorOut[i] = a_VectorA[i] + a_VectorB[i];
        }
    }
}
