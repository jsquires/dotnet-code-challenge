using System;

namespace CodeChallenge.Models
{
    public class Compensation
    {
        public String Id { get; set; }
        public virtual Employee Employee { get; set; }
        public decimal Salary { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
