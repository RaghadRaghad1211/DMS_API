using iTextSharp.text;

namespace DMS_API.Models
{
    public class PdfSettingsModel
    {
        public static readonly BaseColor BLACK = new BaseColor(0, 0, 0);
        public static readonly BaseColor RED = new BaseColor(255, 0, 0);
        public static readonly BaseColor BLUE = new BaseColor(0, 0, 255);
        public static readonly BaseColor ORANGE = new BaseColor(255, 200, 0);
        public static readonly BaseColor Other1 = new BaseColor(60, 42, 33);
        public enum FontSize
        {
            VeryMini=8,
            Mini = 10,
            VerySmall = 12,
            Small = 14,
            Medium = 16,
            Large = 18,
            Xlarg = 20,
            XXlarg = 22,
            XXXlarg=24
        }
        public static readonly string ParagraphBeforQRLine1 = "ان حفاظك على هذه الوثيقة دون ضرر يمكنك من استخدامها في الدوائر المرتبطة بهذا النظام";
        public static readonly string ParagraphBeforQRLine2 = "نؤيد صحة صدور الوثيقة الالكترونية بعد مطابقتها مع الوثيقة الورقية";
        public static readonly string ParagraphAfterQRLine1 = "عزيزي المواطن في حالة حدوث أي تلكؤ أو مشكلة في قراءة رمز الوصول السريع" + "\n" + "يرجى الاتصال على الرقم المجاني 5599";
        public static readonly string ParagraphAfterQRLine2 = "لمزيد من المعلومات عن الخدمات الحكومية الالكترونية، بالأمكان زيارة الرابط التالي" + "\n" + "https://ur.gov.iq";
        public static readonly string ParagraphFooterLine1 = "NDC of Iraq";

    }
}
