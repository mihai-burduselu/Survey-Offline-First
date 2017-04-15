using etouches_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace etouches_API.Controllers
{
    public class SurveyController : ApiController
    {
        public static int count = 0;
        private static Survey Survey = null;

        [HttpGet]
        public Survey GetSurvey()
        {
            return Survey;
        }

        [HttpPost]
        public void PostSurvey(Survey survey)
        {
            Survey = survey;
            count++;
        }
    }
}
