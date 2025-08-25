using System;

public interface IPlayerShootController
{
    void SetActualBullets(float quantity);
    void SetMaxBullets(float quantity);
    
    event Action<int,int> OnAmmoChanged;
}