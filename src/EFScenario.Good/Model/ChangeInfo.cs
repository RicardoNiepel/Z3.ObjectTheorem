using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFScenario.Bad.Model
{
    [ComplexType]
    public class ChangeInfo
    {
        public DateTime Date { get; set; }

        public string User { get; set; }
    }
}
