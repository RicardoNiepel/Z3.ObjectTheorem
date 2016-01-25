using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFScenario.Bad.Model
{
    public class ChangeInfo
    {
        public DateTime Date { get; set; }

        public string User { get; set; }
    }
}
