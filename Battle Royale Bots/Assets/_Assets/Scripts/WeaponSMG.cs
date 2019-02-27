using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSMG : Weapon
{
    public override AmmoType AmmoType => AmmoType.Light;
    public override float Damage => 10f;
    public override float Range => 5f;
    public override int BaseAmmo => 100;
    public override float FireDelay => 0.05f;
    public override float Precision => 4f;
}
