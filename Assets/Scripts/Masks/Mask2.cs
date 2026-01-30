using UnityEngine;

/// <summary>
/// Maske 2 - Zehirli Sis Koruması
/// Aktifken zehirli alanlardan hasar alınmaz
/// </summary>
public class Mask2 : MaskBase
{
    public override string MaskName => "Gaz Maskesi";
    public override int MaskIndex => 1;

    // Static - PlayerHealth bu property'yi kontrol eder
    public static bool IsPoisonImmune { get; private set; }

    protected override void OnActivate()
    {
        IsPoisonImmune = true;
    }

    protected override void OnDeactivate()
    {
        IsPoisonImmune = false;
    }

    void OnDestroy()
    {
        if (IsPoisonImmune)
        {
            IsPoisonImmune = false;
        }
    }
}

