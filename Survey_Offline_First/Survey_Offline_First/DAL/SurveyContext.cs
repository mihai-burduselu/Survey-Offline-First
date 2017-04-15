using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Survey_Offline_First
{
    public class SurveyContext : DbContext
    {
        public const string FileName = "SurveyDB.db";
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<Question> Questions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string databaseFilePath = FileName;
            try
            {
                databaseFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseFilePath);
            }
            catch (InvalidOperationException) { }

            optionsBuilder.UseSqlite($"Data source={databaseFilePath}");
        }

        /// <summary>
        /// Get the survey from localstorage.
        /// </summary>
        /// <returns>Survey instance with questions.</returns>
        public Survey GetSurvey()
        {
            Survey result = Surveys.ToList()[0];
            result.Questions = Questions.ToList();
            return result;
        }

        public void AddSurvey(Survey survey)
        {
            Surveys.Add(survey);
            SaveChanges();
        }

        /// <summary>
        /// Update the survey from the database with "Sent/Not sent" status.
        /// </summary>
        /// <param name="id">Id of the survey</param>
        /// <param name="wasSent">Indicate if the survey was sent to the API or not</param>
        public void UpdateSurveyStatus(int id, bool wasSent)
        {
            Survey surveyToUpdate = Surveys.FirstOrDefault(e => e.SurveyId == id);
            if (surveyToUpdate != null)
            {
                surveyToUpdate.WasSent = true;
                SaveChanges();
            }
        }

        public bool IsEmpty()
        {
            return (Surveys.ToList().Count == 0);
        }

        public void DeleteCache()
        {
            if (Surveys.ToList().Count != 0)
            {
                foreach(Survey s in Surveys)
                {
                    Questions.RemoveRange(Questions.ToList().Where(e => e.SurveyId == s.SurveyId));
                    Surveys.Remove(s);
                }
                SaveChanges();
            }
        }
    }
}
