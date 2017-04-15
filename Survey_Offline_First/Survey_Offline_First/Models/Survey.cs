using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Survey_Offline_First
{
    public class Survey
    {
        [Key]
        public int SurveyId { get; set; }
        public string Title { get; set; }
        public bool WasSent { get; set; }
        public List<Question> Questions { get; set; }

        public Survey(string title, List<Question> questions)
        {
            Title = title;
            Questions = questions;
        }

        public Survey()
        {
            
        }
    }
}
