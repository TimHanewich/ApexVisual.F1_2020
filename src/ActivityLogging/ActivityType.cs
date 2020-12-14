using System;

namespace ApexVisual.F1_2020.ActivityLogging
{
    public enum ActivityType
    {
        // Application ambiguous activities
        Launch = 0, //The game/website (application) being launched
        Other = 1,

        #region "Apex Visual 2020 (Windows 10 app) activities"

        //Login-related
        LoginClickedOnHomePage = 8,
        RegisterClickedOnHomePage = 9,
        LoginAttemptedOnHomePage = 10,

        //Home page
        HomePageOpened = 20,

        //tutorial
        TutorialClosed = 2, // User pressed 'Let's race!' at the bottom of the tutorial page
        TutorialOpened = 3, //User pressed Tutorial on the home page to open the tutorial

        //Livd display
        LiveDisplayClicked = 4, //User pressed on live display on the home page
        LiveDisplayStartListeningClicked = 11, //User clicked on 'start listening'
        LiveDisplaySaveSessionClicked = 12, //User clicked save session
        LiveDisplayOpenSessionClicked = 13, //User clicked open session (now deprecated)
        LiveDisplaySaveToCloudClicked = 14, //User clicked on save to cloud
        LiveDisplaySaveToLocalClicked = 15, //User clicked on save to local
        LiveDisplaySaveToCloudFailed = 21, //The upload to cloud failed
        LiveDisplaySaveToCloudSucceeded = 22, //The upload to cloud succeeded (it is now in the cloud)

        //Analysis
        AnalysisOpened = 5, //User pressed on analysis on the home page
        AnalysisSessionLoaded = 19, //The user opened a Session in the analysis module. Should put the session id in the note maybe.

        //My account
        MyAccountOpened = 6, //User pressed on account on the home page
        ProfilePictureChanged = 18,

        //Version history
        VersionHistoryOpened = 7, //User pressed on live display on the home page

        //Send message to developer
        SendMessageToDeveloperOpened = 16, //The user opened the send message to developer pane
        SendMessageToDeveloperClicked = 17, //The user clicked 'submit' and therefore tried to send a message to developer

        #endregion

    }
}