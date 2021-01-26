using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MultiTenancy {
  public class AclList {
    public string Resource { get; set; }
    public List<string> Actions { get; set; }

    public string GetAction(string name) {
      return Actions.Find(x => x.CompareStandard(name));
    }
  }

  public class AclModel {
    [Required]
    public List<AclList> Acls { get; set; }

    public AclModel() { }

    public AclModel(AclModel other) {
      Acls = other.Acls;
    }

    public AclList GetAclList(string resource) {
      return Acls.Find(x => x.Resource.CompareStandard(resource));
    }
  }
}