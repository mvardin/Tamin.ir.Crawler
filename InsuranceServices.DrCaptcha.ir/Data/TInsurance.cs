using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceServices.DrCaptcha.ir.Data
{
    public class TInsurance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TInsuranceId { get; set; }
        public int DateYear { get; set; }
        public int DateMonth { get; set; }
        public long Salary { get; set; }
        public string WorkDays { get; set; }
        public string KargahName { get; set; }
        public string KargahNumber { get; set; }
    }
}
