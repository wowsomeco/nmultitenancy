using System.Collections.Generic;

namespace MultiTenancy {
  public class MimeTypes {
    public const string Jpeg = "image/jpeg";
    public const string Png = "image/png";

    public static string[] Images = new string[] {
      Jpeg,
      Png
    };

    public const string Pdf = "application/pdf";

    public static string[] Words = new string[] {
      "application/msword",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public static string[] Excels = new string[] {
      "application/vnd.ms-excel",
      "application/vnd.ms-excel.sheet.binary.macroEnabled.12",
      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    public static string[] Ppts = new string[] {
      "application/vnd.ms-powerpoint",
      "application/vnd.ms-powerpoint.presentation.macroEnabled.12",
      "application/vnd.openxmlformats-officedocument.presentationml.presentation"
    };

    public static List<string> OfficeDocs = ListExtensions.Combine(Excels, Words, Ppts);
  }
}