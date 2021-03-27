using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom.Entities
{
    public class TasksTableData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DateIssue { get; set; }
        public string DateDelivery { get; set; }
        public string Answer { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public string Issuer { get; set; }

        public TasksTableData(int id, string title, string description, string dateIssue, string dateDelivery, string answer, string comment, string status, string issuer)
        {
            Id = id;
            Title = title;
            Description = description;
            DateIssue = dateIssue;
            DateDelivery = dateDelivery;
            Answer = answer;
            Comment = comment;
            Status = status;
            Issuer = issuer;
        }
    }
}
