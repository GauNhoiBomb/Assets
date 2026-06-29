using UnityEngine;

// ==========================================
// PHÂN LOẠI TOKEN
// ==========================================
public enum TokenType
{
    WeaponAction, // Các hành động vũ khí (Đâm, Chém, Đập)
    Buff,         // Các chỉ số có lợi (Khung tròn Xanh)
    Debuff,       // Các hiệu ứng xấu (Khung vuông Đỏ)
    Element       // Token nguyên tố độc lập (Lửa, Nước, Môi trường...)
}

[CreateAssetMenu(fileName = "New Token", menuName = "JRPG System/Token Data")]
public class TokenData : ScriptableObject
{
    [Header("--- THÔNG TIN TOKEN ---")]
    public string tokenName = "Tên Token";
    public TokenType type = TokenType.WeaponAction;

    [Tooltip("Kéo thả hình ảnh Icon của Token vào đây")]
    public Sprite tokenIcon;

    [TextArea(2, 4)]
    [Tooltip("Mô tả luật của Token này để sau này in ra UI khi rê chuột vào")]
    public string description = "Mô tả hiệu ứng...";
}