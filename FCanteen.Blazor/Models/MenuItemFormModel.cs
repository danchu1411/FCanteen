using System.ComponentModel.DataAnnotations;

namespace FCanteen.Blazor.Models;

public class MenuItemFormModel
{
    public int MenuItemId { get; set; }

    [Display(Name = "Mã món")]
    [Required(ErrorMessage = "Mã món là bắt buộc.")]
    [RegularExpression(
        @"^MON-\d{4}$",
        ErrorMessage = "Mã món phải có dạng MON-0001.")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Tên món")]
    [Required(ErrorMessage = "Tên món là bắt buộc.")]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "Tên món phải từ 3 đến 100 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Giá bán")]
    [Range(
        typeof(decimal),
        "1000",
        "1000000",
        ErrorMessage = "Giá bán phải từ 1.000 đến 1.000.000 VND.")]
    public decimal Price { get; set; }

    [Display(Name = "Đơn vị")]
    [Required(ErrorMessage = "Đơn vị là bắt buộc.")]
    [StringLength(
        50,
        ErrorMessage = "Đơn vị tối đa 50 ký tự.")]
    public string Unit { get; set; } = string.Empty;

    [Display(Name = "Còn bán")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "Nhóm món")]
    [Required(ErrorMessage = "Vui lòng chọn nhóm món.")]
    public int? CategoryId { get; set; }
}