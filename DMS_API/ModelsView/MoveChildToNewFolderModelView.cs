﻿namespace DMS_API.ModelsView
{
    public class MoveChildToNewFolderModelView
    {
        public int CurrentParentID { get; set; }
        public int ChildID { get; set; }
        public int NewParentID { get; set; }
    }
}