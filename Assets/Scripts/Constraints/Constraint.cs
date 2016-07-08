public interface Constraint
{
    void UpdateJacobians(ParticleSystem a_ParticleSystem);

    double[] GetValue(ParticleSystem a_ParticleSystem);

    double[] GetDerivativeValue(ParticleSystem a_ParticleSystem);

    int GetConstraintDimension();

    void Draw();
}
