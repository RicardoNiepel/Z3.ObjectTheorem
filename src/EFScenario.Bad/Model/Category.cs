namespace EFScenario.Bad.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Category
    {
        public int CatNr { get; set; }

        [Index(IsUnique = true)]
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        public ChangeInfo CreatedAt { get; set; }

        public virtual ICollection<ChangeInfo> UpdatedAt { get; set; }
    }
}
