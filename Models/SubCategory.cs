using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models
{
    public class SubCategory
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Sub Category Name")]
        [Required]
        public string Name { get; set; }
        [Display(Name = "Category Name")]
        [Required]
        public int CategoryId { get; set; }

        //name of the field that is a foreign key
        [ForeignKey("CategoryId")]

        //name of the key where the FK is the PK
        public virtual Category Category { get; set; }
    }
}
