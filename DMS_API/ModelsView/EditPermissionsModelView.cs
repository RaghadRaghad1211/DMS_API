﻿namespace DMS_API.ModelsView
{
    public class EditPermissionsModelView
    {
        public int SourObjId { get; set; }
        public int DestObjId { get; set; }
        //public int SourClsId { get; set; }
        //public int DestClsId { get; set; }
        public bool PerRead { get; set; }
        public bool PerWrite { get; set; }
        public bool PerManage { get; set; }
        public bool PerQR { get; set; }
    }
}
