using System.Collections.Generic;

namespace MultiTenancy {
  public class AclList {
    public string Resource { get; set; }
    public List<string> Actions { get; set; }

    public string GetAction(string name) {
      return Actions.Find(x => x.CompareStandard(name));
    }
  }

  public class AclModel {
    public List<AclList> Acls { get; set; }

    public AclList GetAclList(string resource) {
      return Acls.Find(x => x.Resource.CompareStandard(resource));
    }
  }
}