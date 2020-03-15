using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models
{
    public class TestingNotMapped
    {
        public int Id { get; set; }
        //without mapping or foreign key
        public virtual MenuItem MenuItem { get; set; }
    }

    public class TestingOnlyMapped
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        //without mapping or foreign key
        [NotMapped]
        public virtual MenuItem MenuItem { get; set; }
    }
}
