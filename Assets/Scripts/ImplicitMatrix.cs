public interface ImplicitMatrix
{
    void MatrixTimesVector(float[] a_Source, float[] a_Destination);

    void MatrixTransposeTimesVector(float[] a_Source, float[] a_Destination);
}
