# Retrieve User Information on Azure Authentication

This ASP.NET backend retrieve information of authenticated users with Facebook, Google, Twitter and Microsoft account. It returns user information as a json string. Also, the classes of these strings are included in project.

Firstly, you need to create developer applications to establish communication between your application and Facebook, Twitter, Microsoft and Google. Here is some good instructions to create developer applications provided by Microsoft Azure.

 * [Facebook](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-how-to-configure-facebook-authentication/) 
 * [Twitter](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-how-to-configure-twitter-authentication/)
 * [Microsoft](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-how-to-configure-microsoft-authentication/)
 * [Google](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-how-to-configure-google-authentication/)

Do not forget to click "Request e-mails" checkbox on Twitter App Management console.

![alt tag](/images/7.PNG?raw=true)

After you completed application registration, you need to enable Authentication on Azure portal. Open your app and go Authentication. Click 'On' and set unauthenticated option to 'No Action'.

![alt tag](/images/1.PNG?raw=true)

Facebook Permissions

![alt tag](/images/2.PNG?raw=true)

Microsoft Permissions

![alt tag](/images/5.PNG?raw=true)

Google Permissions

![alt tag](/images/3.PNG?raw=true)

Twitter Permissions

![alt tag](/images/4.PNG?raw=true)


Also, you need to re-write your App key and secret on codebehind.

![alt tag](/images/6.PNG?raw=true)


Feel free to ask questions via 'Issues'.
