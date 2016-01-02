using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace instaPicsWebJobFavoriteDay.Model.POCO
{
    //dans la tale, un utilisateur a une image original, l'image original mais en thumb, la version en noir et blanc en taille normal, et la version en noir et blanc en thumb
    public class UserImageEntity : TableEntity
    {
        public string user { get; set; }

        public string imgOriginal { get; set; }

        public string imgOriginalThumb { get; set; }

        public string imgBN { get; set; }

        public string imgBNThumb { get; set; }
    }
}
