namespace Avalonia.PixelColor.Utils.OpenGl
{
    public class ISFInput
    {
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public float DEFAULT { get; set; }
        public float MIN { get; set; }
        public float MAX { get; set; }
    }

    public class ISFParameters
    {
        public string CREDIT { get; set; }
        public string DESCRIPTION { get; set; }
        public string[] CATEGORIES { get; set; }
        public ISFInput[] INPUTS { get; set; }
    }
}
