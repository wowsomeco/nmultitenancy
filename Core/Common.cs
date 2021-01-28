namespace MultiTenancy {
  public interface IErrorResponse {
    string Error { get; set; }
  }

  public class DeleteResponse {
    public bool Deleted { get; set; }
  }
}