using System.Collections.Generic;

namespace DMS_API.ModelsView
{
    public class LinkParentChildModelView
    {
        public int ParentId { get; set; }
        public List<int> ChildId { get; set; }
    }
}