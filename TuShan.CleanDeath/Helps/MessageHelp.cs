using HandyControl.Controls;

namespace TuShan.CleanDeath.Helps
{
    public static class MessageHelp
    {
        #region message

        public static void InfoMessageShow(string message)
        {
            Growl.Info(message, "InfoMessage");
        }
        public static void ErrorMessageShow(string message)
        {
            Growl.Error(message, "ErrorMessage");
            return;
        }
        public static void ClearMessageShow()
        {
            HandyControl.Controls.Growl.Clear("InfoMessage");
            HandyControl.Controls.Growl.Clear("ErrorMessage");
            return;
        }

        #endregion
    }
}
