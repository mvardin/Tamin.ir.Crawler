using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceServices.DrCaptcha.ir.Data
{
    public class TPerson
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TPersonId { get; set; }
        public string Password { get; set; }
        public DateTime UpdateDate { get; set; }
        public string NationalCode { get; set; }

        public string Serial1 { get; set; }
        public string MilitaryServiceCode { get; set; }
        public string FatherName { get; set; }
        public string LastName { get; set; }
        public string Serial2 { get; set; }
        public long CreationTime { get; set; }
        public long LastModificationTime { get; set; }
        public string CityCode { get; set; }
        public string SocialSecurityNumber { get; set; }
        public string LastModifiedBy { get; set; }
        public string IssueplaceName { get; set; }
        public string BirthDate { get; set; }
        public string FirstName { get; set; }
        public string InsuranceNumber { get; set; }
        public string GenderCode { get; set; }
        public string NationalID { get; set; }
        public object MarriageCode { get; set; }
        public string CreatedBy { get; set; }
        public string IdentityNumber { get; set; }
        public string CountryCode { get; set; }
        public string Id { get; set; }
        public long BirthDateTimestamp { get; set; }
        public string Issueplace { get; set; }
        public string NationCode { get; set; }
    }
}