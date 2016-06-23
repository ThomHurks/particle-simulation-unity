public interface Constraint
{
    void UpdateJacobians(ParticleSystem a_ParticleSystem);

    float GetValue(ParticleSystem a_ParticleSystem);

    float GetDerivativeValue(ParticleSystem a_ParticleSystem);

    int GetConstraintDimension();

    void Draw();
}
