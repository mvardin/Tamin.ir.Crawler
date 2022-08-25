using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceServices.DrCaptcha.ir.Data
{
    public class TRequestLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TRequestLogId { get; set; }
        public RequestLogResult Result { get; set; }
        public DateTime CreateLog { get; set; }
        public double TimeElapsed { get; set; }
        public string Message { get; set; }
    }
    public enum RequestLogResult
    {
        UnCompleted = 0,
        Completed = 1,
        Error = 2
    }
}