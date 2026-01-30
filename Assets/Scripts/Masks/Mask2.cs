using UnityEngine;

/// <summary>
/// Maske 2 - Placeholder özellikler
/// </summary>
public class Mask2 : MaskBase
{
    public override string MaskName => "Maske 2";
    public override int MaskIndex => 1;

    [Header("Maske 2 Ayarları")]
    [Tooltip("Bu maskenin özel efekti için placeholder")]
    public string abilityDescription = "Özel Yetenek 2 - TODO: Implement";

    protected override void OnActivate()
    {
        // TODO: Maske 2 özel yeteneğini buraya ekle
        // Örnek: Duvarların arkasını görme, vb.
        Debug.Log($"[Maske 2] {abilityDescription} - AKTİF");
    }

    protected override void OnDeactivate()
    {
        // TODO: Maske 2 etkilerini kaldır
        Debug.Log($"[Maske 2] {abilityDescription} - KAPALI");
    }

    void Update()
    {
        if (IsActive)
        {
            // TODO: Sürekli efektler buraya
        }
    }
}
