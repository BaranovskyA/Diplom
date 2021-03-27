using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom.Entities
{
    public class Tasks
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateIssue { get; set; }
        public DateTime DateDelivery { get; set; }
        public string Answer { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public int IssuerId { get; set; }
        public Employer Issuer { get; set; }
        public int WorkerId { get; set; }
        public Employer Worker { get; set; }

        public Tasks() {}

        public Tasks(string title, string description, DateTime dateIssue, DateTime dateDelivery, string answer, 
            string comment, string status, int issuerId, int workerId)
        {
            Title = title;
            Description = description;
            DateIssue = dateIssue;
            DateDelivery = dateDelivery;
            Answer = answer;
            Comment = comment;
            Status = status;
            IssuerId = issuerId;
            WorkerId = workerId;
        }

        public Tasks(int id, string title, string description, DateTime dateIssue, DateTime dateDelivery, 
            string answer, string comment, string status, int issuerId, int workerId)
        {
            Id = id;
            Title = title;
            Description = description;
            DateIssue = dateIssue;
            DateDelivery = dateDelivery;
            Answer = answer;
            Comment = comment;
            Status = status;
            IssuerId = issuerId;
            WorkerId = workerId;
            Issuer = DBHelper.FindEmployerById(issuerId);
            Worker = DBHelper.FindEmployerById(workerId);
        }
    }
}
