using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceServices.DrCaptcha.ir.Data
{
    public class TDastmozd
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string DastmozdId { get; set; }
        public string NationalCode { get; set; }
        public string hismon6 { get; set; }
        public string hismon10 { get; set; }
        public string hismon7 { get; set; }
        public object risufname { get; set; }
        public string hismon8 { get; set; }
        public string hismon12 { get; set; }
        public string hismon9 { get; set; }
        public string hismon11 { get; set; }
        public string hismon2 { get; set; }
        public string hismon3 { get; set; }
        public object risubirthdate { get; set; }
        public string hismon4 { get; set; }
        public string hismon5 { get; set; }
        public string hismon1 { get; set; }
        public int hisyear { get; set; }
        public object risuidserial2 { get; set; }
        public object risuidserial1 { get; set; }
        public string rwshname { get; set; }
        public object expcitycode { get; set; }
        public string brhcode { get; set; }
        public string hiswage1 { get; set; }
        public string hiswage2 { get; set; }
        public string hiswage3 { get; set; }
        public string hiswage4 { get; set; }
        public int id { get; set; }
        public string hiswage5 { get; set; }
        public string hiswage6 { get; set; }
        public string hiswage7 { get; set; }
        public string hiswage8 { get; set; }
        public string hiswage9 { get; set; }
        public object risuidno { get; set; }
        public object risudname { get; set; }
        public object risuid { get; set; }
        public object risulname { get; set; }
        public string hiswage10 { get; set; }
        public string hiswage12 { get; set; }
        public object risunatcode { get; set; }
        public string hiswage11 { get; set; }
        public string brhname { get; set; }
        public string historytypedesc { get; set; }
        public string rwshid { get; set; }
    }
}
