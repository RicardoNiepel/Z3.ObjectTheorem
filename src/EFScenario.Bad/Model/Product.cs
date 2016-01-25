namespace EFScenario.Bad.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<string> Tags { get; set; }

        public Guid ReplacementProductId { get; set; }

        public virtual Product ReplacementProduct { get; set; }

        public int CategoryID { get; set; }

        public virtual Category Category { get; set; }
    }
}
