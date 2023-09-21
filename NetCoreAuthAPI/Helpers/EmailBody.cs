namespace NetCoreAuthAPI.Helpers
{
    public static class EmailBody
    {
        public static string EmailStringBody(string email, string emailToken)
        {
            return $@"<html>
            <head></head>
            <body>
            <h1>Reset Your Password</h1>
            <p>Please tap the the button below to choose a new password. </p>
            <p>URL: <a href=""http://localhost:4200/reset?email={email}&code={emailToken}"">Google</a></p>
            <p>Kind Regards</p>
            </body>
            </html>";
        }
    }
}
