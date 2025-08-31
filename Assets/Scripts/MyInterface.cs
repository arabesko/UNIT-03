using System.Collections.Generic;

public interface IDamagiable
{
    public void Health(float health);
    public void Damage(float damage);
}

public interface IModules
{
    public void Initialized(PlayerMovement player);
    public void PowerElement();
}

public interface IPuzzlesElements
{
    public void Activate();
    public void Desactivate();

    public void ActionPuzzle();
    public int MyReturnNumber();
}

public interface IWalls
{

}

public enum ModulosUnit03
{
    BrazoIzquierdo,
    Proyector,
    BrazoDerecho
}
