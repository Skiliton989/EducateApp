using System.ComponentModel.DataAnnotations;

namespace EducateApp.ViewModels.Disciplines
{
    public class EditDisciplineViewModel
    {
        public short Id { get; set; }

        [Display(Name = "Индекс профессионального модуля")]
        public string IndexProfMod { get; set; }

        [Display(Name = "Название профессионального модуля")]
        public string NameProfMod { get; set; }

        [Required(ErrorMessage = "Введите индекс дисциплины")]
        [Display(Name = "Индекс")]
        public string Index { get; set; }

        [Required(ErrorMessage = "Введите наименование дисциплины")]
        [Display(Name = "Наименование")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите сокращенное наименование дисциплины")]
        [Display(Name = "Сокращенное наименование")]
        public string ShortName { get; set; }

        public string IdUser { get; set; }
    }
}
