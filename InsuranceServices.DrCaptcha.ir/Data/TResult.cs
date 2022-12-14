using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceServices.DrCaptcha.ir.Data
{
    public class TResult
    {
        public string Name { get; set; }
        public string Family { get; set; }
        public string NationalCode { get; set; }
        public string IdentityNumber { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }

        public long LastSalary { get; set; }
        public string Last3MonthsAverageSalary { get; set; }
        public string Last1YearAverageSalary { get; set; }
        public long TotalDays { get; set; }
        public long TotalMonths { get; set; }
        public string Last5YearInsurancePercent { get; set; }
        public List<Record> LastRecords { get; set; }
    }
    public class Record
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string KargahName { get; set; }
        public string KargahNumber { get; set; }
        public long Salary { get; set; }
        public int WorkDayCount { get; set; }
    }
}
