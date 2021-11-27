using System.ComponentModel.DataAnnotations;

namespace EducateApp.ViewModels.TypesOfTotals
{
    public class CreateTypeOfTotalViewModel
    {
        [Required(ErrorMessage = "Введите вид аттестации")]
        [Display(Name = "Название промежуточной аттестации")]
        public string CertificateName { get; set; }
    }
}
