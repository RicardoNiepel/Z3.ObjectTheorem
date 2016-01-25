namespace EFScenario.Bad.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Category
    {
        [Key]
        public int CatNr { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(255)]
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        public ChangeInfo CreatedAt { get; set; }
        [NotMapped]
        public virtual ICollection<ChangeInfo> UpdatedAt { get; set; }
    }
}
