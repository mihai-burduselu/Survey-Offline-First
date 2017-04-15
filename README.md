# Survey-Offline-First

How to run
=====================
Open etouches API.sln => Start (F5) =>
<br /> ![Alt text](documentation/localhost.jpg) <br />=> Copy the number marked with yellow
<br />Open Survey_Offline_First.sln => MainPage.xaml.cs => <br /> => Modify (paste) the PORT number at line: private const string PORT = "53780";
<br />=> Survey_Offline_First.sln => Start (F5)
<br /> If you are online, the form is sent to etouches API simulated on localhost. <br />To check it, go to http://localhost:53780/api/Survey/GetSurvey (if PORT == 53780) 
