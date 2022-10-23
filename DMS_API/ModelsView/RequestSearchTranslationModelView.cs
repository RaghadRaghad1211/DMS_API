﻿namespace DMS_API.ModelsView
{
    public class RequestSearchTranslationModelView
    {
        public string WordSearch { get; set; }
        public string KeySearch { get; set; } // language or key
        public int PageRows { get; set; }
        public int PageNumber { get; set; }
    }
}
