namespace RCL.Win
{
    public enum UiTheme
    {
        Light = 0,
        Dark = 1
    }

    public class UserUiSettings
    {
        public UiTheme Theme { get; set; } = UiTheme.Light;
        public double FontSize { get; set; } = 14.0;
        public string FontFamily { get; set; } = "Segoe UI";
    }
}

