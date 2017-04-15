using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Survey_Offline_First
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public string Answer { get; set; }

        public int SurveyId { get; set; }
        [ForeignKey("SurveyId")]
        public Survey Survey { get; set; }

        public Question(string text, string type)
        {
            Text = text;
            Type = type;
        }

        public Question()
        {

        }
    }
}
