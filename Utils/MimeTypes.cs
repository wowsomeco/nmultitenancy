using System.Collections.Generic;

namespace MultiTenancy {
  public class MimeTypes {
    public static List<string> Pdfs = new List<string> {
      "application/pdf"
    };

    public static List<string> Words = new List<string> {
      "application/msword",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public static List<string> Excels = new List<string> {
      "application/vnd.ms-excel",
      "application/vnd.ms-excel.sheet.binary.macroEnabled.12",
      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    public static List<string> Ppts = new List<string> {
      "application/vnd.ms-powerpoint",
      "application/vnd.ms-powerpoint.presentation.macroEnabled.12",
      "application/vnd.openxmlformats-officedocument.presentationml.presentation"
    };

    public static List<string> OfficeDocs = ListExtensions.Combine(Excels, Words, Ppts);
  }
}