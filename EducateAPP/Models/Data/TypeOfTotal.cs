using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducateApp.Models.Data
{
    public class TypeOfTotal
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ИД")]
        public short Id { get; set; }

        [Required(ErrorMessage = "Введите вид аттестации")]
        [Display(Name = "Название промежуточной аттестации")]
        public string CertificateName { get; set; }

        public string IdUser { get; set; }


        // Навигационные свойства

        [ForeignKey("IdUser")]
        public User User { get; set; }
    }
}
