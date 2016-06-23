public abstract class ImplicitMatrix
{
    abstract void MatrixTimesVector(float[] a_Source, float[] a_Destination);

    abstract void MatrixTransposeTimesVector(float[] a_Source, float[] a_Destination);

	abstract int getM();

	abstract int getN();
}
