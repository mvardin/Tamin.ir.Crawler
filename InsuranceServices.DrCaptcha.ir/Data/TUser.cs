using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceServices.DrCaptcha.ir.Data
{
    public class TUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TUserId { get; set; }
        public string EntityId { get; set; }
        public string Login { get; set; }
        public int OrganizationKey { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; }
        public string AccountStatus { get; set; }
        public string Email { get; set; }
        public string NationalCode { get; set; }
        public string Mobile { get; set; }
        public string Gender { get; set; }
        public long BirthDate { get; set; }
    }
}