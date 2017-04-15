using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Survey_Offline_First
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // These are the uri and endpoint of a local WebApi
        // Change these when using another API or port.
        private const string API_URI = "http://localhost:";
        private const string PORT = "53780";
        private const string ENDPOINT = "/api/Survey/PostSurvey";
        public MainPage()
        {
            this.InitializeComponent();
            ListenToInternetConnection();
        }

        private void ListenToInternetConnection()
        {
            // initial verif
            offline.Visibility = (Network.IsConnected ? Visibility.Collapsed : Visibility.Visible);

            Network.InternetConnectionChanged += async (s, e) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    offline.Visibility = (e.IsConnected ? Visibility.Collapsed : Visibility.Visible);
                    if (e.IsConnected)
                    {
                        using (var db = new SurveyContext())
                        {
                            if (db.Surveys.ToList().Count != 0 && !db.GetSurvey().WasSent)
                            {
                                SendToEtouchesAPI(db.GetSurvey());
                                db.DeleteCache();
                                await Popup_OnlineMode();
                            }
                        }
                    }
                });
            };
        }

        private async Task Popup_OnlineMode()
        {
            var message = new Windows.UI.Popups.MessageDialog("");

            message.Title = "You are back online, so I sent the form to EtouchesAPI";
            message.Commands.Add(new Windows.UI.Popups.UICommand("Ok") { Id = 0 });

            await message.ShowAsync();
        }

        private async void Submit(object sender, RoutedEventArgs e)
        {
            SubmitButton.IsEnabled = false;
            string submissionStatus = "";
            Survey survey = getSurveyData();

            Submit(survey, ref submissionStatus);
            await Popup_SubmissionStatus(survey, submissionStatus);
        }

        private void Submit(Survey survey, ref string submissionStatus)
        {
            bool isInternetConnected = NetworkInterface.GetIsNetworkAvailable();

            if (isInternetConnected)
            {
                SendToEtouchesAPI(survey);
                submissionStatus = "Sent to EtouchesAPI";
            }
            else
            {
                AddToLocalStorage(survey);
                submissionStatus = "Offline stored";
            }
        }

        private void SendToEtouchesAPI(Survey survey)
        {
            Post(survey);
            UpdateSurveyStatus(survey);
        }

        private void AddToLocalStorage(Survey survey)
        {
            using (var db = new SurveyContext())
            {
                db.AddSurvey(survey);
            }
        }

        public async void Post(object survey)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                // create the header of the POST request
                httpClient.BaseAddress = new Uri(API_URI + PORT);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8"));

                try
                {
                    string objectSerialized = JsonConvert.SerializeObject(survey, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    HttpContent content = new StringContent(objectSerialized, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(ENDPOINT, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        //do something with json response here
                    }
                }
                catch (Exception e)
                {
                    //Could not connect to server
                }
            }
        }

        private void UpdateSurveyStatus(Survey survey)
        {
            survey.WasSent = true;
            using (var db = new SurveyContext())
            {
                db.UpdateSurveyStatus(survey.SurveyId, survey.WasSent);
            }
        }

        private async Task Popup_SubmissionStatus(Survey survey, string submissionStatus)
        {
            var message = new Windows.UI.Popups.MessageDialog(
               "You mark the events with " + survey.Questions[0].Answer
               + " and the internet with " + survey.Questions[1].Answer + "."
               + "\nYou also find some issues: " + survey.Questions[2].Answer + "."
               + "\nThank you for the time you spend to complete this!\n\n SubmissionStatus: " + submissionStatus
               + "\n\n(If the SubmissionStatus is \"Offline stored\", the survey will be stored on localstorage and it will be sent when you are back online)");

            message.Title = "The form was submitted!";
            message.Commands.Add(new Windows.UI.Popups.UICommand("Ok") { Id = 0 });

            await message.ShowAsync();
        }

        private async void ShowDatabase(object sender, RoutedEventArgs e)
        {
            using (var db = new SurveyContext())
            {
                string text = "";
                if (!db.IsEmpty())
                {
                    Survey survey = db.GetSurvey();
                    text += "SurveyId = " + survey.SurveyId + " Title = " + survey.Title + " WasSent? " + survey.WasSent + "\n";
                    foreach (Question q in db.Questions.ToList())
                    {
                        text += "Question: " + q.Text + " Type = " + q.Type + " Answer = " + q.Answer + "\n";
                    }
                }
                else
                {
                    text += "The database is empty! No survey is cached.";
                }
                var message = new Windows.UI.Popups.MessageDialog("Hello!\n" + text);
                message.Title = "The db was imported!";
                message.Commands.Add(new Windows.UI.Popups.UICommand("Cancel") { Id = 0 });

                await message.ShowAsync();
            }
        }

        private Survey getSurveyData()
        {
            Question q1 = new Question("How were the events during the conference?", "Radio");
            q1.Answer = getQ1Response().ToString();

            Question q2 = new Question("How was the internet access?", "Radio");
            q2.Answer = getQ2Response().ToString();

            Question q3 = new Question("What kind of issues did you discover during the conference?", "TextBox");
            q3.Answer = Issues.Text;

            List<Question> questions = new List<Question>();
            questions.Add(q1);
            questions.Add(q2);
            questions.Add(q3);

            return new Survey("Feedback", questions);
        }

        private int getQ1Response()
        {
            if (rb1.IsChecked == true)
                return 1;
            else if (rb2.IsChecked == true)
                return 2;
            else if (rb3.IsChecked == true)
                return 3;
            else if (rb4.IsChecked == true)
                return 4;
            else if (rb5.IsChecked == true)
                return 5;
            return 0;
        }

        private int getQ2Response()
        {
            if (q2rb1.IsChecked == true)
                return 1;
            else if (q2rb2.IsChecked == true)
                return 2;
            else if (q2rb3.IsChecked == true)
                return 3;
            else if (q2rb4.IsChecked == true)
                return 4;
            else if (q2rb5.IsChecked == true)
                return 5;
            return 0;
        }
    }
}
